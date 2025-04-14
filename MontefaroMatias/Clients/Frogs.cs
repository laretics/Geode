using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    /// <summary>
    /// Elementos de tipo marmita.
    /// Las marmitas (de momento) pueden ser de motor, con o sin detección de final de carrera o de servo.
    /// Voy a crear un objeto genérico que pueda gestionar las marmitas de cualquier tipo.
    /// </summary>
    public class Frog:ClientElement
    {
        
        #region Source Code
        internal byte id { get;private set; } //Número de marmita (no confundir con el número de un dispositivo)
        internal frogDetector? detector { get; private set; }
        internal frogInterface? device { get; private set; }
        internal virtual UInt16 timeout { get; set; } //Tiempo para terminar el recorrido de la aguja
        internal enum frogPositionType
        {
            Indeterminated = 0, //Posición sin comprobar
            Straight = 1,       //A directa
            Curve = 2           //A desviada
        }                     
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            if(detailed)
            {
                sb.AppendFormat("Frog {0}:\n", id);
                if(null!=device)
                {
                    sb.Append("\t");
                    device.describe(sb);
                    if (null != detector)
                    {
                        sb.Append("\t");
                        detector.describe(sb);
                    }
                }
            }
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            id = byteParam("id");
            timeout = uintParam("timeout");
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("l298n"))
                    {
                        device = new l298n();
                        device.parse(hijo);
                    }
                    else if (hijo.Name.Equals("servo"))
                    {
                        device = new Servo();
                        device.parse(hijo);
                    }
                    else if (hijo.Name.Equals("overcurrent"))
                    {
                        detector = new overcurrentFrogDetector();
                        detector.parse(hijo);
                    }
                    else if (hijo.Name.Equals("detector"))
                    {
                        detector = new positionFrogDetector();
                        detector.parse(hijo);
                    }
                }
            }
            return (null!=device);
        }
        #endregion Source Code
        #region Object Code
        internal byte deviceAddress { get; set; } //Dirección en memoria del dispositivo (motor o servo)
        internal override ushort size => (ushort)(base.size+5); //DeviceAddress + Timeout (2 bytes)+ detector directa + detector desviada
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                byte detectorDirecta = 0xff;
                byte detectorDesviada = 0xff;

                if(null!=detector)
                {
                    if (detector is overcurrentFrogDetector)
                    {
                        overcurrentFrogDetector popo = (overcurrentFrogDetector)detector;
                        detectorDirecta = popo.port;
                        detectorDesviada = popo.port;
                    }
                    else
                    {
                         if(detector is positionFrogDetector)
                        {
                            positionFrogDetector popo= (positionFrogDetector)detector;
                            detectorDirecta = popo.rectPort;
                            detectorDesviada = popo.curvePort;
                        }
                    }
                }

                salida.add(deviceAddress, "Frog", "Device address");
                salida.add((byte)(timeout%0x100), null,string.Format("Timeout L ({0} Milliseconds)",timeout));
                salida.add((byte)(timeout/0x100), null, "Timeout H");
                salida.add(detectorDirecta, null, "straight detector port");
                salida.add(detectorDesviada, null, "curve detector port");
                return salida;
            }
        }


        #endregion Object Code
    }

    /// <summary>
    /// Componente que detecta la disposición de las agujas
    /// </summary>
    public abstract class frogDetector : ClientElement { }

    /// <summary>
    /// Detector de fin de carrera de las agujas por sobreconsumo del motor o equivalente
    /// </summary>
    public class overcurrentFrogDetector:frogDetector
    {
        internal byte port { get;private set; }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Overcurrent detector port:{0}\n", port);
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            port = byteParam("port");
            return true;
        }
    }

    /// <summary>
    /// Detector de fin de carrera de dos posiciones
    /// </summary>
    public class positionFrogDetector : frogDetector
    {
        internal byte rectPort { get; private set; }
        internal byte curvePort { get; private set; }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Position detector; rect port:{0} curve port:{1}\n", rectPort, curvePort);
        }
        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) return false;
            rectPort = byteParam("rect");
            curvePort = byteParam("curve");
            return true;
        }
    }

    /// <summary>
    /// Elemento capaz de actuar sobre las agujas
    /// </summary>
    public abstract class frogInterface:ClientElement
    {
        internal override ushort size => 3; 
        //Si es un servo:
        //Puerto/compensed, angulo 0, angulo 1 
        //Si es un l298n
        //enable/compensed, direcciónA, direcciónB

        //Compensed es un boolean que indica si hay que mantener el dispositivo en tensión
        //siempre o si se puede desconectar cuando no reciba una orden.
        //Sirve para ahorrar energía en caso de que no sea necesario mantener el esfuerzo
        //del dispositivo mecánico en reposo.
    }

    /// <summary>
    /// Controlador de motores para aguja
    /// </summary>
    public class l298n:frogInterface
    {
        internal byte enablePort { get;private set; }
        internal byte movAPort { get;private set; }
        internal byte movBPort { get;private set; }

        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("l298N enable:{0} a:{1} b:{2}\n", enablePort, movAPort, movBPort);
        }
        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) return false;
            enablePort = byteParam("enable");
            movAPort = byteParam("a");
            movBPort = byteParam("b");
            
            return true;
        }

        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                salida.add(enablePort, "Motor","Enable port");
                salida.add(movAPort,null, "movA Port");
                salida.add(movBPort,null, "movB Port");
                return salida;
            }
        }
    }


}
