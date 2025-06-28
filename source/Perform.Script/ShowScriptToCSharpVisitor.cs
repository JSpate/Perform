using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Perform.Script;

public class ShowScriptToCSharpVisitor(ShowScript showScript) : ShowScriptBaseVisitor<string>
{
    private readonly List<string> _eventHandlerClasses = new();
    private readonly List<string> _eventHandlerRegistrations = new();
    private bool _inFunctionOrHandler;
    private readonly List<string> _moduleConstants = new();
    private readonly List<string> _moduleVars = new();
    private int _eventHandlerCounter;

    private int _loopNestingLevel;

    private readonly List<string> _functionFields = new();
    private readonly List<string> _functionInitializations = new();
    private readonly List<string> _functionLoops = new();
    private int _functionCounter;

    private readonly Stack<HashSet<string>> _localVars = new();
    private readonly Stack<(string loopVar, string collectionRoot)> _foreachContext = new();

    private string RewriteDottedId(ShowScriptParser.DottedIDContext dotted)
    {
        var ids = dotted.idWithIndex();
        if (_foreachContext.Count > 0 && ids.Length == 2)
        {
            var (loopVar, collectionRoot) = _foreachContext.Peek();
            if (ids[0].GetText() == loopVar)
            {
                var property = ids[1].GetText();
                var parent = dotted.Parent;
                bool isAssignment = parent is ShowScriptParser.AssignmentContext assignCtx && assignCtx.dottedID() == dotted;
                if (!isAssignment)
                {
                    var path = $"{collectionRoot}.{{{loopVar}}}.{property}";
                    var typePath = $"{collectionRoot}.*.{property}";
                    return $"showScript.Get<{GetTypeForPath(typePath)}>($\"{path}\")";
                }
            }
        }
        if (ids.Length > 1)
        {
            var first = ids[0].GetText();
            var rest = ids.Skip(1).Select(id => id.GetText()).ToArray();
            if (_localVars.Any() && _localVars.Any(set => set.Contains(first)))
            {
                var expr = first;
                foreach (var prop in rest)
                    expr = $"{expr}.Get(\"{prop}\")";
                return expr;
            }
        }
        return dotted.GetText();
    }

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

    public override string VisitIfLoopStatement(ShowScriptParser.IfLoopStatementContext context)
    {
        var cond = Visit(context.expr());
        var thenBlock = Visit(context.loopStatementsBlock(0));
        var sb = new StringBuilder();
        sb.Append($"if ({cond}) {thenBlock}");

        if (context.ELSE() != null)
        {
            if (context.ifLoopStatement() != null)
            {
                sb.Append(" else ");
                sb.Append(Visit(context.ifLoopStatement()));
            }
            else if (context.loopStatementsBlock().Length > 1)
            {
                sb.Append(" else ");
                sb.Append(Visit(context.loopStatementsBlock(1)));
            }
        }

        return sb.ToString();
    }

    public override string VisitForLoopStatement(ShowScriptParser.ForLoopStatementContext context)
    {
        var init = "";
        if (context.letStatementNoSemi() != null && context.letStatementNoSemi().Length > 0)
            init = Visit(context.letStatementNoSemi(0));
        else if (context.assignmentNoSemi() != null && context.assignmentNoSemi().Length > 0)
            init = Visit(context.assignmentNoSemi(0));
        else if (context.varDeclNoSemi() != null && context.varDeclNoSemi().Length > 0)
            init = Visit(context.varDeclNoSemi(0));

        var cond = context.expr() != null && context.expr().Length > 0 ? Visit(context.expr(0)) : "";

        string inc = "";
        if (context.letStatementNoSemi() != null && context.letStatementNoSemi().Length > 1)
            inc = Visit(context.letStatementNoSemi(1));
        else if (context.assignmentNoSemi() != null && context.assignmentNoSemi().Length > 1)
            inc = Visit(context.assignmentNoSemi(1));
        else if (context.varDeclNoSemi() != null && context.varDeclNoSemi().Length > 1)
            inc = Visit(context.varDeclNoSemi(1));
        else if (context.expr() != null && context.expr().Length > 1)
            inc = Visit(context.expr(1));

        _loopNestingLevel++;
        var body = Visit(context.loopStatementsBlock());
        _loopNestingLevel--;
        var bodyBlock = body.Trim();
        if (!bodyBlock.StartsWith("{"))
            bodyBlock = "{\n" + bodyBlock + "\n}";
        return $"for ({init}; {cond}; {inc}) {bodyBlock}";
    }

