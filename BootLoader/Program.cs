//Generador de configuraciones para la estructura mtfComm
using BootLoader.Compiler.CmdParser;

public class Program
{
    private static CommandParser parser = new BootLoader.Compiler.CmdParser.CommandParser();
    public static void Main(string[] args)
    {
        while (parser.Active)
        {
            Console.Write(parser.Prompt);
            Console.WriteLine(parser.parse(Console.ReadLine()));
        }        
    }
}