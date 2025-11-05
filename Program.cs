

using System.Net;

class Lox
{
    static void Main(string[] args)
    {
        if (args.Length > 1) { throw new Exception("Why in the seven hells are you adding multiple arguments ARE YOU CRAZY"); }
        else if (args.Length == 1) {
            RunFile(args[0]);
        }
        else {
            RunPrompt();
        }
    }



    static void RunFile(String path)
    {

    }

    static void RunPrompt()
    {
        
    }
}

