using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias
{
    public abstract class BasicSerializableElement
    {
        public abstract bool parse(XmlNode node);
        protected string? parseString(XmlNode node, string attributeName)
        {
            string? salida = null;
            salida = node.Attributes?[attributeName]?.Value ?? null;
            return salida;
        }
        protected bool parseBoolean(XmlNode node, string attributeName)
        {
            bool salida = false;
            string? entrada = parseString(node, attributeName);
            if (null != entrada)
            {
                return
                    
                    entrada.ToUpper().Contains("T") ||
                    entrada.ToUpper().Contains("1")
                    ;
            }
            return salida;
        }
        protected int parseInt(XmlNode node, string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            int salida = -1;
            if (!string.IsNullOrEmpty(entrada))
                int.TryParse(entrada, out salida);
            return salida;
        }
        protected long parseLong(XmlNode node, string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            long salida = -1;
            if (!string.IsNullOrEmpty(entrada))
                long.TryParse(entrada, out salida);
            return salida;
        }
    }
}
