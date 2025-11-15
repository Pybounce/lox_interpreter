
namespace lox.native_functions;


public class Print : LoxCallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, Token paren, List<object> args)
    {
        Console.Write(args[0].ToString());
        return null;
    }
}