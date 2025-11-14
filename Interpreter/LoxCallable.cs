

public interface LoxCallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> args);
}