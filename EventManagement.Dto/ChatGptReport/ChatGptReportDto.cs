using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// ChatGptReport Model
    /// </summary>
    public class ChatGptReportDto : ChatGptReportAbstractBase
    {

    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ChatGPTTurboModel
    {
        public string model { get; set; }
        public List<Message> messages { get; set; } = new List<Message>();
        public double temperature { get; set; }
        public int max_tokens { get; set; }
        public int top_p { get; set; }
        public int frequency_penalty { get; set; }
        public string[] stop { get; set; }
        public int presence_penalty { get; set; }

    }

    public class ChatGPTInvocationDTO
    {
        public int UserId { get; set; } = 0;
        public string LoggedInUserId { get; set; }
        public string text { get; set; } = string.Empty;
        public string tone { get; set; } = string.Empty;
        public string style { get; set; } = string.Empty;
        public string topic { get; set; } = string.Empty;
        public string founder { get; set; } = string.Empty;
        public int maxTokens { get; set; } = 0;
        public int promptIndex { get; set; } = 0;

    }

    public class UserTokenResponseDto
    {
        public int? TokensAvailable { get; set; }

        public string? Content { get; set; }

        public List<ResponseHookDto> Hooks { get; set; } = new List<ResponseHookDto>();
    }

    public class Prompts
    {
        public string Content { get; set; }

        public string DateRange { get; set; }
    }

    public class ResponseHookDto
    {
        public string? Topic { get; set; }
        public string Content { get; set; }
    }

    public class ChatGPTTurboResponseDto
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}
 