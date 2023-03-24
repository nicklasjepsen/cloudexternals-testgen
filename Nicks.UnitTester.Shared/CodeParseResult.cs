using System.Collections.Generic;

namespace Nicks.UnitTester.Shared
{
    public class CodeParseResult
    {
        public List<ClassParseResult> Classes { get; set; } = new List<ClassParseResult>();

        public override string ToString()
        {
            return $"{string.Join("\n\n", Classes)}";
        }
    }
}