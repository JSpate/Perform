using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Perform.Script;

public static class InMemoryCompiler
{
    public static Assembly Compile(string code, string[] refs)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = new List<MetadataReference>();

        // Add core .NET assemblies from the trusted platform assemblies
        var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? [];
        foreach (var path in trustedAssemblies)
        {
            var fileName = Path.GetFileName(path);
            if (fileName is "System.Runtime.dll" or "netstandard.dll" or "System.Private.CoreLib.dll")
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        foreach (var r in refs)
        {
            references.Add(MetadataReference.CreateFromFile(r));
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "InMemoryAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join(Environment.NewLine, result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new Exception("Compilation failed:\n" + errors);
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}
