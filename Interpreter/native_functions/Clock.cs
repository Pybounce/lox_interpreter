

public class NativeClock : LoxCallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> args) => (double)DateTimeOffset.Now.ToUnixTimeMilliseconds();
}