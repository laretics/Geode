using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    public class Circuit:ClientNamedElement
    {
        #region Source Code
        internal Circuit()
        {
            delay = TimeSpan.Zero;
        }
        internal byte port { get; private set; }
        internal TimeSpan delay { get; private set; }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            port = byteParam("port");
            TimeSpan auxDelay = TimeSpan.Zero;
            TimeSpan.TryParse(stringParam("delay"), out auxDelay);
            this.delay = auxDelay;
            return port<255;
            
        }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Circuit {0} (Id {1}) Port {2} Delay {3:mm:ss}", name, id, port,delay);
        }
        #endregion Source Code
        #region Object Code
        internal override ushort size => (ushort)(base.size + 2);
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                byte totalSeconds = 0;
                if (delay.TotalSeconds > 254)
                    totalSeconds = 255;
                else
                    totalSeconds = (byte)delay.TotalSeconds;

                salida.add(port,null,"Circuit status Port");
                salida.add(totalSeconds, null, "Circuit free delay");
                
                return salida;
            }
        }

        #endregion Object Code
    }
}