    public override string VisitForeachLoopStatement(ShowScriptParser.ForeachLoopStatementContext context)
    {
        var varName = context.ID().GetText();
        var collectionStr = GetDottedIdString(context.dottedID());
        var type = GetTypeForPath(collectionStr);
        var collection = $"showScript.Get<{type}>(\"{collectionStr}\")";
        _foreachContext.Push((varName, collectionStr));
        var body = Visit(context.loopStatementsBlock());
        _foreachContext.Pop();
        return $"foreach (var {varName} in {collection}) {body}";
    }

    public override string VisitForeachStatement(ShowScriptParser.ForeachStatementContext context)
    {
        var varName = context.ID().GetText();
        var collectionStr = GetDottedIdString(context.dottedID());
        var type = GetTypeForPath(collectionStr);
        var collection = $"showScript.Get<{type}>(\"{collectionStr}\")";
        _foreachContext.Push((varName, collectionStr));
        var body = Visit(context.block());
        _foreachContext.Pop();
        return $"foreach (var {varName} in {collection}) {body}";
    }

    public override string VisitEndLoopStatement(ShowScriptParser.EndLoopStatementContext context)
    {
        return "_status = ScriptLoopStatus.EndLoop;\nreturn;";
    }

    public override string VisitArray(ShowScriptParser.ArrayContext context)
    {
        var tuples = context.tuple().Select(Visit).ToArray();
        return $"new List<int[]> {{ {string.Join(", ", tuples)} }}";
    }

    public override string VisitAssignment(ShowScriptParser.AssignmentContext context)
    {
        var dotted = context.dottedID();
        var ids = dotted.idWithIndex();
        var exprText = Visit(context.expr());

        if (_foreachContext.Count > 0 && ids.Length >= 2)
        {
            var (loopVar, collectionRoot) = _foreachContext.Peek();
            if (ids[0].GetText() == loopVar)
            {
                var propertyPath = string.Join(".", ids.Skip(1).Select(id => id.GetText()));
                var path = $"{collectionRoot}.{{{loopVar}}}.{propertyPath}";
                var typePath = $"{collectionRoot}.*.{propertyPath}";
                return $"showScript.Set<{GetTypeForPath(typePath)}>($\"{path}\", {exprText});";
            }
        }

        if (ids.Length > 1)
        {
            var first = ids[0].GetText();
            var rest = ids.Skip(1).Select(id => id.GetText()).ToArray();
            if (_localVars.Any() && _localVars.Any(set => set.Contains(first)))
            {
                if (rest.Length == 1)
                    return $"{first}.Set(\"{rest[0]}\") = {exprText};";
                var expr = first;
                for (int i = 0; i < rest.Length - 1; i++)
                    expr = $"{expr}.Get(\"{rest[i]}\")";
                return $"{expr}.Set(\"{rest.Last()}\") = {exprText};";
            }
        }
        var name = RewriteDottedId(dotted);
        return $"{name} = {exprText};";
    }

    public override string VisitAssignmentNoSemi(ShowScriptParser.AssignmentNoSemiContext context)
    {
        var dotted = context.dottedID();
        var ids = dotted.idWithIndex();
        var exprText = Visit(context.expr());

        if (_foreachContext.Count > 0 && ids.Length >= 2)
        {
            var (loopVar, collectionRoot) = _foreachContext.Peek();
            if (ids[0].GetText() == loopVar)
            {
                var propertyPath = string.Join(".", ids.Skip(1).Select(id => id.GetText()));
                var path = $"{collectionRoot}.{{{loopVar}}}.{propertyPath}";
                var typePath = $"{collectionRoot}.*.{propertyPath}";
                return $"showScript.Set<{GetTypeForPath(typePath)}>($\"{path}\", {exprText})";
            }
        }

        if (ids.Length > 1)
        {
            var first = ids[0].GetText();
            var rest = ids.Skip(1).Select(id => id.GetText()).ToArray();
            if (_localVars.Any() && _localVars.Any(set => set.Contains(first)))
            {
                if (rest.Length == 1)
                    return $"{first}.Set(\"{rest[0]}\") = {exprText}";
                var expr = first;
                for (int i = 0; i < rest.Length - 1; i++)
                    expr = $"{expr}.Get(\"{rest[i]}\")";
                return $"{expr}.Set(\"{rest.Last()}\") = {exprText}";
            }
        }
        var name = RewriteDottedId(dotted);
        return $"{name} = {exprText}";
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
        _localVars.Push(new HashSet<string>());
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
        _localVars.Pop();
        return sb.ToString();
    }

