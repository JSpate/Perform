using Antlr4.Runtime;

namespace Perform.Script.UnitTests;

[TestFixture]
public class ShowScriptLanguageTests
{
    private static string GenerateCSharp(string script)
    {
        var inputStream = new AntlrInputStream(script);
        var lexer = new ShowScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ShowScriptParser(tokenStream);
        var tree = parser.script();
        var visitor = new ShowScriptToCSharpVisitor(new ShowScript());
        return visitor.Visit(tree);
    }

    [Test]
    public Task ConstAndVarDeclarations()
    {
        var script = @"
const MaxValue = 42;
int number = 5;
string name = ""test"";
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task LetAndAssignment()
    {
        var script = @"
on(StartSong) {
    var x = 10;
    x = x + 1;
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task EventBlockWithWhen()
    {
        var script = @"
on(StartSong) when(x > 0) {
    var started = true;
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task IfElseStatement()
    {
        var script = @"
on(StartSong) {
    if (x > 0) {
        var y = 1;
    } else {
        var y = 2;
    }
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task ForLoopWithBreakContinue()
    {
        var script = @"
on(StartSong) {
    for (var i = 0; i < 10; i = i + 1) {
        if (i == 5) {
            break;
        }
        if (i % 2 == 0) {
            continue;
        }
    }
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }
    [Test]
    public Task OnLoopBlockWithoutEndLoop()
    {
        var script = @"
on(StartSong) {
    var started = true;
} loop {
    Log.Info(""Loop running"");
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task MultipleEventsWithLoopBlocks()
    {
        var script = @"
on(StartSong) {
    var started = true;
} loop {
    Log.Info(""Start loop"");
    endLoop;
}

on(EndSong) {
    var ended = true;
} loop {
    Log.Info(""End loop"");
    endLoop;
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task OnLoopBlockWithEndLoop()
    {
        var script = @"
on(StartSong) {
    var started = true;
} loop {
    if (started) {
        Log.Info(""Loop running"");
        endLoop;
    }
    Log.Info(""Still looping"");
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task TryCatchFinally()
    {
        var script = @"
on(StartSong) {
    try {
        var x = 1 / 0;
    } catch (ex) {
        Log.Error(ex);
    } finally {
        Log.Info(""done"");
    }
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task SetBlockStatement()
    {
        var script = @"
on(StartSong) {
    set(DMX.Light1, DMX.Light2) {
        r = 255;
        g = 128;
    }
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task FunctionAndDeviceCalls()
    {
        var script = @"
on(StartSong) {
    Log.Info(""Hello"");
    Math.Sin(3.14);
    Color.RGB(255,0,0);
    Lites(DMX.Light1).Set(255,0,0,0);
    get(DMX.Light1);
    isAvailable(DMX.Light1);
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task ExpressionsAndArrays()
    {
        var script = @"
on(StartSong) {
    var arr = [ (1,2,3), (4,5,6) ];
    var t = (7,8,9);
    var b = 1 + 2 * 3 - 4 / 2;
    var c = a ? b : d;
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }

    [Test]
    public Task FullFeatureScript()
    {
        var script = @"
const MaxVal = 10;
int count = 0;
on(ButtonChange, 1, ButtonState.Down) {
    for (var i = 0; i < MaxVal; i = i + 1) {
        if (i == 5) {break;}
        if (i % 2 == 0) {continue;}
        set(DMX.Light1) { r = i * 10; }
    }
    try {
        Log.Info(""Try"");
    } catch (ex) {
        Log.Error(ex); 
    } finally {
        Log.Info(""Finally"");
    }
}
";
        var csharp = GenerateCSharp(script);
        return Verifier.Verify(csharp);
    }
}