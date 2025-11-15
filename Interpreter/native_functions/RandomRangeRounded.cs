namespace lox.native_functions;

public class RandomRangeRounded : LoxCallable
{
    public int Arity() => 2;

    public object Call(Interpreter interpreter, Token paren, List<object> args)
    {
        var rng = new Random();
        return (double)rng.Next((int)Math.Round((double)args[0]), (int)Math.Round((double)args[1]));
    }
}