    public override string VisitLoopStatementsBlock(ShowScriptParser.LoopStatementsBlockContext context)
    {
        _localVars.Push(new HashSet<string>());
        var sb = new StringBuilder();
        sb.AppendLine("{");
        foreach (var stmt in context.loopStatement())
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
        _localVars.Pop();
        return sb.ToString();
    }

    public override string VisitBreakStatement(ShowScriptParser.BreakStatementContext context)
    {
        return "break;";
    }

    public override string VisitContinueStatement(ShowScriptParser.ContinueStatementContext context)
    {
        return _loopNestingLevel == 0
            ? "return;"
            : "continue;";
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
            code = $"private const int {name} = {expr};";
        else if (expr == "true" || expr == "false")
            code = $"private const bool {name} = {expr};";
        else if (expr.StartsWith("\"") && expr.EndsWith("\""))
            code = $"private const string {name} = {expr};";
        else
            code = $"private static readonly dynamic {name} = {expr};";

        if (!_inFunctionOrHandler)
        {
            _moduleConstants.Add(code);
            return "";
        }
        return code;
    }

    public override string VisitDottedID(ShowScriptParser.DottedIDContext context)
    {
        return RewriteDottedId(context);
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
        _functionFields.Clear();
        _functionInitializations.Clear();
        _functionLoops.Clear();
        _functionCounter = 0;

        var handlerClassName = $"EventHandler{++_eventHandlerCounter:0000}";
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

        string flatBlockCode;
        var trimmedBlock = blockCode.Trim();
        if (trimmedBlock.StartsWith("{") && trimmedBlock.EndsWith("}"))
        {
            flatBlockCode = trimmedBlock.Substring(1, trimmedBlock.Length - 2).Trim('\r', '\n');
        }
        else
        {
            flatBlockCode = trimmedBlock;
        }

        string loopBlockCode;
        bool hasLoopContent = false;
        if (context.loopBlock() != null)
        {
            _loopNestingLevel = 0;
            var loopBody = Visit(context.loopBlock());

            var loopBodyBlock = loopBody.Trim();
            if (!loopBodyBlock.StartsWith("{"))
                loopBodyBlock = "{\n" + loopBodyBlock + "\n}";

            var loopLines = string.Join("\n", _functionLoops.Select(l => "        " + l));
            if (!string.IsNullOrWhiteSpace(loopLines))
            {
                var braceIdx = loopBodyBlock.IndexOf('{') + 1;
                loopBodyBlock = loopBodyBlock.Insert(braceIdx, "\n" + loopLines + "\n");
            }

            var lines = loopBodyBlock.Split('\n')
                .Where(line => !line.TrimStart().StartsWith("return ScriptLoopResult.Continue;") &&
                               !line.TrimStart().StartsWith("return ScriptLoopResult.EndLoop;"))
                .ToArray();
            loopBodyBlock = string.Join("\n", lines);

            hasLoopContent = lines.Any(line => !line.Trim().Equals("{") && !line.Trim().Equals("}"));

            loopBlockCode = $@"
    public void Loop()
{IndentBlock(loopBodyBlock, 1)}
";
        }
        else
        {
            var loopLines = string.Join("\n", _functionLoops.Select(l => "        " + l));
            hasLoopContent = !string.IsNullOrWhiteSpace(loopLines);
            loopBlockCode = $@"
    public void Loop()
    {{
{loopLines}
    }}
";
        }

        string finallyBlockCode;
        if (context.loopFinallyBlock() != null)
        {
            var finallyBody = Visit(context.loopFinallyBlock());
            var trimmedFinally = finallyBody.Trim();
            if (trimmedFinally.StartsWith("finally"))
                trimmedFinally = trimmedFinally.Substring("finally".Length).Trim();
            if (trimmedFinally.StartsWith("{") && trimmedFinally.EndsWith("}"))
                trimmedFinally = trimmedFinally.Substring(1, trimmedFinally.Length - 2).Trim('\r', '\n');
            finallyBlockCode = $@"
    public void Finally()
    {{
{IndentBlock(trimmedFinally, 1)}
        _status = ScriptLoopStatus.Finished;
    }}
";
        }
        else
        {
            finallyBlockCode = $@"
    public void Finally()
    {{
        _status = ScriptLoopStatus.Finished;
    }}
";
        }

        var statusInit = hasLoopContent ? "ScriptLoopStatus.Continue" : "ScriptLoopStatus.EndLoop";
        var statusField = $"private ScriptLoopStatus _status = {statusInit};";
        var statusProperty = @"
    public ScriptLoopStatus Status => _status;
";

        var handlerClass = $@"
private class {handlerClassName}(ShowScript showScript, ILogger logger) : IScriptEventHandler
{{
{(_functionFields.Count > 0 ? "    " + string.Join("\n    ", _functionFields) : "")}
    {statusField}

    public void Initialize()
    {{
{IndentBlock(flatBlockCode, 1)}
{(_functionInitializations.Count > 0 ? IndentBlock(string.Join("\n", _functionInitializations), 1) : "")}
    }}
{loopBlockCode}
{finallyBlockCode}
{statusProperty}
}}
";
        _eventHandlerClasses.Add(handlerClass);

        var eventTypeEnum = $"EventType.{eventType}";
        var argsList = eventArgs.Count > 0 ? ", " + string.Join(", ", eventArgs) : "";
        var registration = $"typeof({handlerClassName}), {eventTypeEnum}{argsList}";
        _eventHandlerRegistrations.Add(registration);

        _inFunctionOrHandler = false;
        return "";
    }

