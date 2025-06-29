using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MontefaroMatias.Clients;
using MontefaroMatias.Compiler;

namespace BootLoader.Compiler.CmdParser
{
    internal class CommandParser
    {
        internal bool mvarActive;
        internal LayoutSystem? mvarLayout; //Objeto deserializado. (Código objeto)
        internal ClientNamedElement? mvarSelection; //Elemento seleccionado
        internal MemoDump? mvarCompiledCode; //Código compilado a subir

        internal List<Command> mcolCommandTokens; //Lista de palabras clave que entiende el parser.

        internal CommandParser()
        {
            mcolCommandTokens = new List<Command>();
            mvarActive = true;
            mvarLayout = null;
            mvarCompiledCode = null;
            auxLoadTokens(); //Inicio la lista de tokens.
        }
        internal bool Active { get => mvarActive; }
        internal Command? findCommand(string rhs)
        {
            foreach (Command token in mcolCommandTokens)
                if (token.match(rhs)) return token;

            return null;
        }
        private string processToken(Command token, params string[] arguments)
        {
            token.invoke(arguments);
            List<commandResult> results = token.results;
            int errores = 0;
            int warnings = 0;
            foreach (commandResult result in results)
            {
                if (result is errorResult)
                    errores++;
                else if (result is warningResult)
                    warnings++;
            }
            if (0 == errores + warnings)
                return token.sucessString;
            else
            {
                //Primero los warnings
                StringBuilder respuesta = new StringBuilder();
                if (warnings > 0 && errores > 0)
                    respuesta.AppendFormat("There are {0} warnings and {1} errors:\n", warnings, errores);
                else if (warnings > 0)
                    respuesta.AppendFormat("There are {0} warnings:\n", warnings);
                else
                    respuesta.AppendFormat("There are {0} errors:\n", errores);
                //Primero los warnings
                foreach (commandResult result in results)
                {
                    if (result is warningResult && !(result is errorResult))
                        respuesta.AppendLine(result.ToString());
                }
                //Y después los errores                
                foreach (commandResult result in results)
                {
                    if (result is errorResult)
                        respuesta.AppendLine(result.ToString());
                }
                return respuesta.ToString();
            }
        }

        internal string parse(string? rhs)
        {
            if (string.IsNullOrEmpty(rhs)) return "Syntax error: Bad command";
            string entrada = rhs;
            string[] cadenas = entrada.Split(' ');
            string comando = cadenas[0].ToUpper();
            string[] argumentos = new string[0];
            if (cadenas.Length > 1)
            {
                argumentos = new string[cadenas.Length - 1];
                for (int i = 1; i < cadenas.Length; i++)
                    argumentos[i - 1] = cadenas[i];
            }

            Command? auxToken = findCommand(comando);
            if (null == auxToken)
                return string.Format("Syntax Error: Can't parse {0}.", rhs);
            else
                return processToken(auxToken, argumentos);
        }

        /// <summary>
        /// Indicador de prompt en el parser.
        /// </summary>
        internal string Prompt { get => contextInfo(); }
        /// <summary>
        /// Muestra los elementos cargados ahora mismo en la memoria.
        /// </summary>
        /// <returns>Cadena identificativa de los elementos cargados</returns>
        internal string contextInfo()
        {
            StringBuilder sb = new StringBuilder();
            if (null != mvarLayout)
            {
                sb.AppendFormat("{0}", mvarLayout.fileName);
                if (null != mvarSelection)
                {
                    sb.AppendFormat("|{0}", mvarSelection.name);
                    if (null != mvarCompiledCode)
                    {
                        sb.AppendFormat("|{0} Bytes", mvarCompiledCode.Lines.Count);
                    }
                }
            }
            sb.Append("> ");
            return sb.ToString();
        }

        private void auxLoadTokens()
        {
            mcolCommandTokens.Add(new ClearScreenCommand(this));
            mcolCommandTokens.Add(new Help(this));
            mcolCommandTokens.Add(new Quit(this));
            mcolCommandTokens.Add(new Load(this));
            mcolCommandTokens.Add(new Selection(this));
            mcolCommandTokens.Add(new Tree(this));
            mcolCommandTokens.Add(new Describe(this));
            mcolCommandTokens.Add(new Compile(this));
            mcolCommandTokens.Add(new Upload(this));
            mcolCommandTokens.Add(new Operate(this));
            mcolCommandTokens.Add(new SVGCompile(this));
        }
    }
}
