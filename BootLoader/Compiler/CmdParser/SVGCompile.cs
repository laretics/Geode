using BootLoader.SVG;
using MontefaroMatias.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootLoader.Compiler.CmdParser
{
    internal class SVGCompile:Command
    {
        protected string mvarResultString = string.Empty;
        internal SVGCompile(CommandParser parent): base(parent) { }
        internal override string[] Tokens => new string[] { "svg", "graph" };
        internal override void invoke(string[] arguments)
        {
            base.invoke(arguments);
            SVGCompiler compilador = new SVGCompiler(mvarParent.mvarLayout);
            string salida = compilador.Compile();
            if(salida.Length>0)
                mcolResults.Add(new errorResult(3,salida, -1));
            else
                mvarResultString = string.Format("SVG file '{0}' generated successfully", compilador.OutputFilename);
        }
        internal override string sucessString => base.sucessString;
        internal override string HelpString => string.Format("Compose a basic labeled SVG file for CTC view. If no filename is specified result will be named as '{0}}'",MontefaroMatias.LayoutView.SVGRender.DEFAULT_FILENAME);
        internal override string SyntaxString => string.Format("{0} [filename]", Tokens[0]);
    }
}
