using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Marvin.IDP;
using Marvin.IDP.Entities;
using Marvin.IDP.Model;
using Marvin.IDP.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace IdentityServer4.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger _logger;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private const string RecoveryCodesKey = nameof(RecoveryCodesKey);
        //private readonly IMarvinUserRepository _marvinUserRepository;
        private readonly IPersistedGrantStore _persistedGrantStore;

        private IApplicationAccountRepository _applicationAccountRepository;
        /// <summary>
        /// The claims factory.
        /// </summary>
        protected readonly IUserClaimsPrincipalFactory<User> _claimsFactory;
        private IConfiguration _configuration;


        public AccountController(
            IApplicationAccountRepository applicationAccountRepository,
            UrlEncoder urlEncoder,
            IPersistedGrantStore persistedGrantStore,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
            IEventService events,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _applicationAccountRepository = applicationAccountRepository;
            _urlEncoder = urlEncoder;
            _userManager = userManager;
            //_userManager.PasswordHasher = new MyPasswordHasher();
            _signInManager = signInManager;
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            //  _marvinUserRepository = marvinUserRepository;
            _persistedGrantStore = persistedGrantStore;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _claimsFactory = userClaimsPrincipalFactory;
            _events = events;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccount(string returnUrl)
        {
            var accounts = _applicationAccountRepository.GetAccounts().ToList();

            return Ok(accounts);
        }

        [HttpGet]
        public async Task<ActionResult<ROPCTokenResponse>> GetROPCAccessToken(string username, string password)
        {
            var returnData = new ROPCTokenResponse();

            var user = _userManager.FindByNameAsync(username).Result;

            if (user != null && !await _userManager.IsLockedOutAsync(user))
            {
                if (await _userManager.CheckPasswordAsync(user, password))
                {
                    if (await _userManager.IsEmailConfirmedAsync(user))
                    {
                        HttpClient _tokenClient = new HttpClient();

                        // Get Discovery Document
                        var discoveryDoc = await _tokenClient.GetDiscoveryDocumentAsync(_configuration.GetSection("IdentityServerUrl").Value);
                        if (discoveryDoc.IsError) throw new Exception(discoveryDoc.Error);

                        // Get Token
                        var response = await _tokenClient.RequestPasswordTokenAsync(new PasswordTokenRequest
                        {
                            Address = discoveryDoc.TokenEndpoint,

                            ClientId = "Agency_Client_Id", // default client_id
                            ClientSecret = "Agency_Client_Secret", // default client_secret

                            UserName = username,
                            Password = password,

                            Scope = "openid profile roles tourmanagementapi", // default scopes
                        });
                        if (!response.IsError)
                        {
                            // Extract Header & Claims From AccessToken
                            var parts = response.AccessToken.Split('.');
                            var header = JsonConvert.DeserializeObject<ROPCTokenHeader>(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(parts[0]))).ToString());
                            var claims = JsonConvert.DeserializeObject<ROPCTokenClaims>(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(parts[1]))).ToString());

                            // Store Data                            
                            returnData.TokenData.AccessToken = response.AccessToken;
                            returnData.TokenData.TokenType = response.TokenType;
                            returnData.TokenData.ExpiresIn = response.ExpiresIn;
                            returnData.TokenData.RefreshToken = response.RefreshToken;
                            returnData.TokenData.IdentityToken = response.IdentityToken;
                            returnData.TokenData.header = header;
                            if (claims.super_admin == null)
                            {
                                claims.super_admin = "false";
                            }
                            returnData.TokenData.claims = claims;

                        }
                        else
                        {
                            returnData.IsError = true;
                            returnData.Error = response.ErrorDescription;
                        }
                    }
                    else
                    {
                        returnData.IsError = true;
                        returnData.Error = "Email is not confirmed !!!";
                    }
                }
                else
                {
                    returnData.IsError = true;
                    returnData.Error = "Username and Password not match !!!";
                }
            }
            else
            {
                returnData.IsError = true;
                returnData.Error = "Email not found !!!";
            }
            return Ok(returnData);
        }

        [HttpGet]

        public async Task<bool> CheckCurrentPassword(string username, string password)
        {
            var user = _userManager.FindByNameAsync(username).Result;
            bool isCurrentPasswordMatched = false;
            if (await _userManager.CheckPasswordAsync(user, password))
            {
                isCurrentPasswordMatched = true;
            }
            return isCurrentPasswordMatched;
        }
        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(model.Username).Result;

                if (user != null && !await _userManager.IsLockedOutAsync(user))
                {
                    if (await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError("", "Email is not confirmed");
                            return View();
                        }
                        //if (!user.UserType.Equals("Administrator") && !user.UserType.Equals("User"))
                        //{
                        //    ModelState.AddModelError("", "Unauthenticated User");
                        //    return View();
                        //}

                        await _userManager.ResetAccessFailedCountAsync(user);

                        var principal = await _claimsFactory.CreateAsync(user);

                        //var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: false);
                        //if (result.RequiresTwoFactor)
                        //{
                        //    TempData["UserName"] = model.Username;
                        //    TempData["RedirectUrl"] = model.ReturnUrl;
                        //    return RedirectToAction(nameof(AuthenticatorType));
                        //}

                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                        return Redirect(model.ReturnUrl);
                    }

                    await _userManager.AccessFailedAsync(user);

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        // email user, notifying them of lockout
                    }
                }

                ModelState.AddModelError("", "Invalid UserName or Password");
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AuthenticatorType()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AuthenticatorType(string authenticatorType)
        //{
        //    if (authenticatorType == "1")
        //    {
        //        return View("TwoFactor");
        //    }
        //    else if (authenticatorType == "2")
        //    {
        //        return View("EnableAuthenticator");
        //    }
        //    else
        //    {
        //        return View();
        //    }

        //}

        [HttpGet]
        public async Task<IActionResult> TwoFactor()
        {
            //var user = await _userManager.GetUserAsync(User);GetTwoFactorAuthenticationUserAsync
            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            // System.IO.File.WriteAllText("email2sv.txt", token);

            var htmlContent = "<table width =\"350\" align=\"center\">" +
                        "<tbody><tr><td style =\"background-color:#800080; text-align: center;\">" +
                        "<img src=\"https://drive.google.com/file/d/1dRv1gmAczFMifzdcSdVbDuuGI6VuFQgH/view \" width=\"150\" height=\"68\"/></td></tr> " +
                        "<tr><td style=\"text-align: left;\">" +
                        "<p style=\"padding-left: 30px;\"><strong>Dear " + user.UserName + ",</strong></p>" +
                        "<p style=\"padding-left: 30px;\">You have requested online access from our website.</p>" +
                        "<p style=\"padding-left: 30px;\"><strong>Your time-sensitive One-time Passcode is <br><code style=\"color:red; font-size:20px;\">" + token + "</code></strong></p>" +
                        "<p style=\"padding-left: 30px; color:red\">Please enter the code into the from for which you have requested access. Thank you for utilizing our service.</p>" +
                        "<p style=\"padding-left: 30px;\">We love hearing from you!</p>" +
                        "<p style=\"padding-left: 30px;\">Have any question? Please checkout our <span style=\"color: #800080;\">help center</span>.</p>" +
                        "<p style=\"padding-left: 30px;\">Thanks</p>" +
                        "<p style=\"padding-left: 30px;\"><strong>Agencyeasy</strong></p> " +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>";


            var client = new SendGridClient(_configuration.GetSection("Client").Value);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.GetSection("MailFrom").Value), new EmailAddress(user.Email),
                "2 Factor Authentication", token, htmlContent);
            var response = client.SendEmailAsync(msg);
            await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(user.Id, "Email"));

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TwoFactor(TwoFactorModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "You login request has expired, please start over");
                return View("Login");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));

                if (user != null)
                {
                    var isValid = await _userManager.VerifyTwoFactorTokenAsync(user,
                        result.Principal.FindFirstValue("amr"), model.Token);

                    if (isValid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

                        var claimsPrincipal = await _claimsFactory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);


                        return Redirect(TempData["RedirectUrl"].ToString());
                        TempData.Keep();
                        // return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", "Invalid token");
                    return View();
                }

                ModelState.AddModelError("", "Invalid Request");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator(bool isValidatorCheck = true, string returnUrl = "", string email = "")
        {
            User user;
            //var user = await _userManager.GetUserAsync(User);
            if (!String.IsNullOrEmpty(email))
            {
                user = await _userManager.FindByNameAsync(email);
                //await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
                TempData.Keep();
                //await _signInManager.RefreshSignInAsync(user);
            }

            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (isValidatorCheck)
            {
                var validProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);

                if (validProviders.Contains(_userManager.Options.Tokens.AuthenticatorTokenProvider))
                {
                    return RedirectToAction(nameof(LoginWith2fa));
                }
                else
                {
                    var model = new EnableAuthenticatorViewModel();
                    await LoadSharedKeyAndQrCodeUriAsync(user, model);
                    return View(model);
                }
            }
            else
            {
                TempData["RedirectUrl"] = returnUrl;
                TempData["UserName"] = email;
                await _userManager.ResetAuthenticatorKeyAsync(user);
                var model = new EnableAuthenticatorViewModel();
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {

            User user;

            user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();
            //var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            TempData.Keep();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Invalid verification code.");
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();
            var codes = new ShowRecoveryCodesViewModel();
            codes.RecoveryCodes = recoveryCodes.ToList();
            return View("ShowRecoveryCodes", codes);
        }

        [HttpPost]
        public async Task<IActionResult> Disable2FA(bool value, string email)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (!value)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                //await _userManager.RemoveAuthenticationTokenAsync(user, "[AspNetUserStore]", _userManager.Options.Tokens.AuthenticatorTokenProvider);
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, value);

            //return new ResultVM
            //{
            //    Status = result.Succeeded ? Status.Success : Status.Error,
            //    Message = result.Succeeded ? "2FA has been successfully disabled" : $"Failed to disable 2FA {result.Errors.FirstOrDefault()?.Description}"
            //};
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa()
        {
            var model = new LoginWith2faViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);

            if (is2faTokenValid)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);

                await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                var claimsPrincipal = await _claimsFactory.CreateAsync(user);
                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                var returnURL = TempData["RedirectUrl"].ToString();
                TempData.Keep();

                return Redirect(returnURL);
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError("Code", "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode()
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return Redirect("GenerateRecoveryCodesWarning");
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> grants()
        {
            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            var claimsPrincipal = await _claimsFactory.CreateAsync(user);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

            var returnURL = TempData["RedirectUrl"].ToString();
            TempData.Keep();
            return Redirect(returnURL);
        }
        private async Task LoadSharedKeyAndQrCodeUriAsync(User user, EnableAuthenticatorViewModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);
            model.AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("VNC"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
        private ClaimsPrincipal Store2FA(string userId, string provider)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", userId),
                new Claim("amr", provider)
            }, IdentityConstants.TwoFactorUserIdScheme);

            return new ClaimsPrincipal(identity);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodesWarning()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();
            var codes = new ShowRecoveryCodesViewModel();
            codes.RecoveryCodes = recoveryCodes.ToList();
            return View("ShowRecoveryCodes", codes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail()
        {
            //var user = await _userManager.GetUserAsync(User);GetTwoFactorAuthenticationUserAsync
            var user = await _userManager.FindByNameAsync(TempData["UserName"].ToString());
            TempData.Keep();

            string[] codesArray = (string[])TempData[RecoveryCodesKey];
            string codes = "<table width =\"350\" align=\"center\">" +
                        "<tbody><tr><td style =\"background-color:#800080; text-align: center;\">" +
                        "<img src=\"https://drive.google.com/file/d/1dRv1gmAczFMifzdcSdVbDuuGI6VuFQgH/view \" width=\"150\" height=\"68\"/></td></tr> " +
                        "<tr><td style=\"text-align: left;\">" +
                        "<p style=\"padding-left: 30px;\">Please keep your recovery codes safe.</p>" +
                        "<p style=\"padding-left: 30px;\">Each can be used only once.</p>" +
                        "<p style=\"padding-left: 30px;\">";

            string codesHTML = "<table width =\"350\" align=\"center\">" +
                        "<tbody><tr><td style =\"background-color:#800080; text-align: center;\">" +
                        "<img src=\"https://drive.google.com/file/d/1dRv1gmAczFMifzdcSdVbDuuGI6VuFQgH/view \" width=\"150\" height=\"68\"/></td></tr> " +
                        "<tr><td style=\"text-align: left;\">" +
                        "<p style=\"padding-left: 30px;\">Please keep your recovery codes safe.</p>" +
                        "<p style=\"padding-left: 30px;\">Each can be used only once.</p>" +
                        "<p style=\"padding-left: 30px;\">";


            foreach (var code in codesArray)
            {
                codes = codes + code + " , ";

                codesHTML = codesHTML + "<br /><code style=\"color:red; padding-left: 30px;\">" + code + "</code>";

            }
            codes = codes +
                       "<br /></p>" +
                       "<p style=\"padding-left: 30px;\">We love hearing from you!</p>" +
                       "<p style=\"padding-left: 30px;\">Have any question? Please checkout our <span style=\"color: #800080;\">help center</span>.</p>" +
                       "<p style=\"padding-left: 30px;\">Thanks</p>" +
                       "<p style=\"padding-left: 30px;\"><strong>Agencyeasy</strong></p> " +
                       "</td>" +
                       "</tr>" +
                       "</tbody>" +
                       "</table>";

            codesHTML = codesHTML +
                        "<br /></p>" +
                        "<p style=\"padding-left: 30px;\">We love hearing from you!</p>" +
                        "<p style=\"padding-left: 30px;\">Have any question? Please checkout our <span style=\"color: #800080;\">help center</span>.</p>" +
                        "<p style=\"padding-left: 30px;\">Thanks</p>" +
                        "<p style=\"padding-left: 30px;\"><strong>Agencyeasy </strong></p> " +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>";

            var client = new SendGridClient(_configuration.GetSection("Client").Value);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.GetSection("MailFrom").Value), new EmailAddress(user.UserName),
                "Recovery Codes", codes, codesHTML);
            var response = await client.SendEmailAsync(msg);

            var recoverycodes = new ShowRecoveryCodesViewModel();
            recoverycodes.RecoveryCodes = codesArray.ToList();
            return View("ShowRecoveryCodes", recoverycodes);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserModel modelUser)
        {
            try
            {
                User user;
                if (String.IsNullOrEmpty(modelUser.ID))
                {
                    var isUserExist = await _userManager.FindByNameAsync(modelUser.Email);
                    if (isUserExist != null)
                    {

                        if (isUserExist.Database != null || isUserExist.Database != "" && isUserExist.Database != modelUser.Database)
                        {
                            isUserExist.Database = isUserExist.Database + "|" + modelUser.Database;
                        }
                        else
                        {
                            isUserExist.Database = modelUser.Database;
                        }

                        await _userManager.UpdateAsync(isUserExist);

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(isUserExist);
                        var callbackUrl = _configuration.GetSection("FrontendUrl").Value;

                        var confirmButton = "<table width =\"350\" align=\"center\">" +
                        "<tbody><tr><td style =\"background-color:#800080; text-align: center;\">" +
                        "<img src=\"https://drive.google.com/file/d/1dRv1gmAczFMifzdcSdVbDuuGI6VuFQgH/view \" width=\"150\" height=\"68\"/></td></tr> " +
                        "<tr><td style=\"text-align: left;\">" +
                        "<p style=\"padding-left: 30px;\"><strong> " + modelUser.AdminName + " </strong> is inviting you to access the<strong> Agencyeasy.</strong></p> " +
                        "<p style=\"padding-left: 30px;\">And start looking at business transformation reports.</p>" +
                        "<p style=\"padding-left: 30px;\">Please accept your invite by clicking below.</p>" +
                        "<br />" +
                        "<table class=\"buttonwrapper\" style=\"background-color:#800080;\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td class=\"button\" style=\"text-align: center; font-size: 18px; font-family: sans-serif; font-weight: bold; padding: 0 30px 0 30px;\" height=\"45\">" +
                        "<a style=\"color: #fff; text-decoration: none;\" href ='" + callbackUrl + "'>Accept Invitation</a></td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<br />" +
                        "<p style=\"padding-left: 30px;\">We love hearing from you!</p>" +
                        "<p style=\"padding-left: 30px;\">Have any question? Please checkout our <span style=\"color: #800080;\">help center</span>.</p>" +
                        "<p style=\"padding-left: 30px;\">Thanks</p>" +
                        "<p style=\"padding-left: 30px;\"><strong>Agencyeasy</strong></p> " +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>";

                        var client = new SendGridClient(_configuration.GetSection("Client").Value);
                        var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.GetSection("MailFrom").Value), new EmailAddress(isUserExist.Email),
                            "Confirm Email", callbackUrl, confirmButton);
                        var response = client.SendEmailAsync(msg);

                        return Ok(isUserExist.Id);
                    }
                    else
                    {
                        //Create
                        user = new Marvin.IDP.Entities.User();
                        //user.PasswordHash = "Vadera@2019";
                        user.UserName = modelUser.Email;
                        user.Email = modelUser.Email;
                        user.NormalizedEmail = modelUser.Email;
                        user.NormalizedUserName = modelUser.Email;
                        user.IsActive = modelUser.IsActive;
                        user.Database = modelUser.Database;
                        //List<UserClaim> userClaims = new List<UserClaim>();

                        ////Add Basic claims
                        //userClaims.Add(new UserClaim("given_name", modelUser.FirstName));
                        //userClaims.Add(new UserClaim("given_lname", modelUser.LastName));
                        //userClaims.Add(new UserClaim(JwtClaimTypes.Email, modelUser.Email));

                        //// Add the claims to the user object
                        //user.Claims = userClaims;

                        //await _userManager.CreateAsync(user, "Vadera@2019");
                        var defaultPassword = _configuration["DefaultUserPassword"];
                        if (string.IsNullOrEmpty(defaultPassword))
                        {
                            throw new InvalidOperationException("DefaultUserPassword is not configured in appsettings.json");
                        }
                        var result = await _userManager.CreateAsync(user, defaultPassword);

                        // Adding User Claims

                        Claim firstNameClaim;
                        firstNameClaim = new Claim("given_name", modelUser.FirstName);
                        await _userManager.AddClaimAsync(user, firstNameClaim);

                        Claim lastNameClaim;
                        lastNameClaim = new Claim("given_lname", modelUser.LastName);
                        await _userManager.AddClaimAsync(user, lastNameClaim);

                        if (result.Succeeded)
                        {
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var callbackUrl = Url.Action(
                               "ConfirmEmail", "Account",
                               new { userId = user.Id, code = code, returnUrl = modelUser.ReturnURL },
                               protocol: Request.Scheme);

                            //  var callbackUrl = "https://localhost:3000/agency";

                            var confirmButton = "<table width =\"350\" align=\"center\">" +
                         "<tbody><tr><td style =\"background-color:#5D9CEC; text-align: center;\">" +
                         "<img src=\"ei-logo.png\" width=\"150\" height=\"68\"/></td></tr> " +
                         "<tr><td style=\"text-align: left;\">" +
                         "<p style=\"padding-left: 30px;\"><strong> " + modelUser.AdminName + " </strong> is inviting you to access the<strong>Agencyeasy.</strong></p> " +
                         "<p style=\"padding-left: 30px;\">And start looking at business transformation reports.</p>" +
                         "<p style=\"padding-left: 30px;\">your password => <code>" + Password + "</code></p>" +
                         "<p style=\"padding-left: 30px;\">Please accept your invite by clicking below.</p>" +
                         "<br />" +
                         "<table class=\"buttonwrapper\" style=\"background-color:#800080;\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\">" +
                         "<tbody>" +
                         "<tr>" +
                         "<td class=\"button\" style=\"background-color:#5D9CEC; text -align: center; font-size: 18px; font-family: sans-serif; font-weight: bold; padding: 0 30px 0 30px;\" height=\"45\">" +
                         "<a style=\"color: #fff; text-decoration: none;\" href ='" + callbackUrl + "'>Confirm my Email</a></td>" +
                         "</tr>" +
                         // "<tr>" +
                         //"<td class=\"button\" style=\"text-align: center; font-size: 18px; font-family: sans-serif; font-weight: bold; padding: 0 30px 0 30px;\" height=\"45\">" +
                         //"<a style=\"color: #fff; text-decoration: none;\" href ='" + callbackUrl1 + "'>Let's Go to the Dashboard</a></td>" +
                         //"</tr>" +
                         "</tbody>" +
                         "</table>" +
                         "<br />" +
                         "<p style=\"padding-left: 30px;\">We love hearing from you!</p>" +
                         "<p style=\"padding-left: 30px;\">Have any question? Please checkout our <span style=\"color: #800080;\">help center</span>.</p>" +
                         "<p style=\"padding-left: 30px;\">Thanks</p>" +
                         "<p style=\"padding-left: 30px;\"><strong>Agencyeasy </strong></p> " +
                         "</td>" +
                         "</tr>" +
                         "</tbody>" +
                         "</table>";

                            var client = new SendGridClient(_configuration.GetSection("Client").Value);
                            var msg = MailHelper.CreateSingleEmail(new EmailAddress(_configuration.GetSection("MailFrom").Value), new EmailAddress(user.UserName),
                                "Confirm Email", callbackUrl, confirmButton);
                            var response = client.SendEmailAsync(msg);

                            return Ok(user.Id);
                        }
                    }
                }
                else
                {
                    // Update
                    user = await _userManager.FindByIdAsync(modelUser.ID);
                }

                // Only database will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.Database))
                {
                    user.Database = modelUser.Database;
                }
                // Only Birthday will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.Birthday))
                {
                    string iDate = modelUser.Birthday;
                    user.Birthday = DateTime.ParseExact(iDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                // Only Birthplace will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.Birthplace))
                {
                    user.Birthplace = modelUser.Birthplace;
                }

                // Only Gender will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.Gender))
                {
                    user.Gender = modelUser.Gender;
                }

                // Only Occupation will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.Occupation))
                {
                    user.Occupation = modelUser.Occupation;
                }

                // Only PhoneNumber will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.PhoneNumber))
                {
                    user.PhoneNumber = modelUser.PhoneNumber;
                }

                // Only LivesIn will be updated for Create and Update scenarios
                if (!String.IsNullOrEmpty(modelUser.LivesIn))
                {
                    user.LivesIn = modelUser.LivesIn;
                }
                //// Only Password will be updated for Create and Update scenarios
                //if (!String.IsNullOrEmpty(modelUser.Password))
                //{
                //    user.Password = modelUser.Password;
                //}

                #region Update Claims
                //Add Basic claims
                var claims = await _claimsFactory.CreateAsync(user);
                if (!String.IsNullOrEmpty(modelUser.FirstName))
                {
                    var claimName = claims.Claims.FirstOrDefault(x => x.Type == "given_name");
                    if (claimName != null)
                    {
                        Claim newClaim = new Claim("given_name", modelUser.FirstName);

                        await _userManager.ReplaceClaimAsync(user, claimName, newClaim);
                    }
                    else
                    {
                        Claim firstNameClaim;
                        firstNameClaim = new Claim("given_name", modelUser.FirstName);
                        await _userManager.AddClaimAsync(user, firstNameClaim);
                    }
                }
                if (!String.IsNullOrEmpty(modelUser.LastName))
                {
                    var claimLName = claims.Claims.FirstOrDefault(x => x.Type == "given_lname");
                    if (claimLName != null)
                    {
                        Claim newClaim = new Claim("given_lname", modelUser.LastName);

                        await _userManager.ReplaceClaimAsync(user, claimLName, newClaim);
                    }
                    else
                    {
                        Claim lastNameClaim;
                        lastNameClaim = new Claim("given_lname", modelUser.LastName);
                        await _userManager.AddClaimAsync(user, lastNameClaim);
                    }
                }
                if (!String.IsNullOrEmpty(modelUser.ImageUrl))
                {
                    var imageClaim = claims.Claims.FirstOrDefault(x => x.Type == "image_url");
                    if (imageClaim != null)
                    {
                        Claim newClaim = new Claim("image_url", modelUser.ImageUrl);

                        await _userManager.ReplaceClaimAsync(user, imageClaim, newClaim);
                    }
                    else
                    {
                        Claim ImageUrlClaim;
                        ImageUrlClaim = new Claim("image_url", modelUser.ImageUrl);
                        await _userManager.AddClaimAsync(user, ImageUrlClaim);
                    }
                }
                #endregion

                if (String.IsNullOrEmpty(modelUser.ID))
                {
                    //user.SubjectId = Guid.NewGuid().ToString();
                    //// User is not present in ID and the call is made for creating User
                    //_marvinUserRepository.AddUser(user);
                }
                else
                {
                    // Update user object with claims
                    await _userManager.UpdateAsync(user);
                }
                return Ok(user.Id);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw ex;
            }

        }

        [HttpPost]
        public async Task<IActionResult> InviteUserMail(bool NewUser, string Email, string AdminName, string CompanyName, Guid CompanyId, Guid Code, string Password, string CampaignName, string FromWhom, string ReturnURL, string DashboardLogo, string RequestedUrl, bool IsSendEmailToSuperAdmin)
        {
            var user = await _userManager.FindByNameAsync(Email);
            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(code);
            //code = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);

            var callbackUrl = "";
            //var callbackUrl1 = "";
            var confirmButton = "";
            string projectAccessText = string.Empty;
            string subject = string.Empty;

            
            if (IsSendEmailToSuperAdmin)
            {
                await SendNotificationEmailToSuperAdmin(NewUser, Email, AdminName, CompanyName, CompanyId, Code, Password, FromWhom, ReturnURL, RequestedUrl, DashboardLogo);
                //var res = await SendEmailToSuperAdmin(NewUser, Email, AdminName, CompanyName, CompanyId, Code, Password, FromWhom, ReturnURL, RequestedUrl, DashboardLogo);
            }

            if (!String.IsNullOrEmpty(CampaignName))
            {
                projectAccessText = "<tr style=\"border-collapse:collapse;\"> <td class=\"es-m-txt-l\" align=\"left\" style=\"padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px;\"> <p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px;\"> <strong> " + AdminName + " </strong> is inviting you to access the <strong>" + CompanyName + " for project " + CampaignName + ".</strong></p> </td> </tr>";
            }

            ReturnURL = RequestedUrl + "/" + _configuration.GetSection("ReturnUrlEmailVerification").Value;
            if (NewUser)
            {
                callbackUrl = _configuration.GetSection("ApiUrl").Value + "aspusers/ConfirmEmail?id=" + user.Id + "&code=" + Code + "&returnUrl=" + ReturnURL;
                //callbackUrl = Url.Action(
                //             "ConfirmEmail", "Account",
                //             new { userId = user.Id, code = code, returnUrl = ReturnURL },
                //             protocol: Request.Scheme);

                //callbackUrl = "https://localhost:3000/agency";

                confirmButton = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout:fixed; background-color:#f9f9f9\" id=\"bodyTable\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                    "<table border =\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign=\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:Montserrat, sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text\" hideOnMobile></a>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableCard\" style=\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height=\"3\">&nbsp;</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                    "<a href =\" #\" style=\"text-decoration:none\" target=\"_blank\">" +
                    "<img border=\"0\" src='" + DashboardLogo + "' alt style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                    "</a>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                    "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 20px\">Confirm Your Email Address To Get Started With " + CompanyName + "</h2>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style =\" padding-left:20px;padding-right:20px;\" align=\"center\" valign=\"top\" class=\"containtTable ui-sortable\">" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">To complete you profile and start your journey with " + CompanyName + ", you'll need to confirm your email address by clicking button below</p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href ='" + callbackUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\">Confirm Email</a>" +
                    projectAccessText +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\" 0\" cellspacing=\" 0\" width= \"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> Your Email => " + Email + "</p>" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> Your Password => " + Password + "</p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                    "<tbody>" +
                    "</tbody>" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 20px;\" align= \"center\" valign=\" top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you didn't request this email, there's nothing to worry about.you can safely ignore it. </p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px height=20\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>";

                subject = "Please Confirm Your Email Address";
            }
            else
            {
                callbackUrl = ReturnURL;

                confirmButton = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout:fixed; background-color:#f9f9f9\" id=\"bodyTable\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                    "<table border =\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign=\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:Montserrat, sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text\" hideOnMobile></a>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableCard\" style=\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height=\"3\">&nbsp;</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                    "<a href =\" #\" style=\"text-decoration:none\" target=\"_blank\">" +
                    "<img border=\"0\" src='" + DashboardLogo + "' alt style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                    "</a>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                    "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 20px\">Welcome to <i> " + CompanyName + "</h2>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style =\" padding-left:20px;padding-right:20px;\" align=\"center\" valign=\"top\" class=\"containtTable ui-sortable\">" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">" + AdminName + " has invited you to join the " + CampaignName + " project on " + CompanyName + ".</p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href ='" + callbackUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\">Accept Invite</a>" +
                    "</td>" +
                    "</tr>" +
                    "<tr style=\"border-collapse:collapse;\"> <td class=\"es-m-txt-l\" align=\"left\" style=\"padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px;\"> <p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px;\">What's Agency Easy? <br/>Agency Easy is an easy way to visualize actionable insights across different Marketing Tools. It helps Digital Marketing Agencies automate reporting for their SEO, Social Media, and Search Ad campaigns.</p> </td> </tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\" 0\" cellspacing=\" 0\" width= \"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                    "<tbody>" +
                    "</tbody>" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style =\"padding-bottom:20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 20px;\" align= \"center\" valign=\" top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you didn't request this email, there's nothing to worry about.you can safely ignore it. </p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px height=20\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>";

                subject = "You have been invited to join the " + CampaignName + " project";
            }


            var client = new SendGridClient(_configuration.GetSection("Client").Value);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(FromWhom), new EmailAddress(user.UserName),
              subject, callbackUrl, confirmButton);
            var response = client.SendEmailAsync(msg);

            return Ok(user.Id);
        }

                    


        [HttpPost]
        public async Task<IActionResult> ReSendUserInvite(string Email, string AdminName, string CompanyName, Guid Code, string Password, string FromWhom, string ReturnURL, string DashboardLogo, string RequestedUrl)
        {
            var user = await _userManager.FindByNameAsync(Email);
           
            var callbackUrl = "";
           
            var confirmButton = "";
            string projectAccessText = string.Empty;
            string subject = string.Empty;


            if (!String.IsNullOrEmpty(CompanyName))
            {
                projectAccessText = "<tr style=\"border-collapse:collapse;\"> <td class=\"es-m-txt-l\" align=\"left\" style=\"padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px;\"> <p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px;\"> <strong> " + AdminName + " </strong> is inviting you to access the <strong>" + CompanyName + ".</strong></p> </td> </tr>";
            }

            ReturnURL = RequestedUrl + "/" + _configuration.GetSection("ReturnUrlEmailVerification").Value;
            if (user != null  && !user.EmailConfirmed)
            {
                callbackUrl = _configuration.GetSection("ApiUrl").Value + "aspusers/ConfirmEmail?id=" + user.Id + "&code=" + Code + "&returnUrl=" + ReturnURL;
                //callbackUrl = Url.Action(
                //             "ConfirmEmail", "Account",
                //             new { userId = user.Id, code = code, returnUrl = ReturnURL },
                //             protocol: Request.Scheme);

                //callbackUrl = "https://localhost:3000/agency";

                confirmButton = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout:fixed; background-color:#f9f9f9\" id=\"bodyTable\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                    "<table border =\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign=\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:Montserrat, sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text\" hideOnMobile></a>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td align=\"center\" valign=\"top\">" +
                    "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableCard\" style=\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height=\"3\">&nbsp;</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                    "<a href =\" #\" style=\"text-decoration:none\" target=\"_blank\">" +
                    "<img border=\"0\" src='" + DashboardLogo + "' alt style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                    "</a>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                    "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 20px\">Confirm Your Email Address To Get Started With " + CompanyName + "</h2>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style =\" padding-left:20px;padding-right:20px;\" align=\"center\" valign=\"top\" class=\"containtTable ui-sortable\">" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">To complete you profile and start your journey with " + CompanyName + ", you'll need to confirm your email address by clicking button below</p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href ='" + callbackUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\">Confirm Email</a>" +
                    projectAccessText +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\" 0\" cellspacing=\" 0\" width= \"100%\" class=\"tableButton\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> Your Email => " + Email + "</p>" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> Your Password => " + Password + "</p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                    "<tbody>" +
                    "</tbody>" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"padding-bottom: 20px;\" align= \"center\" valign=\" top\" class=\"description\">" +
                    "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you didn't request this email, there's nothing to worry about.you can safely ignore it. </p>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px height=20\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                    "<tbody>" +
                    "<tr>" +
                    "<td style = \"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</td>" +
                    "</tr>" +
                    "</tbody>" +
                    "</table>";

                subject = "Please Confirm Your Email Address";
            }
          
            var client = new SendGridClient(_configuration.GetSection("Client").Value);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress(FromWhom), new EmailAddress(user.UserName),
              subject, callbackUrl, confirmButton);
            var response = client.SendEmailAsync(msg);

            return Ok(user.Id);
        }



        public async Task<IActionResult> SendEmailToSuperAdmin(bool NewUser, string Email, string AdminName, string CompanyName, Guid CompanyId, Guid Code, string Password, string FromWhom, string ReturnURL, string RequestedUrl, string DashboardLogo)
        {
            var superAdmin = await _userManager.FindByNameAsync(_configuration["SuperAdminEmail"]);
            var user = await _userManager.FindByNameAsync(Email);
            var returnUrl = RequestedUrl + "/" + _configuration.GetSection("ReturnUrlSuperAdminAgencyVerification").Value;
            var callbackUrl = "";
            var confirmButton = "";

            if (NewUser)
            {
                callbackUrl = _configuration.GetSection("ApiUrl").Value + "companys/ActivateAgency?id=" + CompanyId + "&code=" + Code + "&returnUrl=" + returnUrl + "&requestedUrl=" + RequestedUrl;

                //callbackUrl = "https://localhost:3000/agency";

                confirmButton = "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" style =\"table-layout:fixed;background-color:#f9f9f9\" id =\"bodyTable\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style =\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign =\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:'Montserrat', sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text hideOnMobile\"></a>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableCard\" style =\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\" > " +
                "<tbody>" +
                "<tr>" +
                "<td style=\"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height =\"3\" > &nbsp;</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                "<a href=\"#\" style=\"text-decoration:none\" target =\"_blank\">" +
                "<img border=\"0\" src='" + DashboardLogo + "' style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                "</a>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 0px;margin-top: 30px;\">New Agency Activation Request</h2>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-left:20px;padding-right:20px\" align =\"center\" valign =\"top\" class=\"containtTable ui-sortable\"> " +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<h3 class=\"text\" style =\"color:#000;font-family:'Montserrat', sans-serif;font-size:22px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 10px\"> Congratulations! " + user.FName + " " + user.LName + " has signed up on " + CompanyName + ".</h3></tr>" +
                "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">User Email = " + Email + "</p></td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href ='" + callbackUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\">Activate Agency</a>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you do not want to approve, you can safely ignore this email. </p>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"20\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>";
            }
            else
            {
                callbackUrl = returnUrl;

                confirmButton = "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" style =\"table-layout:fixed;background-color:#f9f9f9\" id =\"bodyTable\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style =\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign =\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:'Montserrat', sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text hideOnMobile\"></a>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableCard\" style =\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\" > " +
                "<tbody>" +
                "<tr>" +
                "<td style=\"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height =\"3\" > &nbsp;</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                "<a href=\"#\" style=\"text-decoration:none\" target =\"_blank\">" +
                "<img border=\"0\" src='" + DashboardLogo + "' style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                "</a>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 0px;margin-top: 30px;\">New Agency Activation Request</h2>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-left:20px;padding-right:20px\" align =\"center\" valign =\"top\" class=\"containtTable ui-sortable\"> " +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<h3 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:22px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 10px\">Congratulations! " + CompanyName + " has signed up on Agencyeasy.</h3></tr>" +
                "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">User Email = " + Email + "</p></td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href ='" + callbackUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\">Activate Agency</a>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you do not want to approve, you can safely ignore this email. </p>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"20\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>";
            }

            var client = new SendGridClient(_configuration.GetSection("Client").Value);

            List<EmailAddress> listOfEmail = new List<EmailAddress>();

            listOfEmail.Add(new EmailAddress(superAdmin.UserName));

            var notificationEmails = _configuration.GetSection("NotificationEmails").Value;
            if (!string.IsNullOrEmpty(notificationEmails))
            {
                foreach (var email in notificationEmails.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        listOfEmail.Add(new EmailAddress(email.Trim()));
                    }
                }
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(FromWhom), listOfEmail,
                user.FName + " " + user.LName + "  requesting you to give access of " + CompanyName, callbackUrl, confirmButton);

            var response = client.SendEmailAsync(msg);

            return Ok(user.Id);
        }

        public async Task<IActionResult> SendNotificationEmailToSuperAdmin(bool NewUser, string Email, string AdminName, string CompanyName, Guid CompanyId, Guid Code, string Password, string FromWhom, string ReturnURL, string RequestedUrl, string DashboardLogo)
        {
            var superAdmin = await _userManager.FindByNameAsync(_configuration["SuperAdminEmail"]);
            var user = await _userManager.FindByNameAsync(Email);
             var callbackUrl = string.Empty;
            var emailContent = string.Empty;

            if (NewUser)
            {                
                emailContent = "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" style =\"table-layout:fixed;background-color:#f9f9f9\" id =\"bodyTable\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style =\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign =\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:'Montserrat', sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text hideOnMobile\"></a>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableCard\" style =\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\" > " +
                "<tbody>" +
                "<tr>" +
                "<td style=\"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height =\"3\" > &nbsp;</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                "<a href=\"#\" style=\"text-decoration:none\" target =\"_blank\">" +
                "<img border=\"0\" src='" + DashboardLogo + "' style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                "</a>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 0px;margin-top: 30px;\">New Agency Registered</h2>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"padding-left:20px;padding-right:20px\" align =\"center\" valign =\"top\" class=\"containtTable ui-sortable\"> " +
                "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<h3 class=\"text\" style =\"color:#000;font-family:'Montserrat', sans-serif;font-size:22px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 10px\"> Congratulations! " + user.FName + " " + user.LName + " has signed up on " + CompanyName + ".</h3></tr>" +
                "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">User Email = " + Email + "</p></td>" +
                "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">Password = " + Password + "</p></td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                "<tbody>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                "<tbody>" +               
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"20\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>";
            }
           
            var client = new SendGridClient(_configuration.GetSection("Client").Value);

            List<EmailAddress> listOfEmail = new List<EmailAddress>();

            listOfEmail.Add(new EmailAddress(superAdmin.UserName));

            var notificationEmails = _configuration.GetSection("NotificationEmails").Value;
            if (!string.IsNullOrEmpty(notificationEmails))
            {
                foreach (var email in notificationEmails.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        listOfEmail.Add(new EmailAddress(email.Trim()));
                    }
                }
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(FromWhom), listOfEmail,
                "New Agency Registered: "+user.FName + " " + user.LName + "  has signed up on " + CompanyName, callbackUrl, emailContent);

            var response = client.SendEmailAsync(msg);

            return Ok(user.Id);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmEmail(string userId, string code, string returnUrl)
        {
            if (userId == null || code == null)
            {
                return Ok("Error");
            }
            var codeDecodedBytes = WebEncoders.Base64UrlDecode(code);
            code = Encoding.UTF8.GetString(codeDecodedBytes);

            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                ConfirmEmailViewModel model = new ConfirmEmailViewModel();
                model.RetutnUrl = returnUrl;
                return Ok(returnUrl);
            }
            return Ok(returnUrl);
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<bool> SendEmailForForgotPassword(string email, string baseUrl, string companyLogo)
        {
            var retVal = false;
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var fromWhom = string.Empty;
                    var hostedUrl = _configuration["ApiUrl"];

                    var aspUserclient = new RestClient(hostedUrl + "aspusers/" + user.Id);
                    var requestGet = new RestRequest();
                    var responseAspUser = aspUserclient.Get(requestGet);

                    var aspUser = JsonConvert.DeserializeObject<dynamic>(responseAspUser.Content);


                    var emailWhitelabelClient = new RestClient(hostedUrl + "emailwhitelabels");

                    var requestEmailWhiteLabel = new RestRequest("GetVerifyDomainByCompanyID", Method.GET);
                    requestEmailWhiteLabel.AddParameter("CompanyID", new Guid(Convert.ToString(aspUser.companyID)));
                    var responsePost = emailWhitelabelClient.Get(requestEmailWhiteLabel);

                    var emailWhitelabel = JsonConvert.DeserializeObject<dynamic>(responsePost.Content);


                    if (emailWhitelabel.Count > 0)
                    {
                        fromWhom = "no-reply@" + emailWhitelabel[0].domainName;

                    }
                    else
                    {
                        fromWhom = _configuration.GetSection("MailFrom").Value;
                    }


                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    //var encodeToken = HttpUtility.UrlEncode(token);
                    var resetUrl = baseUrl + "/" + _configuration.GetSection("ResetUrl").Value + "?token=" + token + "&email=" + user.Email;

                    //System.IO.File.WriteAllText("resetLink.txt", resetUrl);

                    var resetButton = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table -layout:fixed;background-color:#f9f9f9\" id=\"bodyTable\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style=\"max -width:600px\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td align=\"center\" valign=\"top\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-top:20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign=\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:'Montserrat', sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target =\"_blank\" class=\"text hideOnMobile\"></a>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBodyv style=\"max-width:600px\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td align=\"center\" valign=\"top\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableCard\" style=\"background -color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\"> " +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height=\"3\"> &nbsp;</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td style=\"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                        "<a href=\"#\" style=\"text-decoration:none\" target=\"_blank\">" +
                        "<img border=\"0\" src=\"" + companyLogo + "\" style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                        "</a>" +
                        "</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td style = \"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                        "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 20px\"> Reset Password</h2>" +
                        "</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td style=\"padding-left:20px;padding-right:20px\" align=\"center\" valign=\"top\" class=\"containtTable ui-sortable\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\" style=\"margin-top:40px\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                        "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> We received a request to reset your password. <br/> Use the link below to set up a new password for your account." +
                        "If you did not request to reset your password, ignore this email and the link will expire on its own.</p>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                        "<tbody>" +
                        "<tr>" +
                        "</tr>" +
                        "<tr>" +
                        "<td style=\"background-color: #FF8A73; padding: 12px 35px; border-radius: 5px;\" align=\"center\" class=\"ctaButton\"> <a href='" + resetUrl + "' style=\"background-color: #FF8A73;color:#fff;font-family:'Montserrat', sans-serif;font-size:13px;font-weight:600;font-style:normal;letter-spacing:1px;line-height:20px;text-transform:uppercase;text-decoration:none;display:block\" target=\"_blank\" class=\"text\"> Reset Password</a>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width= \"100%\" class=\"tableButton\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                        "<tbody>" +
                        "</tbody>" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-bottom:20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                        "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> If you have any further questions please visit our<span style=\"color: #6675df\"><a href=\"#\" style=\"color: #6675df\"> Help Center.</a></span></p>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"padding-bottom: 20px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                        "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\"> If you didn't request this email, there's nothing to worry about.you can safely ignore it. </p>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td style=\"font-size:1px;line-height:1px\" height=\"20\">&nbsp;</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style=\"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>" +
                        "</td>" +
                        "</tr>" +
                        "</tbody>" +
                        "</table>";

                    var client = new SendGridClient(_configuration.GetSection("Client").Value);

                    var msg = MailHelper.CreateSingleEmail(new EmailAddress(fromWhom), new EmailAddress(email),
                        "Reset Password", resetUrl, resetButton);
                    var response = client.SendEmailAsync(msg);
                    retVal = true;

                }
                else
                {
                    retVal = false;
                    // email user and inform them that they do not have an account

                }
            }

            return retVal;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        public async Task<bool> ResetUserPassword(string Email, string Password, string ConfirmPassword, string Token, ResetPasswordModel model)
        {
            bool resetPasswordSuccess = false;
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var removeSpaceFromToken = Regex.Replace(token, " ", "+");
                var result = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", removeSpaceFromToken);

                if (!result)
                {
                    resetPasswordSuccess = false;
                }
                else
                {
                    var updatePassword = await _userManager.ResetPasswordAsync(user, removeSpaceFromToken, Password);
                    if (updatePassword.Succeeded)
                    {
                        resetPasswordSuccess = true;
                    }

                }

            }
            return resetPasswordSuccess;
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                    }
                    TempData["RedirectUrl"] = _configuration.GetSection("FrontendUrl").Value + _configuration.GetSection("ResetUrl");
                    TempData["UserName"] = user.Email;
                    return View("ResetPasswordConfirmation");
                }
                ModelState.AddModelError("", "Invalid Request");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await _userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                var returnURL = _configuration.GetSection("FrontendUrl").Value;
                return Redirect(returnURL);
            }

            return View(model);
        }

        /// <summary>
        /// fetch user by given Subject ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> getUserBySubjectID(string sID)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(sID);
                var calims = await getClaims(user);
                dynamic MyDynamic = new System.Dynamic.ExpandoObject();
                MyDynamic.subjectId = user.Id;
                MyDynamic.username = user.UserName; // Change this with email
                MyDynamic.database = user.Database;
                MyDynamic.claims = new List<dynamic>();

                foreach (var claim in calims)
                {
                    dynamic MyDynamicClaim = new System.Dynamic.ExpandoObject();
                    MyDynamicClaim.claimType = claim.Type;
                    MyDynamicClaim.claimValue = claim.Value;

                    MyDynamic.claims.Add(MyDynamicClaim);
                }

                return Ok(MyDynamic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// fetch user by given Email ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> getUserByEmailID(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var calims = await getClaims(user);
                    dynamic MyDynamic = new System.Dynamic.ExpandoObject();
                    MyDynamic.subjectId = user.Id;
                    MyDynamic.username = user.UserName; // Change this with email
                    MyDynamic.database = user.Database;
                    MyDynamic.claims = new List<dynamic>();

                    foreach (var claim in calims)
                    {
                        dynamic MyDynamicClaim = new System.Dynamic.ExpandoObject();
                        MyDynamicClaim.claimType = claim.Type;
                        MyDynamicClaim.claimValue = claim.Value;

                        MyDynamic.claims.Add(MyDynamicClaim);
                    }

                    return Ok(MyDynamic);
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Fetch all Users from Identity server
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Users(List<string> uID)
        {
            try
            {
                dynamic users;
                if (uID.Count == 0)
                {
                    users = _userManager.Users.ToList();// .GetUserWithClaims();                   
                }
                else
                {
                    users = _userManager.Users.Where(e => uID.Contains(e.Id)).ToList();
                }

                dynamic MyDynamicUserList = new List<System.Dynamic.ExpandoObject>();
                foreach (var user in users)
                {
                    dynamic MyDynamic = new System.Dynamic.ExpandoObject();
                    MyDynamic.subjectId = user.Id;
                    MyDynamic.Id = user.Id;
                    MyDynamic.username = user.UserName; // Change this with email
                    MyDynamic.database = user.Database;
                    var calims = await getClaims(user);
                    MyDynamic.claims = new List<dynamic>();

                    foreach (var claim in calims)
                    {
                        dynamic MyDynamicClaim = new System.Dynamic.ExpandoObject();
                        MyDynamicClaim.claimType = claim.Type;
                        MyDynamicClaim.claimValue = claim.Value;

                        MyDynamic.claims.Add(MyDynamicClaim);
                    }

                    MyDynamicUserList.Add(MyDynamic);
                }
                return Ok(MyDynamicUserList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// This method will delete all the order presitent grants for the user from the database. 
        /// </summary>
        /// <param name="subjectid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteOldTokenBySubjectId(string subjectid)
        {
            try
            {
                var persitedgrants = _persistedGrantStore.GetAllAsync(subjectid).Result;

                persitedgrants = persitedgrants.OrderByDescending(x => x.Expiration);

                var grant = persitedgrants.FirstOrDefault();

                foreach (var grantRemove in persitedgrants)
                {
                    if (grant.Key == grantRemove.Key)
                    {
                        // Do nothing
                    }
                    else
                    {
                        _persistedGrantStore.RemoveAsync(grantRemove.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        //[HttpGet]
        //public IActionResult TwoFactor()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> TwoFactor(TwoFactorModel model)
        //{
        //    var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        //    if (!result.Succeeded)
        //    {
        //        ModelState.AddModelError("", "You login request has expired, please start over");
        //        return View();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));

        //        if (user != null)
        //        {
        //            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user,
        //                result.Principal.FindFirstValue("amr"), model.Token);

        //            if (isValid)
        //            {
        //                await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

        //                var claimsPrincipal = await _claimsFactory.CreateAsync(user);
        //                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);


        //                return Redirect(TempData["RedirectUrl"].ToString());
        //                // return RedirectToAction("Index");
        //            }

        //            ModelState.AddModelError("", "Invalid token");
        //            return View();
        //        }

        //        ModelState.AddModelError("", "Invalid Request");
        //    }

        //    return View();
        //}


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<List<Claim>> getClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, await _userManager.GetUserIdAsync(user)),
                new Claim(JwtClaimTypes.Name, await _userManager.GetUserNameAsync(user))
            };

            if (_userManager.SupportsUserEmail)
            {
                var email = await _userManager.GetEmailAsync(user);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified,
                            await _userManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (_userManager.SupportsUserPhoneNumber)
            {
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified,
                            await _userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (_userManager.SupportsUserClaim)
            {
                claims.AddRange(await _userManager.GetClaimsAsync(user));
            }

            if (_userManager.SupportsUserRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
            }

            // Add databases
            if (user.Database != null)
            {
                claims.Add(new Claim("Database", user.Database));
            }
            if (user.LivesIn != null)
            {
                claims.Add(new Claim("livesIn", user.LivesIn));
            }
            if (user.Occupation != null)
            {
                claims.Add(new Claim("phoneNumber", user.PhoneNumber));
            }
            if (user.Occupation != null)
            {
                claims.Add(new Claim("occupation", user.Occupation));
            }
            if (user.Gender != null)
            {
                claims.Add(new Claim("gender", user.Gender));
            }
            if (user.Birthplace != null)
            {
                claims.Add(new Claim("birthplace", user.Birthplace));
            }
            if (user.Birthday != null)
            {
                claims.Add(new Claim("birthday", user.Birthday.ToString("MM/dd/yyyy")));
            }
            claims.Add(new Claim("ShowDemoProject", user.ShowDemoProject.ToString(), ClaimValueTypes.Boolean));

            claims.Add(new Claim("TwoFactorEnabled", await _userManager.GetTwoFactorEnabledAsync(user) ? "true" : "false", ClaimValueTypes.Boolean));
            return claims;
        }
    }
}



