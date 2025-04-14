using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.Locking
{
    public class setCrossingOrder:BasicAtom
    {
        public string crossingId { get;private set; }
        public bool orderClose { get; private set; }
        public setCrossingOrder()
        {
            crossingId = string.Empty;
        }

        internal override bool deserialize(XmlNode node)
        {
            string? auxCadena = parseString(node, "id");
            if (null == auxCadena) return false;
            crossingId = auxCadena;
            auxCadena = parseString(node, "set");
            if (null == auxCadena) return false;
            if (auxCadena.Length > 0)
                orderClose = auxCadena.ToUpper()[0] == 'C';
            return true;
        }

    }
}
