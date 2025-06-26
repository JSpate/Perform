using System.Text;

namespace Perform.Script;

public class ShowScriptToCSharpVisitor(ShowScript showScript) : ShowScriptBaseVisitor<string>
{
    private readonly List<string> _eventHandlerMethods = new();
    private readonly List<string> _eventHandlerRegistrations = new();
    private bool _inFunctionOrHandler = false;
    private readonly List<string> _moduleConstants = new();
    private readonly List<string> _moduleVars = new();
    private int _eventHandlerCounter = 0;

    public override string VisitAdditiveExpr(ShowScriptParser.AdditiveExprContext context)
    {
        var left = Visit(context.multiplicativeExpr(0));
        for (var i = 1; i < context.multiplicativeExpr().Length; i++)
        {
            var op = context.GetChild(2 * i - 1).GetText();
            left += $" {op} " + Visit(context.multiplicativeExpr(i));
        }
        return left;
    }

    public override string VisitArray(ShowScriptParser.ArrayContext context)
    {
        var tuples = context.tuple().Select(Visit).ToArray();
        return $"new[] {{ {string.Join(", ", tuples)} }}";
    }

    public override string VisitAssignment(ShowScriptParser.AssignmentContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        return $"{name} = {expr};";
    }

    public override string VisitAssignmentNoSemi(ShowScriptParser.AssignmentNoSemiContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        return $"{name} = {expr}";
    }
    public override string VisitTryStatement(ShowScriptParser.TryStatementContext context)
    {
        var tryBlock = Visit(context.block());
        var catchBlock = context.catchBlock() != null ? Visit(context.catchBlock()) : "";
        var finallyBlock = context.finallyBlock() != null ? Visit(context.finallyBlock()) : "";
        return $"try {tryBlock}{catchBlock}{finallyBlock}";
    }

    public override string VisitCatchBlock(ShowScriptParser.CatchBlockContext context)
    {
        var exceptionVar = context.ID().GetText();
        var catchBody = Visit(context.block());
        return $" catch (Exception {exceptionVar}) {catchBody}";
    }

    public override string VisitFinallyBlock(ShowScriptParser.FinallyBlockContext context)
    {
        var finallyBody = Visit(context.block());
        return $" finally {finallyBody}";
    }

    public override string VisitBlock(ShowScriptParser.BlockContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        foreach (var stmt in context.statement())
        {
            var code = Visit(stmt);
            if (!string.IsNullOrWhiteSpace(code))
            {
                var trimmed = code.TrimEnd();
                if (!trimmed.EndsWith(";") && !trimmed.EndsWith("}") && !trimmed.StartsWith("public"))
                    code += ";";
                sb.AppendLine("    " + code.Replace("\n", "\n    "));
            }
        }
        sb.Append("}");
        return sb.ToString();
    }
    public override string VisitBreakStatement(ShowScriptParser.BreakStatementContext context)
    {
        return "break;";
    }

    public override string VisitContinueStatement(ShowScriptParser.ContinueStatementContext context)
    {
        return "continue;";
    }

    public override string VisitButtonStateEnum(ShowScriptParser.ButtonStateEnumContext context)
    {
        return context.GetText();
    }

    public override string VisitCallArg(ShowScriptParser.CallArgContext context)
    {
        if (context.ID() != null && context.ASSIGN() != null)
            return $"{context.ID().GetText()} : {Visit(context.ID())}";
        return Visit(context.expr());
    }

    public override string VisitCallArgs(ShowScriptParser.CallArgsContext context)
    {
        return string.Join(", ", context.callArg().Select(Visit));
    }

    public override string VisitColorCall(ShowScriptParser.ColorCallContext context)
    {
        return $"Color.{context.colorFunction().GetText()}";
    }

    public override string VisitConstStatement(ShowScriptParser.ConstStatementContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        string code;
        if (int.TryParse(expr, out _))
            code = $"public const int {name} = {expr};";
        else if (expr == "true" || expr == "false")
            code = $"public const bool {name} = {expr};";
        else if (expr.StartsWith("\"") && expr.EndsWith("\""))
            code = $"public const string {name} = {expr};";
        else
            code = $"public const dynamic {name} = {expr};";

        if (!_inFunctionOrHandler)
        {
            _moduleConstants.Add(code);
            return "";
        }
        return code;
    }

    public override string VisitDottedID(ShowScriptParser.DottedIDContext context)
    {
        var ids = new List<string>();
        CollectDottedIds(context, ids);
        return string.Join(".", ids);
    }

