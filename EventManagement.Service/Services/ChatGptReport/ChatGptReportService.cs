using System;
using System.Collections.Generic;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using Amazon.Amplify.Model;
using SendGrid.Helpers.Mail.Model;

namespace EventManagement.Service
{
    public class ChatGptReportService : IChatGptReportService
    {

        #region PRIVATE MEMBERS


        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public ChatGptReportService(ILogger<ChatGptReportService> logger, IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<UserTokenResponseDto> GetReportSummary(Prompts prompts)
        {
            var chatGPTInvocationDTO = new ChatGPTInvocationDTO();

            var responseBody = new UserTokenResponseDto();

            var apiKey = _configuration.GetSection("ChatGptApiKey").Value;

            RestClientOptions options = new RestClientOptions("https://api.openai.com");
            RestClient client = new RestClient(options);
            RestRequest request = new RestRequest("/v1/chat/completions");

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + apiKey);

            Message systemMessage = new Message()
            {
                role = "system",
                content = "Follow these steps to answer the user queries.\r\n\r\nStep 1: The user will provide you with a list of data enclosed in triple quotes.\r\n\r\nStep 2: Understand our list of data. The data in brackets represents the data from the previous period. For example, [{“OrganicSessions”: “100(+25)”}] indicates that our current period data for organic sessions is 100, and the previous period data was 75.\r\n\r\nStep 3: Understand our list of data. The data in brackets represents the percentage increase from the previous period. For example, [{“OrganicSessions”: “100(+25%)”}] indicates that our current period data for organic sessions is 100, and there has been a 25% increase from the previous period data.\r\n\r\nStep 4: If the list of data is “null” or “undefined”, then do not write anything about that specific topic.\r\n\r\nStep 5: As a digital marketing expert, you are required to generate a concise summary of the marketing report . Ensure the summary reflects the positive aspects of the marketing report . Focus on positive improvements. Use a professional writing style and a positive voice to describe negative data. for example, instead of using the word “decrease”, you could say “below the target” in overall summary . Act as a Digital Marketing Expert.\r\n\r\nStep 6: Create a heading titled “Performance Achievements”. Then, explain all the data based on performance achievements and emphasize positive achievements in only 100 words.\r\n\r\nStep 7: Create a heading titled “Comparative Success”. Then, explain all the data based on comparative success and emphasize positive achievements in only 100 words.\r\n\r\nStep 8: Create a heading titled “Overall Positive Achievements”. Then, explain all the data based on overall positive achievements and emphasize positive achievements in only 100 words."
            };

            // User message
            Message userMessage = new Message()
            {
                role = "user",
                content = prompts.Content,
            };

            ChatGPTTurboModel chatGPTTurboModel = new ChatGPTTurboModel()
            {
                temperature = 0.7,
                model = "gpt-3.5-turbo-16k",
                frequency_penalty = 0,
                presence_penalty = 0,
                top_p = 1,
                max_tokens = 400,
                stop = new string[] { "Note", "In conclusion", "In summary", "Please note" }
            };

            // Add system and user messages to the model
            chatGPTTurboModel.messages.Add(systemMessage);
            chatGPTTurboModel.messages.Add(userMessage);


            string json = JsonConvert.SerializeObject(chatGPTTurboModel);
            request.AddStringBody(json, ContentType.Json);

            var response = await client.PostAsync(request);
            ChatGPTTurboResponseDto chatGPTTurboResponseDto = JsonConvert.DeserializeObject<ChatGPTTurboResponseDto>(response.Content);

            var Content = chatGPTTurboResponseDto?.choices.FirstOrDefault()?.message.content;
            var TokensUsed = chatGPTTurboResponseDto?.usage.total_tokens;

            //remove following text from content
            Content = Content.Replace("Hook Line:", "");
            Content = Content.Replace("Hook line:", "");
            Content = Content.Replace("Hook:", "");

            var dateRange = string.Empty;

            if (!string.IsNullOrEmpty(prompts.DateRange))
            {
                 dateRange = "<br /> The report from " + prompts.DateRange + "<br />";
            }
            
            responseBody.Content = dateRange + Content.Replace("\n", "<br />");


            return responseBody;
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION



        #endregion
    }
}
