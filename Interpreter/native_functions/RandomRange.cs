namespace lox.native_functions;

public class RandomRange : LoxCallable
{
    public int Arity() => 2;

    public object Call(Interpreter interpreter, List<object> args)
    {
        var rng = new Random();
        return (double)rng.Next((int)Math.Round((double)args[0]), (int)Math.Round((double)args[1]));
    }
}