    public override string VisitEqualityExpr(ShowScriptParser.EqualityExprContext context)
    {
        var left = Visit(context.relationalExpr(0));
        for (var i = 1; i < context.relationalExpr().Length; i++)
            left += " && " + Visit(context.relationalExpr(i));
        return left;
    }

    public override string VisitEventBlock(ShowScriptParser.EventBlockContext context)
    {
        _inFunctionOrHandler = true;
        var handlerName = $"EventHandler_{++_eventHandlerCounter}";
        var eventTypeAndArgs = context.eventTypeAndArgs();
        var eventType = eventTypeAndArgs.eventType().GetText();
        var eventArgs = new List<string>();

        if (eventTypeAndArgs.eventArg() != null && eventTypeAndArgs.eventArg().Length > 0)
        {
            foreach (var arg in eventTypeAndArgs.eventArg())
            {
                var argStr = VisitEventArgForHandler(arg);
                if (!string.IsNullOrWhiteSpace(argStr))
                    eventArgs.Add(argStr);
            }
        }

        var blockCode = Visit(context.block());
        if (!blockCode.TrimEnd().EndsWith("return Task.CompletedTask;"))
        {
            var insertPos = blockCode.LastIndexOf('}');
            if (insertPos > 0)
            {
                blockCode = blockCode.Insert(insertPos, "    return Task.CompletedTask;\n");
            }
        }
        var handlerMethod = $"private Task {handlerName}(CancellationToken cancellationToken) {blockCode}";
        _eventHandlerMethods.Add(handlerMethod);

        var eventTypeEnum = $"EventType.{eventType}";
        var argsList = eventArgs.Count > 0 ? ", " + string.Join(", ", eventArgs) : "";
        var registration = $"{handlerName}, {eventTypeEnum}{argsList}";
        _eventHandlerRegistrations.Add(registration);

        _inFunctionOrHandler = false;
        return "";
    }

    public override string VisitExpr(ShowScriptParser.ExprContext context)
        => Visit(context.ternaryExpr());

    public override string VisitForStatement(ShowScriptParser.ForStatementContext context)
    {
        var init = "";
        if (context.letStatementNoSemi(0) != null)
            init = Visit(context.letStatementNoSemi(0));
        else if (context.assignmentNoSemi(0) != null)
            init = Visit(context.assignmentNoSemi(0));
        else if (context.varDeclNoSemi(0) != null)
            init = Visit(context.varDeclNoSemi(0));

        var cond = context.expr(0) != null ? Visit(context.expr(0)) : "";

        string inc = "";
        if (context.letStatementNoSemi(1) != null)
            inc = Visit(context.letStatementNoSemi(1));
        else if (context.assignmentNoSemi(1) != null)
            inc = Visit(context.assignmentNoSemi(1));
        else if (context.varDeclNoSemi(1) != null)
            inc = Visit(context.varDeclNoSemi(1));
        else if (context.expr(1) != null)
            inc = Visit(context.expr(1));

        var body = Visit(context.block());

        body = body.Insert(1,
            "\r\n\tif (cancellationToken.IsCancellationRequested)\r\n\t{\r\n\t\t          break;\r\n            }");

        return $"for ({init}; {cond}; {inc}) {body}";
    }

    public override string VisitGenericCall(ShowScriptParser.GenericCallContext context)
    {
        var func = Visit(context.dottedID());
        var args = context.callArgs() != null ? Visit(context.callArgs()) : "";
        return $"{func}({args})";
    }

    public override string VisitGlobalGetCall(ShowScriptParser.GlobalGetCallContext context)
    {
        var expr = context.expr();
        var arg = TryGetDottedIdOrIdAsString(expr) ?? Visit(expr);
        var type = showScript.TypeFor(arg);
        return $"_showScript.Get<{type}>({arg})";
    }

    public override string VisitGlobalIsAvailableCall(ShowScriptParser.GlobalIsAvailableCallContext context)
    {
        var expr = context.expr();
        var arg = TryGetDottedIdOrIdAsString(expr) ?? Visit(expr);
        return $"_showScript.IsAvailable({arg})";
    }

    public override string VisitIfStatement(ShowScriptParser.IfStatementContext context)
    {
        var cond = Visit(context.expr());
        var thenBlock = Visit(context.block(0));
        var sb = new StringBuilder();
        sb.Append($"if ({cond}) {thenBlock}");

        if (context.ELSE() != null)
        {
            if (context.ifStatement() != null)
            {
                sb.Append(" else ");
                sb.Append(Visit(context.ifStatement()));
            }
            else if (context.block().Length > 1)
            {
                sb.Append(" else ");
                sb.Append(Visit(context.block(1)));
            }
        }

        return sb.ToString();
    }

