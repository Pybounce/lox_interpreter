namespace lox.native_functions;

public class ToDouble : LoxCallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, Token paren, List<object> args)
    {
        if (double.TryParse(args[0].ToString(), out var d))
        {
            return d;
        }
        throw new RuntimeError(paren, "Failed to parse to double");
    }
}