    public override string VisitLoopBlock(ShowScriptParser.LoopBlockContext context)
    {
        var loopBody = Visit(context.loopStatementsBlock());
        return loopBody;
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

        _loopNestingLevel++;
        var body = Visit(context.block());
        _loopNestingLevel--;
        var bodyBlock = body.Trim();
        if (!bodyBlock.StartsWith("{"))
            bodyBlock = "{\n" + bodyBlock + "\n}";
        return $"for ({init}; {cond}; {inc}) {bodyBlock}";
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
        return $"showScript.Get<{GetTypeForPath(arg)}>({arg})";
    }

    public override string VisitGlobalIsAvailableCall(ShowScriptParser.GlobalIsAvailableCallContext context)
    {
        var expr = context.expr();
        var arg = TryGetDottedIdOrIdAsString(expr) ?? Visit(expr);
        return $"showScript.IsAvailable({arg})";
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
        if (_localVars.Count > 0)
            _localVars.Peek().Add(name);
        var code = $"private static dynamic {name} = {expr};";
        if (!_inFunctionOrHandler)
        {
            _moduleVars.Add(code);
            return "";
        }
        return $"var {name} = {expr};";
    }

    public override string VisitLetStatementNoSemi(ShowScriptParser.LetStatementNoSemiContext context)
    {
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        if (_localVars.Count > 0)
            _localVars.Peek().Add(name);
        return $"var {name} = {expr}";
    }

    public override string VisitLitesCall(ShowScriptParser.LitesCallContext context)
    {
        var litesArgs = context.litesArgs().dottedID();
        var liteNames = litesArgs.Select(arg =>
        {
            var ids = new List<string>();
            CollectDottedIds(arg, ids);
            return $"\"{string.Join(".", ids)}\"";
        });
        var litesArray = $"[{string.Join(", ", liteNames)}]";

        var funcName = context.litesFunction().Start.Text;
        var className = $"Perform.Scripting.Functions.{funcName}";

        var funcText = context.litesFunction().GetText();
        var argsStart = funcText.IndexOf('(');
        var args = argsStart >= 0 ? funcText.Substring(argsStart + 1, funcText.Length - argsStart - 2) : "";

        var fieldName = $"_function{++_functionCounter:D4}";
        _functionFields.Add($"private IDeviceScriptFunction? {fieldName};");
        _functionInitializations.Add($"{fieldName} = new {className}({args});\n{fieldName}.Initialize(showScript, {litesArray});");
        _functionLoops.Add($"if ({fieldName} is {{ IsFinished: false }}) {fieldName}.Loop();");

        return "";
    }

    public override string VisitMotionCall(ShowScriptParser.MotionCallContext context)
    {
        var motionArgs = context.motionArgs().dottedID();
        var motionNames = motionArgs.Select(arg =>
        {
            var ids = new List<string>();
            CollectDottedIds(arg, ids);
            return $"\"{string.Join(".", ids)}\"";
        });
        var motionArray = $"[{string.Join(", ", motionNames)}]";

        var funcName = context.motionFunction().Start.Text;
        var className = $"Perform.Scripting.Functions.{funcName}";

        var funcText = context.motionFunction().GetText();
        var argsStart = funcText.IndexOf('(');
        var args = argsStart >= 0 ? funcText.Substring(argsStart + 1, funcText.Length - argsStart - 2) : "";

        var fieldName = $"_function{++_functionCounter:D4}";
        _functionFields.Add($"private IDeviceScriptFunction? {fieldName};");
        _functionInitializations.Add($"{fieldName} = new {className}({args});\n{fieldName}.Initialize(showScript, {motionArray});");
        _functionLoops.Add($"if ({fieldName} is {{ IsFinished: false }}) {fieldName}.Loop();");

        return "";
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
        return $"logger.{logCall}({arg})";
    }