    public override string VisitIsAvailableCall(ShowScriptParser.IsAvailableCallContext context)
    {
        var obj = Visit(context.dottedID());
        return $"{obj}.isAvailable()";
    }

    public override string VisitLetStatement(ShowScriptParser.LetStatementContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        var code = $"var {name} = {expr};";
        if (!_inFunctionOrHandler)
        {
            _moduleVars.Add(code);
            return "";
        }
        return code;
    }

    public override string VisitLetStatementNoSemi(ShowScriptParser.LetStatementNoSemiContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        return $"var {name} = {expr}";
    }

    public override string VisitLitesCall(ShowScriptParser.LitesCallContext context)
    {
        // Get all dottedID arguments as strings
        var litesArgs = context.litesArgs().dottedID();
        var liteNames = litesArgs.Select(arg =>
        {
            var ids = new List<string>();
            CollectDottedIds(arg, ids);
            return $"\"{string.Join(".", ids)}\"";
        });
        var litesArray = $"[{string.Join(", ", liteNames)}]";

        // Get the function call (e.g., Chase([...]))
        var funcCall = context.litesFunction().GetText();

        // Output the desired C# code
        return $"_showScript.WithLites(cancellationToken, {litesArray}).{funcCall}";
    }

    public override string VisitLogicExpr(ShowScriptParser.LogicExprContext context)
    {
        var left = Visit(context.equalityExpr(0));
        for (var i = 1; i < context.equalityExpr().Length; i++)
            left += " || " + Visit(context.equalityExpr(i));
        return left;
    }

    public override string VisitLogCall(ShowScriptParser.LogCallContext context)
    {
        var level = context.logLevel().GetText();
        var logCall = level switch
        {
            "Info" => "LogInformation",
            "Warn" => "LogWarning",
            _ => "LogError"
        };

        var arg = Visit(context.expr());
        return $"_logger.{logCall}({arg})";
    }

    public override string VisitMathCall(ShowScriptParser.MathCallContext context)
    {
        return $"Math.{context.mathFunction().GetText()}";
    }

    public override string VisitMotionCall(ShowScriptParser.MotionCallContext context)
    {
        // Get all dottedID arguments as strings
        var motionArgs = context.motionArgs().dottedID();
        var motionNames = motionArgs.Select(arg =>
        {
            var ids = new List<string>();
            CollectDottedIds(arg, ids);
            return $"\"{string.Join(".", ids)}\"";
        });
        var motionArray = $"[{string.Join(", ", motionNames)}]";

        // Get the function call (e.g., Circle(...))
        var funcCall = context.motionFunction().GetText();

        // Output the desired C# code
        return $"_showScript.WithMotion(cancellationToken, {motionArray}).{funcCall}";
    }

    public override string VisitMultiplicativeExpr(ShowScriptParser.MultiplicativeExprContext context)
    {
        var left = Visit(context.unaryExpr(0));
        for (var i = 1; i < context.unaryExpr().Length; i++)
        {
            var op = context.GetChild(2 * i - 1).GetText();
            left += $" {op} " + Visit(context.unaryExpr(i));
        }
        return left;
    }

    public override string VisitPrimaryExpr(ShowScriptParser.PrimaryExprContext context)
    {
        var result = Visit(context.atom());

        var exprIndex = 0;
        for (var i = 1; i < context.ChildCount;)
        {
            var child = context.GetChild(i);
            if (child.GetText() == "[")
            {
                var exprText = Visit(context.expr(exprIndex));
                exprIndex++;

                if (int.TryParse(exprText, out var tupleIndex) && tupleIndex is >= 0 and <= 7)
                {
                    result = $"{result}.Item{tupleIndex + 1}";
                }
                else
                {
                    result = $"{result}[{exprText}]";
                }
                i += 3;
            }
            else if (child.GetText() == ".")
            {
                var next = context.GetChild(i + 1);
                if (next.GetText() == "Length")
                {
                    result = $"{result}.Length";
                    i += 3;
                }
                else
                {
                    var methodName = next.GetText();
                    var args = "";

                    if (i + 3 < context.ChildCount && context.GetChild(i + 3) is ShowScriptParser.CallArgsContext callArgsCtx)
                    {
                        args = Visit(callArgsCtx);
                    }
                    result = $"{result}.{methodName}({args})";
                    i += (args.Length > 0) ? 5 : 4;
                }
            }
            else if (child.GetText() == "++")
            {
                result = $"{result}++";
                i++;
            }
            else if (child.GetText() == "--")
            {
                result = $"{result}--";
                i++;
            }
            else
            {
                i++;
            }
        }

        return result;
    }

