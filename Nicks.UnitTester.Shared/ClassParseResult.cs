using System.Collections.Generic;

namespace Nicks.UnitTester.Shared
{
    public class ClassParseResult
    {
        public ClassParseResult(string nameSpace, string className)
        {
            NameSpace = nameSpace;
            ClassName = className;
        }

        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public List<MethodParseResult> Methods { get; set; } = new List<MethodParseResult>();

        public override string ToString()
        {
            // Return a string representation of all properties
            return $"Namespace: {NameSpace}\nClass name: {ClassName}\nMethods:\n{string.Join("\n", Methods)}";
        }
    }
}