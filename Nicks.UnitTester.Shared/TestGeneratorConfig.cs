namespace Nicks.UnitTester.Shared
{
    public class TestGeneratorConfig
    {
        /// <summary>
        /// Currently only support xUnit
        /// </summary>
        public string TestFramework { get; set; } = "xUnit";

        /// <summary>
        /// If not set, the generator will create a new project, if set this must be set to the name of an existing Test project
        /// </summary>
        public string TestProject { get; set; }
        /// <summary>
        /// Only needed if <seealso cref="TestProject"/> is not set
        /// </summary>
        public string NameFormatForTestProject { get; set; }
        /// <summary>
        /// Only needed if <seealso cref="TestProject"/> is not set
        /// </summary>
        public string Namespace { get; set; }
        //public string OutputFile { get; set; }
        //public string NameFormatForTestClass{ get; set; }

    }
}