    public override string VisitRelationalExpr(ShowScriptParser.RelationalExprContext context)
    {
        var left = Visit(context.additiveExpr(0));
        for (var i = 1; i < context.additiveExpr().Length; i++)
        {
            var op = context.compOp(i - 1).GetText();
            left += $" {op} " + Visit(context.additiveExpr(i));
        }
        return left;
    }

    public override string VisitReturnStatement(ShowScriptParser.ReturnStatementContext context)
    {
        if (context.expr() != null)
            return $"return {Visit(context.expr())};";
        return "return;";
    }

    public override string VisitScript(ShowScriptParser.ScriptContext context)
    {
        _eventHandlerMethods.Clear();
        _eventHandlerRegistrations.Clear();
        _eventHandlerCounter = 0;
        _moduleConstants.Clear();
        _moduleVars.Clear();

        foreach (var child in context.children)
        {
            Visit(child);
        }

        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using Perform;");
        sb.AppendLine("using Perform.Model;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Math = Perform.Scripting.MathMethods;");
        sb.AppendLine();
        sb.AppendLine("public class GeneratedShowScript : IDisposable");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ShowScript _showScript;");
        sb.AppendLine("    private readonly ILogger _logger;");

        foreach (var c in _moduleConstants)
            sb.AppendLine("    " + c);
        foreach (var v in _moduleVars)
            sb.AppendLine("    " + v);

        sb.AppendLine("    public GeneratedShowScript(ShowScript showScript, ILogger logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _showScript = showScript;");
        sb.AppendLine("        _logger = logger;");

        foreach (var reg in _eventHandlerRegistrations)
            sb.AppendLine($"        _showScript.AddEventHandler({reg});");
        sb.AppendLine("    }");

        foreach (var method in _eventHandlerMethods)
            sb.AppendLine("    " + method.Replace("\n", "\n    "));

        sb.AppendLine("    public void Dispose() {");
        foreach (var reg in _eventHandlerRegistrations)
            sb.AppendLine($"        _showScript.RemoveEventHandler({reg});");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        return sb.ToString();
    }

    public override string VisitSetBlockStatement(ShowScriptParser.SetBlockStatementContext context)
    {
        var deviceTargets = context.setTargets().dottedID();
        var assignments = context.setBlock().setAssignment();

        var sb = new StringBuilder();

        foreach (var device in deviceTargets)
        {
            var devicePath = GetDottedIdString(device);
            foreach (var assignment in assignments)
            {
                var property = assignment.ID().GetText();
                var value = Visit(assignment.expr());
                var arg = $"{devicePath}.{property}";
                sb.AppendLine($"_showScript.Set(\"{arg}\", {value});");
            }
        }

        return sb.ToString();
    }

    public override string VisitSongCall(ShowScriptParser.SongCallContext context)
    {
        return $"_showScript.CurrentSong.{context.songFunction().GetText().TrimEnd('(', ')')}";
    }

    public override string VisitTernaryExpr(ShowScriptParser.TernaryExprContext context)
    {
        if (context.QUESTION() != null)
        {
            var cond = Visit(context.logicExpr());
            var ifTrue = Visit(context.expr(0));
            var ifFalse = Visit(context.expr(1));
            return $"{cond} ? {ifTrue} : {ifFalse}";
        }
        return Visit(context.logicExpr());
    }

    public override string VisitTuple(ShowScriptParser.TupleContext context)
    {
        var values = context.value().Select(Visit).ToArray();
        return $"({string.Join(", ", values)})";
    }

    public override string VisitUnaryExpr(ShowScriptParser.UnaryExprContext context)
    {
        if (context.primaryExpr() != null)
            return Visit(context.primaryExpr());
        return context.GetChild(0).GetText() + Visit(context.unaryExpr());
    }

    public override string VisitValue(ShowScriptParser.ValueContext context)
    {
        if (context.TRUE() != null) return "true";
        if (context.FALSE() != null) return "false";
        if (context.NULL() != null) return "null";
        if (context.INT() != null) return context.INT().GetText();
        if (context.FLOAT() != null) return context.FLOAT().GetText();
        if (context.STRING() != null) return context.STRING().GetText();
        if (context.ID() != null) return context.ID().GetText();
        if (context.dottedID() != null) return Visit(context.dottedID());
        if (context.array() != null) return Visit(context.array());
        if (context.tuple() != null) return Visit(context.tuple());
        return base.VisitValue(context);
    }

