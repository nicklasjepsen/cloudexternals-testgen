using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nicks.UnitTester.Shared
{
    public class CodeParser
    {
        public FileParseResult ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return new FileParseResult(Path.GetFileName(path), ParseCode(text));
        }

        public CodeParseResult ParseCode(string code)
        {
            return DoParse(code);
        }

        public CodeParseResult ParseCode(string code, int maxNumMethods)
        {
            return DoParse(code, maxNumMethods);
        }

        private static CodeParseResult DoParse(string code, int takeMethods = int.MaxValue)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var result = new CodeParseResult();
            foreach (var member in root.Members)
            {
                switch (member.Kind())
                {
                    case SyntaxKind.NamespaceDeclaration:
                    {
                        var ns = (NamespaceDeclarationSyntax)member;
                        foreach (var c in ns.Members.OfType<ClassDeclarationSyntax>())
                        {
                            result.Classes.Add(new ClassParseResult(ns.Name.ToString(), c.Identifier.ToString())
                            {
                                Methods = c.Members.OfType<MethodDeclarationSyntax>()
                                    .Take(takeMethods)
                                    .Select(m => new MethodParseResult(m.Identifier.Text, m.ToString()))
                                    .ToList()
                            });
                        }
                        break;
                    }
                }
            }

            return result;
        }
    }
}
