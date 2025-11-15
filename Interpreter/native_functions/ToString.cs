namespace lox.native_functions;

public class ToString : LoxCallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return args[0].ToString();
    }
}