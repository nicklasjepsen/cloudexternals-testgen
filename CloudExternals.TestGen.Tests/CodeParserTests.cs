using CloudExternals.TestGen.Shared;

namespace CloudExternals.TestGen.Tests
{
    public class CodeParserTests
    {
        //[Fact]
        //public void ParseFile_ReturnsExpectedResult()
        //{
        //    // Arrange
        //    var path = "test.cs";
        //    var text = "namespace Test { public class Foo { public void Bar() {} } }";
        //    var fileParseResult = new FileParseResult("test.cs", new CodeParseResult
        //    {
        //        Classes = { new ClassParseResult("Test", "Foo") { Methods = { new MethodParseResult("Bar", "public void Bar() {}") } } }
        //    });

        //    var fileSystem = new Mock<IFileSystem>();
        //    fileSystem.Setup(fs => fs.ReadAllText(path)).Returns(text);

        //    var codeParser = new CodeParser(fileSystem.Object);

        //    // Act
        //    var result = codeParser.ParseFile(path);

        //    // Assert
        //    Assert.Equal(fileParseResult, result);
        //}

        [Fact]
        public void ParseCode_ReturnsExpectedResult()
        {
            // Arrange
            var code = "namespace Test { public class Foo { public void Bar() {} } }";
            var codeParseResult = new CodeParseResult
            {
                Classes = { new ClassParseResult("Test", "Foo") { Methods = { new MethodParseResult("Bar", "public void Bar() {}") } } }
            };

            var codeParser = new CodeParser();

            // Act
            var result = codeParser.ParseCode(code);

            // Assert
            Assert.Equivalent(codeParseResult, result);
        }

        [Fact]
        public void ParseCode_WithMaxNumMethods_ReturnsExpectedResult()
        {
            // Arrange
            var code = "namespace Test { public class Foo { public void Bar() {} public void Baz() {} } }";
            var codeParseResult = new CodeParseResult
            {
                Classes = { new ClassParseResult("Test", "Foo") { Methods = { new MethodParseResult("Bar", "public void Bar() {}") } } }
            };

            var codeParser = new CodeParser();

            // Act
            var result = codeParser.ParseCode(code, 1);

            // Assert
            Assert.Equivalent(codeParseResult, result);
        }
    }
}
