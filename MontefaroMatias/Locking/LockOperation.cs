using MontefaroMatias.LayoutView.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.Locking
{
    /// <summary>
    /// Es una posible orden a cumplir por el enclavamiento.
    /// El enclavamiento sólo reaccionará ante órdenes ya definidas.
    /// </summary>
    public class LockOperation:BasicAtom
    {
        public string id { get;private set; } //Código de la orden
        public string name { get;private set; } //Nombre público de la orden
        public string? groupId { get;private set; }
        public string origin { get;private set; }
        public string destination { get;private set; }
        public bool shunting { get; private set; } //Indica si la orden es de maniobra o no.

        public List<string> colKeys { get; private set; } //Lista de elementos donde se hace click para activar la orden.
        public List<string> colPreviousFree { get; private set; } //Lista de circuitos a comprobar que están libres.
        public List<string> colLockCircuits { get; private set; } //Lista de circuitos que se deben marcar como enclavados.
        public List<changeCircuitOrder> layoutOrders { get; private set; }
        public List<setSignalOrder> signalOrders { get; private set; }
        public List<setCrossingOrder> crossingOrders { get; private set; }

        public LockOperation()
        {
            layoutOrders = new List<changeCircuitOrder>();
            signalOrders = new List<setSignalOrder>();
            crossingOrders = new List<setCrossingOrder>();
            colKeys = new List<string>();
            colPreviousFree = new List<string>();
            colLockCircuits = new List<string>();

            id = string.Empty;
            name = string.Empty;
            origin = string.Empty;
            destination = string.Empty;
        }
        public bool matchesKeys(List<string> keys)
        {
            foreach (string key in keys)
            {
                if(!colKeys.Contains(key))
                    return false;
            }
            return true;
        }

        internal bool parseOrders(XmlNode node)
        {
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if(hijo.NodeType == XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("pos"))
                    {
                        changeCircuitOrder nuevoCambioCircuito = new changeCircuitOrder();
                        if (!nuevoCambioCircuito.deserialize(hijo)) return false;
                        layoutOrders.Add(nuevoCambioCircuito);
                    }
                    else if (hijo.Name.Equals("signal"))
                    {
                        setSignalOrder nuevaOrdenSenal = new setSignalOrder();
                        if(!nuevaOrdenSenal.deserialize(hijo)) return false;
                        signalOrders.Add(nuevaOrdenSenal);
                    }
                    else if (hijo.Name.Equals("crossing"))
                    {
                        setCrossingOrder nuevoCrossing = new setCrossingOrder();
                        if(!nuevoCrossing.deserialize(hijo)) return false;
                        crossingOrders.Add(nuevoCrossing);
                    }
                }
            }
            return true;
        }

        internal override bool deserialize(XmlNode node)
        {
            string? auxCadena = parseString(node, "id");
            if(null==auxCadena) return false;
            id= auxCadena;
            auxCadena = parseString(node, "name");
            if (null == auxCadena) return false;
            name = auxCadena;
            groupId = parseString(node, "group");
            auxCadena = parseString(node, "origin");
            if (null == auxCadena) return false;
            origin = auxCadena;
            auxCadena = parseString(node, "destination");
            if (null == auxCadena) return false;
            destination = auxCadena;
            auxCadena = parseString(node, "shunting");
            shunting = (null != auxCadena&&auxCadena.ToUpper().StartsWith("TRUE"));
            auxCadena = parseString(node, "key");
            if (null == auxCadena) return false;            
            colKeys = auxCadena.Split(",").ToList();
            foreach (XmlNode child in node.ChildNodes) 
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    switch(child.Name)
                    {
                        case "pre":
                            auxCadena = parseString(child, "ckfree");
                            if (null == auxCadena) return false;
                            colPreviousFree = auxCadena.Split(',').ToList();
                            break;
                        case "chg":
                            if(!parseOrders(child)) return false;
                            break;
                        case "post":
                            auxCadena = parseString(child, "setlock");
                            if (null == auxCadena) return false;
                            colLockCircuits = auxCadena.Split(",").ToList();
                            break;
                    }
                }
            }
            return true;
        }
    }



}
