using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    public class Signal:ClientNamedElement
    {
        #region Source Code
        internal bool semaphore { get => null != servo; }
        internal Servo? servo { get; private set; }
        protected List<SignalLight> mcolLights;
        internal enum SignalLightColor:byte
        {
            None = 0,
            green=1,
            red=2,
            yellow=3,
            white=4
        }
        static string stringColor(SignalLightColor rhs)
        {
            switch (rhs)
            {
                case SignalLightColor.None: return "None";
                case SignalLightColor.green: return "Green";
                case SignalLightColor.red: return "Red";
                case SignalLightColor.yellow: return "Yellow";
                case SignalLightColor.white: return "White";
                default: return rhs.ToString();
            }
        }
        internal enum OrderType:byte
        {
            free = 0,
            stop = 1,
            stopAdvice = 2,
            warning = 3,
            shunt=4,
            none =255
        }
        
        internal Signal():base()
        {
            mcolLights = new List<SignalLight>();
        }
        internal OrderType parseOrder(string order)
        {
            string auxCadena = order.Trim().ToUpper();
            if (auxCadena.Contains("STOPADVICE"))
                return OrderType.stopAdvice;
            else if (auxCadena.Contains("FREE"))
                return OrderType.free;
            else if (auxCadena.Contains("STOP"))
                return OrderType.stop;
            else if (auxCadena.Contains("WARNING"))
                return OrderType.warning;
            else if (auxCadena.Contains("SHUNT"))
                return OrderType.shunt;
            else
            {
                pp(string.Format("Unknown signal order type ({0}).",order));
                return OrderType.none;
            }
                
        }

        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("light"))
                    {
                        SignalLight nuevo = new SignalLight(); 
                        if (!nuevo.parse(hijo))
                        {
                            pp("Could not parse signal light");
                            return false;
                        }
                            
                        mcolLights.Add(nuevo);
                    }
                    else if(hijo.Name.Equals("servo"))
                    {
                        servo = new Servo();
                        if(!servo.parse(hijo))
                        {
                            pp("Could not parse servo");
                            return false;
                        }                            
                        servo.tag = string.Format("For signal {0}",name);
                    }
                }
            }
            return true;
        }

        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Signal {0} (Id {1}) {2}{3}", name, id, semaphore ? "Semaphore" : "Light", null != servo && servo.tomeu ? "[Tomeu]" : "");
            if (detailed)
            {
                sb.AppendLine();
                foreach(SignalLight ll in mcolLights)
                    ll.describe(sb, detailed);
                if(null!=servo)
                    servo.describe(sb);
            }
            else
            {
                sb.AppendFormat(" {0} lights", mcolLights.Count);
            }
            sb.AppendLine();
        }
        #endregion Source Code
        #region Object Code
        internal byte servoAddress //Posición del servo en el array de servos (o 255 si no hay)
        { 
            get
            {
                if (null == servo) return 0xff;
                return (byte)servo.address;
            }
        }
        
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                salida.add(servoAddress,null,"Servo Address");
                byte lightsCount = (byte)mcolLights.Count;
                bool auxTomeu = false;
                if (null!=servo)
                {
                    auxTomeu = servo.tomeu;
                }
                
                salida.add((byte)(lightsCount), null, string.Format("Lights count {0}",lightsCount));
                for (int i = 0; i < lightsCount; i++)
                {
                    byte flagInverted = (byte)(mcolLights[i].inverted ? 0x80 : 0x00);
                    salida.add((byte)(mcolLights[i].port|flagInverted), null, string.Format("Color {0} port | inverted {1}", mcolLights[i].color, mcolLights[i].inverted));
                }                    
                return salida;
            }
        }

        internal override ushort size => (ushort)(base.size+ 2 + mcolLights.Count);

        #endregion Object Code
        public class SignalLight:ClientElement
        {
            internal Signal.SignalLightColor color { get; set; }
            internal byte port { get; set; }
            internal bool inverted { get; set; }
            public override bool parse(XmlNode node)
            {
                if (!base.parse(node)) return false;
                color = parseColor(stringParam("color"));
                port = byteParam("port");
                inverted = boolParam("inverted");
                return color != SignalLightColor.None;
            }
            private Signal.SignalLightColor parseColor(string colorId)
            {
                string auxCadena = colorId.ToUpper();
                if (auxCadena.Contains("GREEN")) return SignalLightColor.green;
                if (auxCadena.Contains("RED")) return SignalLightColor.red;
                if (auxCadena.Contains("YELLOW")) return SignalLightColor.yellow;
                if (auxCadena.Contains("WHITE")) return SignalLightColor.white;
                return SignalLightColor.None;
            }
            
            public override void describe(StringBuilder sb, bool detailed = false)
            {
                sb.AppendFormat("{0} -> port {1} {2}\n", Signal.stringColor(color), port, inverted ? "[Inverted]" : "");
            }
        }
    }
}
