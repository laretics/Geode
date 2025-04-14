using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.Locking
{
    public class setSignalOrder:BasicAtom
    {
        public string signalId { get;private set; }
        public Common.orderType order { get;private set; }
        public setSignalOrder()
        {
            signalId = string.Empty;
        }
        internal override bool deserialize(XmlNode node)
        {
            string? auxCadena = parseString(node, "id");
            if (null==auxCadena) return false;
            signalId = auxCadena;
            auxCadena = parseString(node, "set");
            if (null == auxCadena) return false;
            switch(auxCadena.ToUpper())
            {
                case "FREE":
                    order = Common.orderType.toViaLibre;
                    break;
                case "STOPADVICE":
                    order = Common.orderType.toAvisoDeParada;
                    break;
                case "STOP":
                    order = Common.orderType.toParada;
                    break;
                case "WARNING":
                    order = Common.orderType.toPrecaucion;
                    break;
                case "OVERRIDE":
                    order = Common.orderType.toRebaseAutorizado;
                    break;
                default:
                    order = Common.orderType.toUnknown;
                    break;                
            }
            return true;
        }
    }
}
