
using System.Runtime.InteropServices;

public readonly struct Nothing { }

public class StackEvent
{
    public class StatementEvent: StackEvent
    {
        public Stmt Statement;
        public StatementEvent(Stmt statement)
        {
            this.Statement = statement;
        }
    }
    public class UnwrapEnvironment: StackEvent { }

}
public class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<Nothing>
{
    public readonly Environment Globals = new Environment();
    private Environment _environment;

    private Stack<StackEvent> _statementStack = new Stack<StackEvent>();
    public Interpreter()
    {
        Globals.Define("clock", new lox.native_functions.Clock());
        Globals.Define("read_line", new lox.native_functions.ReadLine());
        Globals.Define("random_range_rounded", new lox.native_functions.RandomRangeRounded());
        Globals.Define("to_number", new lox.native_functions.ToNumber());
        Globals.Define("to_string", new lox.native_functions.ToString());
        Globals.Define("clear", new lox.native_functions.Clear());
        Globals.Define("print", new lox.native_functions.Print());
        Globals.Define("print_line", new lox.native_functions.PrintLine());
        Globals.Define("is_number", new lox.native_functions.IsNumber());
        Globals.Define("read_key", new lox.native_functions.ReadKey());
        Globals.Define("try_read_key", new lox.native_functions.TryReadKey());

        _environment = Globals;
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            statements.Reverse();
            foreach (var stmt in statements)
            {
                _statementStack.Push(new StackEvent.StatementEvent(stmt));
            }
            while (_statementStack.Count() > 0)
            {
                ExecuteStackEvent(_statementStack.Peek());
            }
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    public void ExecuteStackEvent(StackEvent stackEvent) {
        if (stackEvent is StackEvent.StatementEvent)
        {
            var stmtEvent = (StackEvent.StatementEvent)stackEvent;
            Execute(stmtEvent.Statement);
        }
        else if (stackEvent is StackEvent.UnwrapEnvironment)
        {
            _statementStack.Pop();
            if (_environment._enclosing != null)
            {
                _environment = _environment._enclosing;
            }
        }
    }

    private string Stringify(object obj)
    {
        if (obj == null) { return "nil"; }
        if (obj is double)
        {
            var text = obj.ToString();
            if (text.EndsWith(".0")) { text = text.Substring(0, text.Length - 2); }
            return text;
        }
        return obj.ToString();
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.left);
        var right = Evaluate(expr.right);

        switch (expr.op.TokenType)
        {
            case TokenType.MINUS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left - (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.op, left, right);
                return (double)left * (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr.op, left, right);
                return (double)left / (double)right;
            case TokenType.PLUS:
                if (left is double && right is double) { return (double)left + (double)right; }
                if (left is string && right is string) { return (string)left + (string)right; }
                throw new RuntimeError(expr.op, "Operands must both be numbers or strings.");
            case TokenType.GREATER:
                CheckNumberOperands(expr.op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left <= (double)right;
            case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            case TokenType.BANG_EQUAL: return !IsEqual(left, right);
        }

        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.right);
        switch (expr.op.TokenType)
        {
            case TokenType.MINUS:
                CheckNumberOperand(expr.op, right);
                return -(double)right;
            case TokenType.BANG:
                return !IsTruthy(right);
        }
        return null;
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private bool IsTruthy(Object obj)
    {
        if (obj == null) { return false; }
        if (obj is bool) { return (bool)obj; }
        return true;
    }

    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) { return true; }
        if (a == null) { return false; }
        return a.Equals(b);
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double) { return; }
        throw new RuntimeError(op, "Operand must be a number.");
    }
    private void CheckNumberOperands(Token op, object leftOperand, object rightOperand)
    {
        if (leftOperand is double && rightOperand is double) { return; }
        throw new RuntimeError(op, "Operands must both be numbers.");
    }

    public Nothing VisitExpressionStmt(Stmt.Expression stmt)
    {
        _statementStack.Pop();
        Evaluate(stmt.expression);
        return new Nothing();
    }

    public Nothing VisitVarStmt(Stmt.Var stmt)
    {
        Console.WriteLine("var");
        object value = null;
        if (stmt.initialiser != null)
        {
            value = Evaluate(stmt.initialiser);
        }
        _statementStack.Pop();
        _environment.Define(stmt.name.Lexeme, value);
        return new Nothing();
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return _environment.Get(expr.name);
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.value);
        _environment.Assign(expr.name, value);
        return value;
    }

    public Nothing VisitBlockStmt(Stmt.Block stmt)
    {
        Console.WriteLine("pop block");
        _statementStack.Pop();
        ExecuteBlock(stmt.statements, new Environment(_environment));
        return new Nothing();
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        _environment = environment;
        _statementStack.Push(new StackEvent.UnwrapEnvironment{});
        for (int i = statements.Count - 1; i >= 0; i--)
        {
            Console.WriteLine("add some block statement");
            _statementStack.Push(new StackEvent.StatementEvent(statements[i]));
        }
    }

    public Nothing VisitIfStmt(Stmt.If stmt)
    {
        _statementStack.Pop();
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            _statementStack.Push(new StackEvent.StatementEvent(stmt.ThenBranch));
        }
        else if (stmt.ElseBranch != null)
        {
            _statementStack.Push(new StackEvent.StatementEvent(stmt.ElseBranch));
        }
        return new Nothing();
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.Op.TokenType == TokenType.OR)
        {
            if (IsTruthy(left)) { return left; }
        }
        else
        {
            if (!IsTruthy(left)) { return left; }
        }

        return Evaluate(expr.Right);
    }

    public Nothing VisitWhileStmt(Stmt.While stmt)
    {
        if (!IsTruthy(Evaluate(stmt.Condition)))
        {
            Console.WriteLine("while stopped");
            _statementStack.Pop();
        }
        else
        {
                        Console.WriteLine("while going");

            _statementStack.Push(new StackEvent.StatementEvent(stmt.Body));
        }

        return new Nothing();
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);
        var arguments = new List<object>();
        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (!(callee is LoxCallable)) 
        { 
            throw new RuntimeError(expr.Paren, "Can only call functions and classes."); 
        }

        var function = (LoxCallable)callee;

        if (function.Arity() != arguments.Count)
        {
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
        }

        return function.Call(this, expr.Paren, arguments);
    }

    public Nothing VisitFunctionStmt(Stmt.Function stmt)
    {
        _statementStack.Pop();
        var function = new LoxFunction(stmt, _environment);
        _environment.Define(stmt.Name.Lexeme, function);
        return new Nothing();
    }

    public Nothing VisitReturnStmt(Stmt.Return stmt)
    {
        object val = null;
        if (stmt.expression != null) { val = Evaluate(stmt.expression); }

        throw new Return(val);
    }
}