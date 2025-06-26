using System.Reflection;
using Antlr4.Runtime;
using Perform.Model;

namespace Perform.Script;

public class GrammarTest
{
    public static Assembly RunTest()
    {
        // Read the test script
        var script = File.ReadAllText("TestScript.txt");

        // Set up ANTLR input
        var inputStream = new AntlrInputStream(script);
        var lexer = new ShowScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ShowScriptParser(tokenStream);

        // Parse using the root rule
        var tree = parser.script();

        var visitor = new ShowScriptToCSharpVisitor(new ShowScript());
        var generatedCSharp = visitor.Visit(tree);
        
        var assembly = InMemoryCompiler.Compile(
            generatedCSharp,
            [
                "Perform.Core.dll", 
                "Microsoft.Extensions.Logging.Abstractions.dll"
            ]);
        
        return assembly;
    }
}
