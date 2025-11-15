namespace lox.native_functions;

public class ReadLine : LoxCallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return Console.ReadLine();
    }
}