    public override string VisitMathCall(ShowScriptParser.MathCallContext context)
    {
        return $"Math.{context.mathFunction().GetText()}";
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
                result = $"{result}[{exprText}]";
                i += 3;
            }
            else if (child.GetText() == ".")
            {
                var next = context.GetChild(i + 1);

                if (_foreachContext.Count > 0 && next is Antlr4.Runtime.Tree.ITerminalNode terminalNode)
                {
                    var property = terminalNode.GetText();
                    var (loopVar, collectionRoot) = _foreachContext.Peek();

                    if (result.Trim() == loopVar)
                    {
                        var parent = context.Parent;
                        bool isAssignment = false;
                        if (parent is ShowScriptParser.AssignmentContext assignCtx && assignCtx.dottedID() != null)
                        {
                            var ids = assignCtx.dottedID().idWithIndex();
                            if (ids.Length == 2 && ids[0].GetText() == loopVar && ids[1].GetText() == property)
                                isAssignment = true;
                        }
                        if (!isAssignment)
                        {
                            var typePath = $"{collectionRoot}.*.{property}";
                            result = $"showScript.Get<{GetTypeForPath(typePath)}>($\"{collectionRoot}.{{{loopVar}}}.{property}\")";
                            i += 2;
                            continue;
                        }
                    }
                }

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
        _eventHandlerClasses.Clear();
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
        sb.AppendLine("using Perform.Interfaces;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Math = Perform.Scripting.MathMethods;");
        sb.AppendLine();
        sb.AppendLine("public class GeneratedShowScript");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ShowScript _showScript;");
        sb.AppendLine("    private readonly ILogger _logger;");
        sb.AppendLine();
        foreach (var c in _moduleConstants)
            sb.AppendLine("    " + c);
        foreach (var v in _moduleVars)
            sb.AppendLine("    " + v);
        sb.AppendLine();
        sb.AppendLine("    public GeneratedShowScript(ShowScript showScript, ILogger logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _showScript = showScript;");
        sb.AppendLine("        _logger = logger;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public void Initialise()");
        sb.AppendLine("    {");
        foreach (var reg in _eventHandlerRegistrations)
            sb.AppendLine($"        _showScript.AddEventHandler({reg});");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public void Finalize() {");
        foreach (var reg in _eventHandlerRegistrations)
            sb.AppendLine($"        _showScript.RemoveEventHandler({reg});");
        sb.AppendLine("    }");

        foreach (var handlerClass in _eventHandlerClasses)
            sb.AppendLine("    " + handlerClass.Replace("\n", "\n    "));

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
                sb.AppendLine($"showScript.Set<{GetTypeForPath(arg)}>(\"{arg}\", {value});");
            }
        }

        return sb.ToString();
    }

    public override string VisitSongCall(ShowScriptParser.SongCallContext context)
    {
        return $"showScript.CurrentSong.{context.songFunction().GetText().TrimEnd('(', ')')}";
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
        return $"new[]{{{string.Join(", ", values)}}}";
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
        if (context.dottedID() != null) return RewriteDottedId(context.dottedID());
        if (context.array() != null) return Visit(context.array());
        if (context.tuple() != null) return Visit(context.tuple());
        return base.VisitValue(context);
    }

    public override string VisitVarDeclNoSemi(ShowScriptParser.VarDeclNoSemiContext context)
    {
        var type = MapType(context.typeName().GetText());
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        if (_localVars.Count > 0)
            _localVars.Peek().Add(name);
        return $"{type} {name} = {expr}";
    }

    public override string VisitVarDeclStatement(ShowScriptParser.VarDeclStatementContext context)
    {
        var type = MapType(context.typeName().GetText());
        var name = context.ID().GetText();
        var expr = Visit(context.expr());
        if (_localVars.Count > 0)
            _localVars.Peek().Add(name);
        var code = $"private static {type} {name} = {expr};";
        if (!_inFunctionOrHandler)
        {
            _moduleVars.Add(code);
            return "";
        }
        return $"var {name} = {expr};";
    }

    private static void CollectDottedIds(ShowScriptParser.DottedIDContext ctx, List<string> ids)
    {
        ids.AddRange(ctx.idWithIndex().Select(id => id.GetText()));
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

    private static string IndentBlock(string code, int level)
    {
        var indent = new string(' ', level * 4);
        var lines = code.Split('\n');
        return string.Join("\n", lines.Select(line => string.IsNullOrWhiteSpace(line) ? line : indent + line));
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

    private string GetTypeForPath(string path)
    {
        return showScript.TypeFor(path);
    }
}