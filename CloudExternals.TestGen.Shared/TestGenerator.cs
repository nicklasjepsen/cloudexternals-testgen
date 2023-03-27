using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudExternals.TestGen.Shared.OpenAI;

namespace CloudExternals.TestGen.Shared
{
    public class TestGenerator
    {
        private const string prompt =
            "You are a code generator that only responds with generated code. Your task is to generate unit tests covering at least 90 % of the following C# class. Use xUnit and Moq.";
        private static OpenAIOptions options = new OpenAIOptions(OpenAIOptions.GPT35Turbo)
        {
            //MaxTokens = int.MaxValue,
            Temperature = 0,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            TopP = 1
        };
        private readonly OpenAIService _openAI;
        private readonly CodeParser _parser;
        private readonly MarkdownParser _markdownParser;

        public TestGenerator(OpenAIService openAI, CodeParser parser, MarkdownParser markdownParser)
        {
            _openAI = openAI;
            _parser = parser;
            _markdownParser = markdownParser;
        }

        private void CreateTestProjectAndAddToSolution(string namespaceTemplate, string name)
        {

        }

        public async Task<IEnumerable<GenerationResult>> GenerateTests(params string[] files)
        {
            var fileContents = files.Select(file => _parser.ParseFile(file)).ToList();
            var results = new List<GenerationResult>();
            foreach (var file in fileContents)
            {
                foreach (var c in file.Classes)
                {
                    foreach (var m in c.Methods)
                    {
                        var result = await _openAI.ChatCompletion(new[]
                        {
                            new ChatMessage
                            {
                                content = prompt,
                                role = "user"
                            },
                            new ChatMessage
                            {
                                content = m.Implementation,
                                role = "user"
                            }
                        }, options);

                        // We only want the code results, so try to use markdig to grab code snippets
                        var code = _markdownParser.GetCodeBlocks(result.choices[0].message.content)
                            .ToList();
                        results.Add(new GenerationResult
                        {
                            CodeBlocks = code,
                            ClassName = c.ClassName,
                            MethodName = m.Name,
                            Namespace = c.NameSpace
                        });
                    }
                }
            }

            return results;
        }
    }

    public class GenerationResult
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public List<string> CodeBlocks { get; set; }
    }
}
