using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP.Model
{
    public class ROPCTokenResponse
    { 
        public bool IsError { get; set; }
        public string Error { get; set; }
        public ROPCToken TokenData { get; set; }

        public ROPCTokenResponse()
        {
            IsError = false;
            Error = "";
            TokenData = new ROPCToken();
        }
    }
        public class ROPCToken
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string IdentityToken { get; set; }
        public string RefreshToken { get; set; }
        public ROPCTokenHeader header { get; set; }
        public ROPCTokenClaims claims { get; set; }
    }
    public class ROPCTokenHeader
    {
        public string alg { get; set; }
        public string kid { get; set; }
        public string typ { get; set; }
    }
    public class ROPCTokenClaims
    {
        public int nbf { get; set; }
        public int exp { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public string client_id { get; set; }
        public string sub { get; set; }
        public int auth_time { get; set; }
        public string idp { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string super_admin { get; set; }
        public string birthday { get; set; }
        public string given_name { get; set; }
        public string given_lname { get; set; }
        public bool IsAuthenticatorConfigured { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<string> scope { get; set; }
        public List<string> amr { get; set; }
        public bool ShowDemoProject { get; set; }
    }
}
