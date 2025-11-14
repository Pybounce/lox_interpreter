


public class LoxFunction : LoxCallable
{
    private readonly Stmt.Function _declaration;

    public LoxFunction(Stmt.Function declaration)
    {
        _declaration = declaration;
    }

    public int Arity()
    {
        return _declaration.Params.Count;
    }

    public object Call(Interpreter interpreter, List<object> args)
    {
        var environment = new Environment(interpreter.Globals);
        for (int i = 0; i < _declaration.Params.Count; i++)
        {
            environment.Define(_declaration.Params[i].Lexeme, args[i]);
        }
        interpreter.ExecuteBlock(_declaration.Body, environment);
        return null;
    }
}