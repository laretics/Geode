using MontefaroMatias.LayoutView.Elements;
using MontefaroMatias.Locking;
using MontefaroMatias.LayoutView.Elements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.Json.Serialization;
using MontefaroMatias.Models.Elements;

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
            //mcolElements = new List<Element>();
            mcolPlatforms = new List<Platform>();
            mcolSignals = new Dictionary<string, Signal>();
            mcolCircuits = new Dictionary<string, Unit>();
            mcolCrossings = new Dictionary<string, Crossing>();
            mcolOperations = new List<LockOperation>();
            mcolSelectedElements = new List<string>();
            mcolActiveOperations = new List<LockOperation>();
        }
        public static Action? OnUpdateCallback { get; set; }
        internal Dictionary<string,Signal> mcolSignals;
        internal Dictionary<string, Unit> mcolCircuits;
        internal Dictionary<string, Crossing> mcolCrossings;
        internal List<Platform> mcolPlatforms;
        internal List<LockOperation> mcolOperations; //Lista de todas las operaciones posibles en el enclavamiento
        internal List<LockOperation> mcolActiveOperations; //Lista con todas las operaciones que ahora mismo están activas.
        public DateTime lastUpdate { get; set; }
        private void doUpdate()
        {
            lastUpdate = DateTime.Now;
            OnUpdateCallback?.Invoke();
        }
        public List<Element> Elements         
        { 
            get
            {
                List<Element> salida = new List<Element>();
                foreach (Platform platform in mcolPlatforms) {salida.Add(platform);}
                foreach(Crossing crossing in mcolCrossings.Values) {salida.Add(crossing);}
                foreach(Unit circuit in mcolCircuits.Values) {salida.Add(circuit);}
                foreach(Signal signal in mcolSignals.Values) {salida.Add(signal);}
                return salida;
            }            
        }
        public Dictionary<string,DynamicElement> DynamicElements
        {
            get
            {
                Dictionary<string,DynamicElement> salida = new Dictionary<string,DynamicElement>();
                foreach (Unit circuit in mcolCircuits.Values)salida.Add(circuit.name,circuit);
                foreach (Signal signal in mcolSignals.Values)salida.Add(signal.name,signal);
                return salida;
            }
        }
        public Dictionary<string,Signal> Signals { get => mcolSignals; set => mcolSignals = value; }
        public Dictionary<string,Unit> Circuits { get => mcolCircuits; set => mcolCircuits = value; }
        private List<string> mcolSelectedElements; //Lista de los elementos que están ahora mismo seleccionados
        
        public LayoutModel UDPSnapshot() //Obtiene un fotograma con la posición actual de todo el enclavamiento
        {
            LayoutModel salida = new LayoutModel();
            foreach(Unit circuito in Circuits.Values)
            {
                LayoutUnitModel auxUnit = new LayoutUnitModel();
                auxUnit.id = circuito.Id;
                auxUnit.nme = circuito.name;
                auxUnit.con = circuito.CurrentPosition;
                auxUnit.stt = (byte)circuito.CurrentStatus;
                salida.units.Add(auxUnit);
            }

            return salida;
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
                            mcolSignals.Add(nuevaSenal.name, nuevaSenal);
                            break;
                        case "section":
                            Unit nuevoLock = new Unit();
                            if(!nuevoLock.parse(child)) return false;
                            mcolCircuits.Add(nuevoLock.name, nuevoLock);
                            break;
                        case "platform":
                            Platform nuevoAnden = new Platform();
                            if(!nuevoAnden.parse(child)) return false;
                            mcolPlatforms.Add(nuevoAnden);
                            break;
                        case "crossing":
                            Crossing nuevoPN = new Crossing();
                            if(!nuevoPN.parse(child)) return false;
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
        
        /// <summary>
        /// Asigna una orden a una señal del enclavamiento.
        /// Este método es la única forma de asignar también las indicaciones de las avanzadas
        /// si es que tiene, porque desde signal no hay visibilidad al resto.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="order"></param>
        public void setSignalOrder(Signal signal, Common.orderType order)
        {
            signal.Order = order;
            if(null!=signal.advance)
            {
                if (order == Common.orderType.toParada || order == Common.orderType.toRebaseAutorizado)
                {
                    //Ahora cambiamos las avanzadas de esta señal
                    foreach (Signal signala in mcolSignals.Values)
                    {
                        if (signala.name.Equals(signal.advance))
                            setSignalOrder(signala, Common.orderType.toAvisoDeParada);
                    }
                }
            }
            doUpdate();
        }
        public void setSignalOrder(string signalId, Common.orderType order)
        {
            if (mcolSignals.ContainsKey(signalId))
            {
                setSignalOrder(mcolSignals[signalId], order);
            }
        }

        /// <summary>
        /// Provoca una ocupación deliberada en un circuito.
        /// Un segundo toque provoca una liberación deliberada en el mismo circuito.
        /// </summary>
        /// <param name="circuitId"></param>
        /// <returns></returns>       
        public bool ExecuteOccupancy(string circuitId)
        {
            if(mcolCircuits.ContainsKey(circuitId))
            {
                Unit auxCircuit = mcolCircuits[circuitId];
                if (auxCircuit.CurrentStatus == Common.layoutTraceStatus.ltOccupied)
                    auxCircuit.CurrentStatus = Common.layoutTraceStatus.ltFree;
                else
                    OccupyCircuit(auxCircuit);
                doUpdate();
            }
            return false;
        }
        /// <summary>
        /// Ocupa el circuito de la referencia.
        /// Además modifica las señales afectadas en caso de que exista alguna.
        /// </summary>
        /// <param name="rhs"></param>
        public void OccupyCircuit(Unit rhs)
        {
            rhs.CurrentStatus=Common.layoutTraceStatus.ltOccupied;
            //Ahora buscamos señales que tengan este circuito como su protegido
            foreach (Signal senal in  mcolSignals.Values)
            {
                if(null!=senal.circuit)
                {
                    if (senal.circuit.Equals(rhs.name))
                        setSignalOrder(senal, Common.orderType.toParada);
                }                
            }
        }
        public bool ExecuteOperation(string operationCMD)
        {
            if (operationCMD.Equals("DAI"))
            {
                Dai();
                return true;
            }                
            else
            {
                foreach (LockOperation operation in mcolOperations)
                {
                    if (operation.id.Equals(operationCMD))
                        return ExecuteOperation(operation);
                }
            }
            return false;
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
                        Unit unidad = mcolCircuits[order.circuitId];
                        unidad.CurrentPosition = order.position;
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
                        Unit unidad = mcolCircuits[iden];
                        unidad.CurrentStatus = Common.layoutTraceStatus.ltLocked;
                    }
                }
                //Por último accionamos las señales
                foreach(setSignalOrder order in rhs.signalOrders)
                {
                    if(mcolSignals.ContainsKey(order.signalId))
                    {
                        Signal senal = mcolSignals[order.signalId];
                        setSignalOrder(senal, order.order);
                    }
                }
                if(!mcolActiveOperations.Contains(rhs))
                    mcolActiveOperations.Add(rhs);
                doUpdate();
            }
            return false;
        }
        private bool areFree(List<string> circuits)
        {
            foreach (string circ in circuits)
            {
                if(mcolCircuits.ContainsKey(circ))
                {
                    Unit unidad = mcolCircuits[circ];
                    if (unidad.CurrentStatus != Common.layoutTraceStatus.ltFree) return false;
                }
            }
            return true;
        }
        public void Dai()
        {
            foreach(Signal senal in mcolSignals.Values)
            {
                if (senal.name.ToUpper()[0].Equals('A'))
                    setSignalOrder(senal, Common.orderType.toAvisoDeParada);
                else
                    setSignalOrder(senal, Common.orderType.toParada);
            }
            foreach(Unit cir in mcolCircuits.Values)
            {
                cir.CurrentStatus = Common.layoutTraceStatus.ltFree;
            }
            foreach(Crossing pn in mcolCrossings.Values)
            {
                pn.status = Common.crossingStatus.csClosed;
            }
            mcolActiveOperations.Clear();
            doUpdate();
        }
        public void UndoOperation(LockOperation rhs)
        {
            //Deshace la operación... libera circuitos enclavados
            foreach (string iden in rhs.colLockCircuits)
            {
                if (mcolCircuits.ContainsKey(iden))
                {
                    Unit unidad = mcolCircuits[iden];
                    unidad.CurrentStatus = Common.layoutTraceStatus.ltFree;
                }
            }
            //Pone las señales en parada
            foreach (setSignalOrder order in rhs.signalOrders)
            {
                if (mcolSignals.ContainsKey(order.signalId))
                {
                    Signal senal = mcolSignals[order.signalId];
                    if (order.signalId[0] == 'A')
                        setSignalOrder(senal, Common.orderType.toAvisoDeParada);
                    else
                        setSignalOrder(senal, Common.orderType.toParada);
                }
            }
            //elimina la orden de la lista de órdenes activas.
            if (mcolActiveOperations.Contains(rhs))
                mcolActiveOperations.Remove(rhs);
            doUpdate();
        }
        public void ChangeStatus(string circuitList, Common.layoutTraceStatus status)
        {
            string[] auxList = circuitList.Split(',');
            foreach (string aux in auxList)
            {
                if (mcolCircuits.ContainsKey(aux))
                    mcolCircuits[aux].CurrentStatus = status;
            }
        }
        public void ChangePosition(string circuitId, byte newPosition)
        {
            if(mcolCircuits.ContainsKey(circuitId))
            {
                mcolCircuits[circuitId].CurrentPosition = newPosition;
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
            foreach (Signal signal in mcolSignals.Values)
                signal.HasChanged = false;
            foreach (Unit unit in mcolCircuits.Values)
                unit.HasChanged = false;
            foreach(Crossing cross in mcolCrossings.Values)
                cross.HasChanged = false;
        }
        public void ClearSelection()
        {
            foreach (Signal signal in mcolSignals.Values)
                signal.selected = false;
            foreach (Unit unit in mcolCircuits.Values)
                unit.selected = false;
            foreach (Crossing cross in mcolCrossings.Values)
                cross.selected = false;
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
        public List<Unit> circuitsChanged()
        {
            List<Unit> salida = new List<Unit>();
            foreach (Unit circuit in mcolCircuits.Values)
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

}
