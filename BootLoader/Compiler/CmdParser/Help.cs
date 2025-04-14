using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.Compiler;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Comando para obtener ayuda del resto de comandos.
    /// </summary>
    internal class Help:Command
    {
        protected string mvarResultString = string.Empty;
        internal Help(CommandParser parent) : base(parent) { }
        internal override string[] Tokens => new string[] { "help", "h", "?" };
        internal override void invoke(params string[] arguments)
        {
            base.invoke(arguments);
            mcolResults.Add(new sucessResult());
            if (arguments.Length > 0)
            {
                //Ayuda sobre un determinado comando
                Command? auxCommand = mvarParent.findCommand(arguments[0]);
                if (null == auxCommand)
                    mcolResults.Add(new warningResult(0, string.Format("Could not find token {0}.", arguments[0]), -1));
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Command {0}", auxCommand.Tokens[0]);
                    if (auxCommand.Tokens.Length > 1)
                    {
                        sb.Append(" (variants: ");
                        for (int i = 1; i < auxCommand.Tokens.Length; i++)
                        {
                            sb.Append(auxCommand.Tokens[i]);
                            sb.Append(' ');
                        }
                        sb.Append(')');
                    }
                    sb.Append(":");
                    sb.AppendLine();
                    sb.AppendFormat("Syntax: {0} \nFunction: {1}", auxCommand.SyntaxString, auxCommand.HelpString);
                    sb.AppendLine();
                    mvarResultString = sb.ToString();
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                List<string> auxLista = new List<string>();
                foreach (Command token in mvarParent.mcolCommandTokens)
                {
                    foreach (string auxComando in token.Tokens)
                        auxLista.Add(auxComando);
                }
                auxLista.Sort();
                sb.AppendLine("Commands list");
                foreach (string auxCommand in auxLista)
                {
                    sb.AppendFormat("\t- {0}", auxCommand);
                    sb.AppendLine();
                }
                mvarResultString = sb.ToString();
            }
        }
        internal override string sucessString => mvarResultString;
        internal override string HelpString => "Show all the available commands list or help about one of them";
        internal override string SyntaxString => string.Format("{0} / {0} command_id", Tokens[0]);
    }
}
