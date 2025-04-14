using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.Compiler;
using MontefaroMatias.Clients;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Estos son los elementos que admite el parser como comandos, con sus respectivas funciones.
    /// </summary>
    internal abstract class Command
    {
        protected List<commandResult> mcolResults; //Lista de resultados generados de la ejecución del código
        protected CommandParser mvarParent;
        internal abstract string[] Tokens { get; }

        internal Command(CommandParser parent)
        {
            mvarParent = parent;
            mcolResults = new List<commandResult>();
        }
        internal virtual void invoke(string[] arguments)
        {
            mcolResults = new List<commandResult>();
        }
        internal virtual string sucessString { get=> string.Empty; }
        internal List<commandResult> results { get => mcolResults; }
        internal bool match(string command)
        {
            foreach (string candidate in Tokens)
            {
                if (candidate.Equals(command, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return false;
        }
        internal abstract string HelpString { get; }
        internal abstract string SyntaxString { get; }
    }
    /// <summary>
    /// Todos estos comandos requieren que se haya cargado el código desde un XML válido.
    /// </summary>
    internal abstract class LoadedStructureCommand : Command
    {
        internal LoadedStructureCommand(CommandParser parent) : base(parent) { }
        internal override void invoke(string[] arguments)
        {
            base.invoke(arguments);
            Debug.Assert(null != mvarParent);
            if (null == Layout)
                results.Add(new errorResult(4, "The layout is empty. Try loading any before", -1));
            else
                invokeSecure(arguments);
        }
        protected LayoutSystem? Layout => mvarParent.mvarLayout;
        internal abstract void invokeSecure(string[] arguments);
    }
    /// <summary>
    /// Todos estos comandos requieren tener cargado un elemento de la estructura
    /// </summary>
    internal abstract class SelectedElementCommand : LoadedStructureCommand
    {
        internal SelectedElementCommand(CommandParser parent) : base(parent) { }
        internal override void invoke(string[] arguments)
        {            
            if (null == Selection)
                results.Add(new errorResult(5, "Is compulsory to select an element from structure.", -1));
            else
                base.invoke(arguments);
        }
        protected ClientNamedElement? Selection { get => mvarParent.mvarSelection; }
    }
}
