
namespace lox.native_functions;


public class Clock : LoxCallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, Token paren, List<object> args) => (double)DateTimeOffset.Now.ToUnixTimeMilliseconds();
}