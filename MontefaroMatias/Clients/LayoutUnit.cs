using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    /// <summary>
    /// Circuito de vía (parte activa)
    /// Contiene un conjunto de marmitas y combinaciones de posiciones.
    /// </summary>
    public class LayoutUnit:ClientNamedElement
    {
        #region Source Code
        internal Frog[] Frogs { get; private set; } //Colección de agujas
        internal List<Itinerary> Itineraries { get; private set; }//Itinerarios definidos en el cliente.
        internal bool Sequential { get; private set; } //Si el circuito es secuencial, las agujas se irán moviendo una tras otra.

        internal LayoutUnit()
        {
            Frogs = new Frog[0];
            Itineraries = new List<Itinerary>();
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            Sequential = boolParam("sequential");
            foreach(XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("frogs"))
                    {
                        if (!parseFrogs(hijo)) return false;
                    }                        
                    else if(hijo.Name.Equals("itins"))
                    {
                        if (!parseItineraries(hijo)) return false;
                    }                        
                }
            }
            return true;
        }
        internal bool parseFrogs(XmlNode node)
        {
            List<Frog> auxListaFrog = new List<Frog>();
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if(hijo.NodeType==XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("frog"))
                    {
                        Frog frog = new Frog();
                        if (!frog.parse(hijo)) return false;
                        auxListaFrog.Add(frog);
                    }
                }
            }
            Frogs = auxListaFrog.ToArray();
            return true;
        }
        internal bool parseItineraries(XmlNode node)
        {
            Itineraries.Clear();
            foreach(XmlNode hijo in node.ChildNodes)
            {
                if(hijo.NodeType==XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("itin"))
                    {
                        Itinerary nuevo = new Itinerary();
                        if(!nuevo.parse(hijo)) return false;
                        Itineraries.Add(nuevo);
                    }
                }
            }
            return true;
        }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            if(detailed)
            {
                string sequentialString = "";
                if (Sequential) sequentialString = " (sequential)";
                sb.AppendFormat("Layout unit {0}{1} (id {2}).\n", name, sequentialString, id);
                sb.AppendFormat("\t{0} frogs and {1} itineraries.\n", Frogs.Count(), Itineraries.Count);
                sb.Append("\tFrogs:\n");
                foreach(Frog frog in Frogs)
                {
                    sb.Append("\t\t");
                    frog.describe(sb, detailed);
                }
                sb.Append("\tItineraries\n");
                foreach(Itinerary itin in Itineraries)
                {
                    sb.Append("\t\t");
                    itin.describe(sb, detailed);
                }
            }
            else
            {
                string sequentialString = "";
                if (Sequential) sequentialString = " (sequential)";
                sb.AppendFormat("Layout unit {0}{1} (id {2}). {3} frogs and {4} itineraries", name, sequentialString, id, Frogs.Count(), Itineraries.Count);
            }
        }
        public override ClientNamedElement? search(string name)
        {
            ClientNamedElement? salida = base.search(name);
            if (null != salida) return salida;
            foreach (Itinerary itin in Itineraries)
            {
                salida = itin.search(name);
                if(null != salida) return salida;
            }
            return null;
        }
        public override void makeTree(int indent, StringBuilder sb, ClientElement? selection)
        {
            base.makeTree(indent, sb, selection);
            indent++;
            foreach(Itinerary itin in Itineraries)
                itin.makeTree(indent, sb, selection);
            indent--;
        }
        #endregion Source Code
        #region Object Code
        internal override ushort size
        {
            get
            {
                ushort salida = base.size;
                salida += 2;//Número de referencias a agujas o servos.
                                  // + Número de itinerarios. 
                foreach(Frog frog in Frogs)
                    salida += frog.size; //Listado de referencias a agujas o servos.
                foreach (Itinerary itinerario in  Itineraries)//Código de cada itinerario.
                    salida += itinerario.size;

                return salida;
            }
        }
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                byte frogsCount = (byte)Frogs.Count();
                byte combined = frogsCount;
                if (Sequential) combined |= 0x80;
                salida.add(combined, null, string.Format("Frogs Count {0}| Sequential {1}",frogsCount,Sequential));
                salida.add((byte)Itineraries.Count(), null, "Itineraries Count");
                foreach(Frog frog in Frogs)
                {
                    MemoDump auxFrog = frog.code;
                    salida.add(auxFrog);
                }                    
                foreach (Itinerary itin in Itineraries)
                    salida.add(itin.code);
                return salida;                    
            }
        }
        #endregion Object Code
    }

    /// <summary>
    /// Itinerario por un enclavamiento.
    /// Contiene la especificación de colocación de las agujas para 
    /// un itinerario concreto.
    /// </summary>
    public class Itinerary:ClientNamedElement
    {
        #region Source Code
        internal List<FrogCommand> commands { get; private set; }
        internal Itinerary()
        {
            commands = new List<FrogCommand>();
        }
        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) return false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.Name.Equals("frog"))
                    {
                        FrogCommand nuevo = new FrogCommand();
                        if (!nuevo.parse(child)) return false;
                        commands.Add(nuevo);
                    }
                }
            }
            return true;
        }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            if (detailed)
            {
                sb.AppendFormat("Itinerary {0} ({1}, {2} commands):\n", id, name,commands.Count);
                foreach (FrogCommand command in commands)
                {
                    sb.Append("\t");
                    command.describe(sb);
                }
            }
            else
            {
                sb.AppendFormat("Itinerary {0} \"{1}\" (Has {2} commands)\n", id, name, commands.Count);
            }
        }
        #endregion Source Code
        #region Object Code
        internal override ushort size => (byte)(commands.Count+1); //la suma de las longitudes de los comandos (1) + un byte con el número total de comandos del itinerario.

        public override MemoDump code
        {
            get
            {
                MemoDump salida = new MemoDump();
                salida.add((byte)commands.Count,name,"Commands count");
                foreach(FrogCommand command in commands)
                    salida.add(command.code);
                return salida;
            }
        }
        #endregion Object Code

        internal class FrogCommand:ClientElement
        {
            #region Source Code
            internal byte frogIndex { get; private set; }
            internal Frog.frogPositionType position { get; private set; }
            public override bool parse(XmlNode node)
            {
                if(!base.parse(node)) return false;
                frogIndex = byteParam("id");
                if (boolParam("rect"))
                    position = Frog.frogPositionType.Straight;
                else
                    position = Frog.frogPositionType.Curve;
                return true;
            }
            public override void describe(StringBuilder sb, bool detailed = false)
            {
                string auxPos = (position == Frog.frogPositionType.Straight) ? "Straight" : "Curve";
                sb.AppendFormat("Frog {0} to {1}\n", frogIndex, auxPos);
            }
            #endregion Source Code
            #region Object Code            
            internal override ushort size => 1; //Es la dirección de la aguja (motor o servo, con el flag "recto" en el bit 7
            public override MemoDump code
            {
                get
                {
                    MemoDump salida = new MemoDump();
                    byte positionFlag = (byte)(position == Frog.frogPositionType.Straight ? 0x80 : 0x00);
                    salida.add((byte)(positionFlag|frogIndex), null, string.Format("Frog index ({0}) | Position Flag ({1}, {2})", frogIndex, positionFlag, position));
                    return salida;
                }
            }
            #endregion Object Code
        }
    }

}
