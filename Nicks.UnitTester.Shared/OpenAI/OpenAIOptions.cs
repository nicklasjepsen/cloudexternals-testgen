namespace Nicks.UnitTester.Shared.OpenAI
{
    public class OpenAIOptions
    {
        public const string GPT35Turbo = "gpt-3.5-turbo";
        public OpenAIOptions(string model)
        {
            Model = model;
        }

        public string Model { get; set; }
        public double Temperature { get; set; }
        //public int MaxTokens { get; set; }
        public double TopP { get; set; }
        public double FrequencyPenalty { get; set; }
        public double PresencePenalty { get; set; }
    }
}
