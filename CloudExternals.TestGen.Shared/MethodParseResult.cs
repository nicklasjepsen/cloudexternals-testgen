namespace CloudExternals.TestGen.Shared
{
    public class MethodParseResult
    {
        public MethodParseResult(string name, string implementation)
        {
            Name = name;
            Implementation = implementation;
        }
        public string Name { get; set; }
        public string Implementation { get; set; }
    }
}