using MontefaroMatias.LayoutView.Elements;
using MontefaroMatias.Locking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.Json.Serialization;

namespace MontefaroMatias.LayoutView
{
    /// <summary>
    /// Este elemento contiene todos los objetos representables de la vista del enclavamiento
    /// Las vistas son subconjuntos de Topology.
    /// Topology es singleton. Sólo puede haber una por proyecto.
    /// </summary>
    public class Topology
    {
        public Topology() 
        { 
            mcolElements = new List<Element>();
            mcolSignals = new Dictionary<string, Signal>();
            mcolCircuits = new Dictionary<string, LayoutUnit>();
            mcolCrossings = new Dictionary<string, Crossing>();
            mcolOperations = new List<LockOperation>();
            mcolSelectedElements = new List<string>();
            mcolActiveOperations = new List<LockOperation>();
        }
        public List<Element> mcolElements; //Lista con todas las geometrías.        
        internal Dictionary<string,Signal> mcolSignals;
        internal Dictionary<string, LayoutUnit> mcolCircuits;
        internal Dictionary<string, Crossing> mcolCrossings;
        internal List<LockOperation> mcolOperations; //Lista de todas las operaciones posibles en el enclavamiento
        internal List<LockOperation> mcolActiveOperations; //Lista con todas las operaciones que ahora mismo están activas.
        public List<Element> Elements { get => mcolElements; }
        public Dictionary<string,Signal> Signals { get => mcolSignals; set => mcolSignals = value; }
        public Dictionary<string,LayoutUnit> Circuits { get => mcolCircuits; set => mcolCircuits = value; }
        private List<string> mcolSelectedElements; //Lista de los elementos que están ahora mismo seleccionados
        public PortableTopology portableElement
        {
            get
            {
                PortableTopology salida = new PortableTopology();
                salida.setBase(0, 0, "Topology");
                foreach (Element element in mcolElements)
                {
                    PortableElement nuevo = element.portableElement;
                    switch(nuevo.typ)
                    {
                        case 1: //Platform
                            salida.ptf.Add((PortablePlatform)nuevo); break;
                        case 2: //Crossing
                            salida.crs.Add((PortableCrossing)nuevo); break;
                        case 3: //Signal
                            salida.sgn.Add((PortableSignal)nuevo);  break;
                        case 6: //LayoutUnit
                            salida.lyt.Add((PortableLayoutUnit)nuevo); break;                
                    }
                }
                return salida;
            }
            set
            {
                PortableTopology topo = value; 
                foreach (PortablePlatform platform in topo.ptf)
                {
                    Platform anden = new Platform();
                    anden.portableElement = platform;
                    mcolElements.Add(anden);
                }
                foreach (PortableCrossing crossing in topo.crs)
                {
                    Crossing cruce = new Crossing();
                    cruce.portableElement = crossing;
                    mcolElements.Add(cruce);
                    mcolCrossings.Add(cruce.name, cruce);
                }
                foreach (PortableSignal signal in topo.sgn)
                {
                    Signal senal = new Signal();
                    senal.portableElement = signal;
                    mcolElements.Add(senal);
                    mcolSignals.Add(senal.name, senal);
                }
                foreach (PortableLayoutUnit layoutUnit in topo.lyt)
                {
                    LayoutUnit via = new LayoutUnit();
                    via.portableElement = layoutUnit;
                    mcolElements.Add(via);
                    mcolCircuits.Add(via.name, via);
                }
            }
        }
        public bool parse(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if(XmlNodeType.Element == child.NodeType)
                {
                    switch (child.Name)
                    {
                        case "signal":
                            Signal nuevaSenal = new Signal();
                            if (!nuevaSenal.parse(child)) return false;
                            mcolElements.Add(nuevaSenal);
                            mcolSignals.Add(nuevaSenal.name, nuevaSenal);
                            break;
                        case "section":
                            LayoutUnit nuevoLock = new LayoutUnit();
                            if(!nuevoLock.parse(child)) return false;
                            mcolElements.Add(nuevoLock);
                            mcolCircuits.Add(nuevoLock.name, nuevoLock);
                            break;
                        case "platform":
                            Platform nuevoAnden = new Platform();
                            if(!nuevoAnden.parse(child)) return false;
                            mcolElements.Add(nuevoAnden);
                            break;
                        case "crossing":
                            Crossing nuevoPN = new Crossing();
                            if(!nuevoPN.parse(child)) return false;
                            mcolElements.Add(nuevoPN);
                            mcolCrossings.Add(nuevoPN.name, nuevoPN);
                            break;

                        default: //No haremos nada (de momento)
                            break;
                    }
                }
            }
            return true;
        }

        public List<String> selElements { get => mcolSelectedElements; }
        public List<LockOperation> activeOperations { get => mcolActiveOperations; }
        public bool parseOperations(XmlNode node)
        {
            foreach(XmlNode child in node.ChildNodes)
            {
                if(XmlNodeType.Element == child.NodeType)
                {
                    if(child.Name.Equals("itin"))
                    {
                        LockOperation nuevaOp = new LockOperation();
                        if(!nuevaOp.deserialize(child)) return false;
                        mcolOperations.Add(nuevaOp);
                    }
                }
            }
            return true;
        }
        
        public List<LockOperation> selOperations()
        {
            List<LockOperation> salida = new List<LockOperation>();
            foreach (LockOperation op in mcolOperations)
            {
                if(op.matchesKeys(mcolSelectedElements))
                    salida.Add(op);
            }
            return salida;
        }

