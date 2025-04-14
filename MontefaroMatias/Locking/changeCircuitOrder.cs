using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.Locking
{
    public class changeCircuitOrder:BasicAtom
    {
        public string circuitId { get; private set; }
        public byte position { get; private set; }
        internal override bool deserialize(XmlNode node)
        {
            string? auxCadena = parseString(node, "cir");
            if (null==auxCadena) return false;
            circuitId = auxCadena;
            position = parseByte(node, "set");
            return true;
        }
    }
}
