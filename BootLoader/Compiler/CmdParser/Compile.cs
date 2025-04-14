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
    /// Compilación del código.
    /// </summary>
    internal class Compile:SelectedElementCommand
    {
        protected string mvarResultString=string.Empty;
        internal Compile(CommandParser parent) : base(parent) { }
        internal override string[] Tokens => new string[] { "compile", "com" };
        internal override void invokeSecure(params string[] arguments)
        {
            mcolResults.Clear();
            mvarParent.mvarCompiledCode = null;
            Debug.Assert(null != Selection);
            MemoDump salida = Selection.code;
            if (salida.Errors.Count > 0)
            {
                mcolResults.Clear();
                mcolResults.AddRange(salida.Errors);
            }
            else
            {
                mcolResults.Add(new sucessResult());
                mvarResultString = salida.ToString();
                if (Selection is Client) //Importante: Sólo podemos volcar a Arduino los clientes.
                    mvarParent.mvarCompiledCode = salida;
            }
        }
        internal override string sucessString => mvarResultString;
        internal override string HelpString => "Compile the selected element";
        internal override string SyntaxString => string.Format("{0}", Tokens[0]);
    }
}
