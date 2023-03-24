namespace CloudExternals.TestGen.Shared.OpenAI
{
    public class ChatRequest : Request
    {
        public ChatMessage[] messages { get; set; }

        public ChatRequest(OpenAIOptions settings, ChatMessage[] messages) : base(settings)
        {
            this.messages = messages;
        }
    }

    public class CompletionRequest : Request
    {
        public string prompt { get; set; }

        public CompletionRequest(OpenAIOptions settings, string prompt) : base(settings)
        {
            this.prompt = prompt;
        }
    }

    public class ChatMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public abstract class Request
    {
        public string model { get; set; }
        public double temperature { get; set; }
        //public int max_tokens { get; set; }
        public double top_p { get; set; }
        public double frequency_penalty { get; set; }
        public double presence_penalty { get; set; }

        protected Request(OpenAIOptions settings)
        {
            frequency_penalty = settings.FrequencyPenalty;
            //max_tokens = settings.MaxTokens;
            model = settings.Model;
            presence_penalty = settings.PresencePenalty;
            temperature = settings.Temperature;
            top_p = settings.TopP;
        }
    }

    public abstract class Response
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }

    public class ChatResponse : Response
    {
        public ChatChoice[] choices { get; set; }
    }

    public class CompletionResponse : Response
    {
        public CompletionChoice[] choices { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class ChatChoice : Choice
    {
        public Message message { get; set; }
    }

    public class CompletionChoice : Choice
    {
        public string text { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }
}
