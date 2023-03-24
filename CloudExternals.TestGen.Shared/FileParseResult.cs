namespace CloudExternals.TestGen.Shared
{
    public class FileParseResult : CodeParseResult
    {
        public FileParseResult(string fileName, CodeParseResult codeParseResult)
        {
            FileName = fileName;
            Classes = codeParseResult.Classes;
        }

        public string FileName { get; set; }

        public override string ToString()
        {
            return $"File: {FileName}\n{base.ToString()}";
        }
    }
}