using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.Compiler;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Orden auxiliar para terminar la sesión.
    /// No sería estrictamente necesario para terminar el programa, pero viene bien.
    /// </summary>
    internal class Quit : Command
    {
        internal Quit(CommandParser parent) : base(parent) { }
        internal override string[] Tokens => new string[] { "quit", "exit" };
        internal override void invoke(params string[] arguments)
        {
            base.invoke(arguments);
            mcolResults.Add(new sucessResult());
            mvarParent.mvarActive = false;
        }
        internal override string sucessString => "Goodbye!";
        internal override string HelpString => "Terminate session and closes program.";
        internal override string SyntaxString => string.Format("{0}", Tokens[0]);
    }
    /// <summary>
    /// Orden auxiliar para borrar la pantalla.
    /// </summary>
    internal class ClearScreenCommand : Command
    {
        internal ClearScreenCommand(CommandParser parent) : base(parent) { }
        internal override string[] Tokens => new string[] { "cls" };
        internal override void invoke(params string[] arguments)
        {
            base.invoke(arguments);
            Console.Clear();
            mcolResults.Add(new sucessResult());
        }
        internal override string HelpString => "Clear screen content.";
        internal override string SyntaxString => string.Format("{0}", Tokens[0]);
    }
}
