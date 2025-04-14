using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Describe la estructura completa o un determinado elemento.
    /// En la descripción de un elemento, cualquier otro parámetro hará que sea detallada.
    /// </summary>
    internal class Describe:SelectedElementCommand
    {
        protected string mvarSucessString = string.Empty;
        internal Describe(CommandParser parent): base(parent) { }
        internal override string[] Tokens => new string[] { "describe", "des" };
        internal override void invokeSecure(params string[] arguments)
        {
            Debug.Assert(null != mvarParent && null != Layout);
            if (null == Selection)
            {
                mvarSucessString = "Cant describe null element.";
            }                
            else
            {
                if (arguments.Length > 0)
                    mvarSucessString = Selection.detailed;
                else
                    mvarSucessString = Selection.description;
            }
        }
        internal override string sucessString => mvarSucessString;
        internal override string HelpString => "Shows selected element description.";
        internal override string SyntaxString => string.Format("{0} / {0} full (for detailed description)", Tokens[0]);
    }
}