    public override string VisitVarDeclNoSemi(ShowScriptParser.VarDeclNoSemiContext context)
    {
        var type = MapType(context.typeName().GetText());
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        return $"{type} {name} = {expr}";
    }

    public override string VisitVarDeclStatement(ShowScriptParser.VarDeclStatementContext context)
    {
        var type = MapType(context.typeName().GetText());
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        var code = $"public {type} {name} = {expr};";
        if (!_inFunctionOrHandler)
        {
            _moduleVars.Add(code);
            return "";
        }
        return $"var {name} = {expr};";
    }

    public override string VisitWhileStatement(ShowScriptParser.WhileStatementContext context)
    {
        var cond = Visit(context.expr());
        var body = Visit(context.block());
        return $"while ({cond}) {body}";
    }

    private static void CollectDottedIds(ShowScriptParser.DottedIDContext ctx, List<string> ids)
    {
        ids.AddRange(ctx.ID().Select(id => id.GetText()));
    }

    private string GetDottedIdString(ShowScriptParser.DottedIDContext ctx)
    {
        var ids = new List<string>();
        CollectDottedIds(ctx, ids);
        return string.Join(".", ids);
    }

    private string MapType(string type)
    {
        return type switch
        {
            "Device" => "IDevice",
            _ => type
        };
    }

    private string? TryGetDottedIdOrIdAsString(ShowScriptParser.ExprContext expr)
    {
        var unary = expr.ternaryExpr()?.logicExpr()?.equalityExpr(0)
            ?.relationalExpr(0)?.additiveExpr(0)?.multiplicativeExpr(0)?.unaryExpr()[0];

        if (unary == null)
            return null;

        var atom = UnwrapToAtom(unary);

        if (atom.value() != null && atom.value().ID() != null)
            return $"\"{atom.value().ID().GetText()}\"";

        return $"\"{atom.GetText()}\"";
    }

    private ShowScriptParser.AtomContext UnwrapToAtom(ShowScriptParser.UnaryExprContext unary)
    {
        if (unary.primaryExpr() != null)
            return unary.primaryExpr().atom();
        return UnwrapToAtom(unary.unaryExpr());
    }

    private string VisitEventArgForHandler(ShowScriptParser.EventArgContext context)
    {
        if (context.dottedID() != null)
            return $"\"{VisitDottedID(context.dottedID())}\"";
        if (context.ID() != null && context.ASSIGN() != null && context.dottedID() != null)
            return $"{context.ID().GetText()} : \"{VisitDottedID(context.dottedID())}\"";
        if (context.buttonStateEnum() != null)
            return Visit(context.buttonStateEnum());
        if (context.ID() != null && context.ASSIGN() != null && context.buttonStateEnum() != null)
            return $"{context.ID().GetText()} : {Visit(context.buttonStateEnum())}";
        if (context.ID() != null && context.ASSIGN() != null && context.expr() != null)
            return $"{context.ID().GetText()} : {Visit(context.expr())}";
        if (context.expr() != null)
        {
            var expr = context.expr();
            var text = expr.GetText();
            return text;
        }
        return "";
    }

    public override string VisitAtom(ShowScriptParser.AtomContext context)
    {
        if (context.value() != null)
            return Visit(context.value());
        if (context.dottedID() != null)
            return Visit(context.dottedID());
        if (context.songCall() != null)
            return Visit(context.songCall());
        if (context.motionCall() != null)
            return Visit(context.motionCall());
        if (context.mathCall() != null)
            return Visit(context.mathCall());
        if (context.colorCall() != null)
            return Visit(context.colorCall());
        if (context.litesCall() != null)
            return Visit(context.litesCall());
        if (context.logCall() != null)
            return Visit(context.logCall());
        if (context.genericCall() != null)
            return Visit(context.genericCall());
        if (context.getCall() != null)
            return Visit(context.getCall());
        if (context.isAvailableCall() != null)
            return Visit(context.isAvailableCall());
        if (context.globalGetCall() != null)
            return Visit(context.globalGetCall());
        if (context.globalIsAvailableCall() != null)
            return Visit(context.globalIsAvailableCall());
        if (context.LPAREN() != null && context.expr() != null && context.RPAREN() != null)
            return $"({Visit(context.expr())})";
        return base.VisitAtom(context);
    }

    public override string VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
    {
        if (node.ChildCount == 1)
            return Visit(node.GetChild(0));
        return string.Join(" ", Enumerable.Range(0, node.ChildCount).Select(i => Visit(node.GetChild(i))));
    }
}