
namespace lox.native_functions;


public class PrintLine : LoxCallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, Token paren, List<object> args)
    {
        Console.WriteLine(args[0].ToString());
        return null;
    }
}