        public bool ExecuteOperation(LockOperation rhs)
        {
            //Primero tenemos que asegurar que el itinerario sea compatible con los previos
            if(areFree(rhs.colPreviousFree))
            {
                //Ahora vamos a cambiar las agujas afectadas
                foreach (changeCircuitOrder order in rhs.layoutOrders)
                {
                    if(mcolCircuits.ContainsKey(order.circuitId))
                    {
                        LayoutUnit unidad = mcolCircuits[order.circuitId];
                        unidad.currentPosition = order.position;
                    }
                }
                //Cerramos los pasos a nivel afectados
                foreach (setCrossingOrder crOrder in rhs.crossingOrders)
                {
                    if(mcolCrossings.ContainsKey(crOrder.crossingId))
                    {
                        Crossing cruze = mcolCrossings[crOrder.crossingId];
                        cruze.status = crOrder.orderClose ? Common.crossingStatus.csOpen : Common.crossingStatus.csClosed;
                    }
                }
                //Reservamos los circuitos establecidos
                foreach(string iden in rhs.colLockCircuits)
                {
                    if(mcolCircuits.ContainsKey(iden))
                    {
                        LayoutUnit unidad = mcolCircuits[iden];
                        unidad.currentStatus = Common.layoutTraceStatus.ltLocked;
                    }
                }
                //Por último accionamos las señales
                foreach(setSignalOrder order in rhs.signalOrders)
                {
                    if(mcolSignals.ContainsKey(order.signalId))
                    {
                        Signal senal = mcolSignals[order.signalId];
                        senal.Order = order.order;
                    }
                }
                if(!mcolActiveOperations.Contains(rhs))
                    mcolActiveOperations.Add(rhs);
            }
            return false;
        }
        public void UndoOperation(LockOperation rhs)
        {
            //Deshace la operación... hace un DAI
            foreach(string iden in rhs.colLockCircuits)
            {
                if(mcolCircuits.ContainsKey(iden))
                {
                    LayoutUnit unidad = mcolCircuits[iden];
                    unidad.currentStatus = Common.layoutTraceStatus.ltFree;
                }
            }
            //Pone las señales en parada
            foreach (setSignalOrder order in rhs.signalOrders)
            {
                if (mcolSignals.ContainsKey(order.signalId))
                {
                    Signal senal = mcolSignals[order.signalId];
                    if (order.signalId[0] == 'A')
                        senal.Order = Common.orderType.toAvisoDeParada;
                    else
                        senal.Order = Common.orderType.toParada;
                }
            }
            //elimina la orden de la lista de órdenes activas.
            if(mcolActiveOperations.Contains(rhs))
                mcolActiveOperations.Remove(rhs);
        }

        private bool areFree(List<string> circuits)
        {
            foreach (string circ in circuits)
            {
                if(mcolCircuits.ContainsKey(circ))
                {
                    LayoutUnit unidad = mcolCircuits[circ];
                    if (unidad.currentStatus != Common.layoutTraceStatus.ltFree) return false;
                }
            }
            return true;
        }
        public void Dai()
        {
            foreach(Signal senal in mcolSignals.Values)
            {
                if (senal.name.ToUpper()[0].Equals('A'))
                    senal.Order = Common.orderType.toAvisoDeParada;
                else
                    senal.Order = Common.orderType.toParada;
            }
            foreach(LayoutUnit cir in mcolCircuits.Values)
            {
                cir.currentStatus = Common.layoutTraceStatus.ltFree;
            }
            foreach(Crossing pn in mcolCrossings.Values)
            {
                pn.status = Common.crossingStatus.csClosed;
            }
            mcolActiveOperations.Clear();
        }
        public void ChangeStatus(string circuitList, Common.layoutTraceStatus status)
        {
            string[] auxList = circuitList.Split(',');
            foreach (string aux in auxList)
            {
                if (mcolCircuits.ContainsKey(aux))
                    mcolCircuits[aux].currentStatus = status;
            }
        }
        public void ChangePosition(string circuitId, byte newPosition)
        {
            if(mcolCircuits.ContainsKey(circuitId))
            {
                mcolCircuits[circuitId].currentPosition = newPosition;
            }
        }
        public void ChangeOrder(string signalId, Common.orderType order)
        {
            if(mcolSignals.ContainsKey(signalId))
            {
                mcolSignals[signalId].Order = order;
            }
        }
        public void ChangeCrossing(string crossingId,Common.crossingStatus status)
        {
            if(mcolCrossings.ContainsKey(crossingId))
            {
                mcolCrossings[crossingId].status = status;
            }
        }        

        public void ClearChanges()
        {
             foreach (Element el  in mcolElements)
            {
                if(el.GetType()==typeof(DynamicElement))
                {
                    DynamicElement din = (DynamicElement)el;
                    din.HasChanged = false;
                }
            }
        }
    
        public List<Signal> signalsChanged()
        {
            List<Signal> salida = new List<Signal>();
            foreach (Signal auxSenal in mcolSignals.Values)
            {
                if (auxSenal.HasChanged)
                {
                    salida.Add(auxSenal);
                    auxSenal.HasChanged = false;
                }
            }
            return salida;
        }
        public List<LayoutUnit> circuitsChanged()
        {
            List<LayoutUnit> salida = new List<LayoutUnit>();
            foreach (LayoutUnit circuit in mcolCircuits.Values)
            {
                if (circuit.HasChanged)
                {
                    salida.Add(circuit);
                    circuit.HasChanged = false;
                }
            }
            return salida;
        }
    }
    public class PortableTopology:PortableElement
    {
        public PortableTopology() : base(7)
        {
            ptf = new List<PortablePlatform>();
            crs = new List<PortableCrossing>();
            sgn = new List<PortableSignal>();
            lyt = new List<PortableLayoutUnit>();
        }
        public List<PortablePlatform> ptf { get; set; }
        public List<PortableCrossing> crs { get; set; }
        public List<PortableSignal> sgn { get; set; }
        public List<PortableLayoutUnit> lyt { get; set; }
    }
}
