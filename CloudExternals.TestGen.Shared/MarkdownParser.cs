using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Syntax;

namespace CloudExternals.TestGen.Shared
{
    public class MarkdownParser
    {
        public IEnumerable<string> GetCodeBlocks(string input)
        {
            var document = Markdown.Parse(input);
            return document.Select(b => b as CodeBlock)
                .Where(b => b != null)
                .Select(hb => string.Join("\n", hb.Lines));
        }
    }
}
