using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.Compiler;
using MontefaroMatias.XML;
namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Carga de un XML en la estructura.
    /// Es el primer comando necesario para trabajar.
    /// </summary>
    internal class Load:Command
    {
        protected string mvarResultString = string.Empty;
        internal Load(CommandParser parent) : base(parent) { }
        internal override string[] Tokens => new string[] { "load", "ld" };
        internal override void invoke(params string[] arguments)
        {
            base.invoke(arguments);
            mvarParent.mvarSelection = null; //Al cambiar el archivo vaciaré la selección.
            if (null == arguments || !arguments.Any())
                mcolResults.Add(new errorResult(1, "Wrong parameter", -1));
            else
            {
                string fileName = arguments[0];
                XMLImporter auxImporter = new XMLImporter();
                if (auxImporter.loadScheme(fileName))
                {
                    mvarParent.mvarLayout = auxImporter.getSystem();
                    if (null == mvarParent.mvarLayout)
                        mcolResults.Add(new errorResult(2, string.Format("Error parsing file {0}.", fileName), -1));
                    else
                    {
                        mvarParent.mvarLayout.fileName = fileName;
                        mvarResultString = string.Format("{0} parsed sucessfully", mvarParent.mvarLayout.name);
                    }
                }
                else
                    mcolResults.Add(new errorResult(3, "Incorrect file or wrong filename", -1));
            }
        }
        internal override string sucessString => mvarResultString;
        internal override string HelpString => "Load an XML file project in memory. This operation is mandatory to process further data.";
        internal override string SyntaxString => string.Format("{0} fileName", Tokens[0]);
    }
}
