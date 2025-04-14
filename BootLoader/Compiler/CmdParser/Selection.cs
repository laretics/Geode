using MontefaroMatias.Clients;
using MontefaroMatias.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Carga la selección.
    /// </summary>
    internal class Selection:LoadedStructureCommand
    {
        protected string mvarResultString = string.Empty;
        internal Selection(CommandParser parent): base(parent) { }
        internal override string[] Tokens => new string[] { "select", "sel", "ss", "cd" };
        internal override void invokeSecure(string[] arguments)
        {
            if (!arguments.Any())
            {
                //Sin parámetros muestro la selección actual
                if (null == mvarParent.mvarSelection)
                    mvarResultString = "There is no element selected";
                else
                    mvarResultString = string.Format("Current selected element is \"{0}\" ({1})\n", mvarParent.mvarSelection.name, mvarParent.mvarSelection.description);
            }
            else
            {
                string auxName = arguments[0];
                ClientNamedElement? elemento = Layout.search(auxName);
                if (null == elemento)
                    mcolResults.Add(new warningResult(5, string.Format("Couldn't find element {0} in structure.", auxName), -1));
                else
                {
                    mvarParent.mvarSelection = elemento;
                    mvarParent.mvarCompiledCode = null; //Al cambiar de objeto seleccionado se anula la última compilación.
                    mvarResultString = string.Format("Current selected element is \"{0}\" ({1})\n", mvarParent.mvarSelection.name, mvarParent.mvarSelection.description);
                }
            }
        }
        internal override string sucessString => mvarResultString;
        internal override string HelpString => "Select an element from structure.";
        internal override string SyntaxString => string.Format("{0} element_name", Tokens[0]);
    }
}
