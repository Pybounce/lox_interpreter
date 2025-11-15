namespace lox.native_functions;

public class ToDouble : LoxCallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return double.Parse(args[0].ToString());
    }
}