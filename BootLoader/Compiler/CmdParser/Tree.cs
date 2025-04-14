using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Muestra los componentes seleccionados en forma arborescente
    /// </summary>

    internal class Tree:LoadedStructureCommand
    {
        protected string mvarResultString = string.Empty;  
        internal Tree(CommandParser parent): base(parent) { }
        internal override string[] Tokens => new string[] { "tree", "dir" };
        internal override void invokeSecure(params string[] arguments)
        {
            if(null!=Layout)
            {
                mvarResultString = Layout.tree(mvarParent.mvarSelection);
            }            
        }
        internal override string sucessString => mvarResultString;
        internal override string HelpString => "Shows structure content. All these elements can be selected.";
        internal override string SyntaxString => string.Format("{0}", Tokens[0]);
    }
}
