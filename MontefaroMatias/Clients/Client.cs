using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    //Modelización de un cliente de enclavamiento
    public class Client:ClientNamedElement
    {
        /// <summary>
        /// Cambiar este valor con el cambio de versión
        /// </summary>
        internal const byte CODE_VERSION = 15; //Versión que aparece en el display al arrancar.
        internal const byte CODE_STARTER = 69;

        #region Código Fuente        
        protected IPAddress mvarAddress; //Dirección del cliente
        protected byte mvarMask; //Número de bytes de la máscara de la dirección IP del cliente.
        protected byte[] mcolMAC; //Dirección MAC del cliente        

        protected List<Signal> mcolSignal; //Señales de tráfico
        protected List<Circuit> mcolCircuit; //Circuitos de vía (detectores)
        protected List<LayoutUnit> mcolLayoutUnits; //Circuitos de vía (agujas)
        protected List<ClientNamedElement> mcolClients; //Conjunto general de elementos de este cliente.

        internal enum InterfaceType:byte //Dispositivo para ajustes locales
        {
            uep = 0, //Terminal de tipo UEP (IO Shield)
            none =1, //No hay ningún interface
            lcd=2, //Pantalla táctil            
            other=255
        }
        internal InterfaceType userInterface { get; private set; } //Tipo de interface del usuario.
        #endregion Código Fuente
        #region Código objeto
        protected List<Servo> mcolServos; //Lista con todos los servos de un mismo cliente.
        protected List<l298n> mcolMotors; //Lista con todos los motores de agujas de un mismo cliente.
        protected UInt16 mvarSize=0; //Tamaño del cliente compilado (vale cero cuando no hemos precompilado).
        protected UInt16 motor0Address, structureBeginAddress;
        protected UInt16 mvarPrecompileIndex;
        protected MemoDump mcolCompilation;
        protected byte mvarCRC; //Comprobación cíclica redundante.
        internal void resetCompile() //Reinicia el motor de compilación
        {
            mvarSize = 0;
            mvarPrecompileIndex = 0;
            mvarCRC = 0;
        }
        internal void precompile() //Construcción de las tablas de referencias de servos y motores.
        {
            if (0!=mvarSize) return;
            mcolServos = new List<Servo>();
            mcolMotors = new List<l298n>();
            //Primera etapa... recolectamos servos y motores en las colecciones.
            precompileFirstStage();
            //Segunda etapa... asignamos direcciones a todos los servos y motores de la estructura.
            mvarPrecompileIndex = 0;
            precompileSecondStage();
            //Tercera etapa... resolvemos las referencias de los servos y motores.
            precompileThirdStage();
            mvarPrecompileIndex++; //Valor de CRC
            mvarSize = mvarPrecompileIndex;
        }

        //Primera etapa... recolectamos servos y motores en las colecciones.
        private void precompileFirstStage()
        {            
            foreach (Signal senal in mcolSignal)
            {
                if (null != senal.servo)
                    mcolServos.Add(senal.servo);
            }
            foreach (LayoutUnit unit in mcolLayoutUnits)
            {
                foreach (Frog frog in unit.Frogs)
                {
                    if (null != frog.device)
                    {
                        if (frog.device is Servo)
                            mcolServos.Add((Servo)frog.device);
                        else if (frog.device is l298n)
                            mcolMotors.Add((l298n)frog.device);
                    }
                }
            }
        }        
        private void compileFirstStage()
        {
            //En la precompilación sólo habíamos construido las tablas de motores y servos.
            //Sólo podemos crear la estructura e iniciar el índice
            mcolCompilation = new MemoDump();
        }
        //Segunda etapa... asignamos direcciones a todos los servos y motores de la estructura.
        private void precompileSecondStage()
        {
            //El código compilado comienza con dos bytes de detección de firmware y versión
            //address 00 -> Constante: 69
            //address 01 -> Versión: 0 a 255
            //address 02 -> Interface del cliente.
            for (int i = 0; i < 3; i++) mvarPrecompileIndex++;

            //A continuación los campos de dirección Ethernet
            //address 03 a 08 -> Dirección MAC
            for (int i = 0; i < 6; i++) mvarPrecompileIndex++;
            //address 09 a 12 -> Dirección IP
            for (int i = 0; i < 4; i++) mvarPrecompileIndex++;
            //address 13 -> Bits de máscara
            mvarPrecompileIndex++;

            //address 14 -> Puntero de comienzo de los componentes. (Siempre menor de 255)            
            mvarPrecompileIndex++;
            //address 15 -> Comienzo de la tabla de motores. (Desde la dirección 15 y hasta este valor serán servos)
            mvarPrecompileIndex++;
            foreach (Servo servo in mcolServos)
            {
                servo.address = mvarPrecompileIndex;
                mvarPrecompileIndex += servo.size;
            }
            motor0Address = mvarPrecompileIndex; //Esta será la dirección del primer motor de agujas del cliente.
            foreach (l298n motor in mcolMotors)
            {
                motor.address = mvarPrecompileIndex;
                mvarPrecompileIndex += motor.size;
            }
            structureBeginAddress = mvarPrecompileIndex; //Aquí comenzará el código de la estructura.
        }
        //Segunda etapa: Construímos la lista de servos y de motores.
        private void compileSecondStage()
        {
            //Address 00 -> Campo constante de comienzo de código
            mcolCompilation.add(CODE_STARTER, "Code header");
            //Address 01 -> Campo de versión de código (para que Arduino sepa cómo gestionar el contenido)
            mcolCompilation.add(CODE_VERSION, "Code version");
            //Address 02 -> Interface del cliente (un byte).
            mcolCompilation.add((byte)userInterface, "User Interface");

            //Address 03 -> Dirección MAC
            for (int i = 0;i<6;i++)
            {
                mcolCompilation.add(mcolMAC[i], string.Format("MAC[{0}]", i), mcolMAC[i].ToString("X2"));
            }

            //Address 09 -> Dirección IP local
            byte[] auxBytes = mvarAddress.GetAddressBytes();
            for(int i = 0;i<4;i++)
            {
                mcolCompilation.add(auxBytes[i], string.Format("IP[{0}]",1), auxBytes[i].ToString());
            }

            //Address 13 -> Bits de máscara
            mcolCompilation.add(mvarMask, "Address range");

            //address 14 -> Puntero de comienzo de los componentes. (Siempre menor de 255)
            mcolCompilation.add((byte)structureBeginAddress, "Structure begin address");
            //address 15 -> Comienzo de la tabla de motores. (Desde la dirección 02 y hasta este valor serán servos)
            mcolCompilation.add((byte)motor0Address, "First Motor address");
            foreach(Servo servo in mcolServos)
            {
                Debug.Assert(servo.address == mcolCompilation.currentAddress);
                MemoDump dump = servo.code;
                mcolCompilation.add(dump);
            }
            Debug.Assert(motor0Address == mcolCompilation.currentAddress);
            foreach(l298n motor in mcolMotors)
            {
                Debug.Assert(motor.address == mcolCompilation.currentAddress);
                MemoDump dump = motor.code;
                mcolCompilation.add(dump);
            }
        }
        
        //Tercera etapa... resolvemos las referencias de los servos y motores.
        private void precompileThirdStage()
        {
            mvarPrecompileIndex++;//Número de señales
            mvarPrecompileIndex++;//Número de enclavamientos
            mvarPrecompileIndex++;//Número de circuitos de vía
            foreach (Signal senal in mcolSignal)
            {
                senal.address = mvarPrecompileIndex;
                mvarPrecompileIndex += senal.size;
                //Asignamos los servos a las direcciones del banco
                //senal.servoAddress = 0xFF; //Por defecto 255.
                //if (null != senal.servo)
                //{
                //    foreach (Servo xservo in mcolServos)
                //    {
                //        senal.servoAddress = (byte)xservo.address; //Asignamos la referencia al servo del semáforo.
                //    }
                //}
            }
            foreach (LayoutUnit unit in mcolLayoutUnits)
            {
                unit.address = mvarPrecompileIndex;
                mvarPrecompileIndex += unit.size;
                //Asignamos servos y motores a las direcciones del banco
                foreach (Frog auxFrog in unit.Frogs)
                {
                    if (auxFrog.device is Servo)
                    {
                        foreach (Servo xServo in mcolServos)
                        {
                            if (xServo == auxFrog.device)
                                auxFrog.deviceAddress = (byte)xServo.address;
                        }
                    }
                    else if (auxFrog.device is l298n)
                    {
                        foreach (l298n xMotor in mcolMotors)
                        {
                            if (xMotor == auxFrog.device)
                                auxFrog.deviceAddress = (byte)xMotor.address;
                        }
                    }
                }
            }
            foreach (Circuit circuit in mcolCircuit)
            {
                circuit.address = mvarPrecompileIndex;
                mvarPrecompileIndex += circuit.size;
            }                
        }

        private bool compileThirdStage()
        {
            Debug.Assert(structureBeginAddress == mcolCompilation.currentAddress);
            mcolCompilation.add((byte)mcolSignal.Count,"Signal count"); //Número de señales
            mcolCompilation.add((byte)mcolLayoutUnits.Count,"Layout units count");//Número de enclavamientos
            mcolCompilation.add((byte)mcolCircuit.Count,"Layout circuits count");//Número de circuitos detectores
            foreach (Signal senal in mcolSignal)
            {
                Debug.Assert(senal.address == mcolCompilation.currentAddress);
                MemoDump dump = senal.code;
                mcolCompilation.add(dump);
            }
            foreach(LayoutUnit unit in mcolLayoutUnits)
            {
                Debug.Assert(unit.address == mcolCompilation.currentAddress);
                MemoDump dump = unit.code;
                mcolCompilation.add(dump);
            }
            foreach(Circuit circuit in mcolCircuit)
            {
                Debug.Assert(circuit.address == mcolCompilation.currentAddress);
                MemoDump dump = circuit.code;
                mcolCompilation.add(dump);
            }
            return true;
        }
        
        
        internal override ushort size
        {
            get
            {
                if (0 == mvarSize) precompile();
                return mvarSize;
            }
        }
        public override MemoDump code
        {
            get
            {
                resetCompile();
                precompile();
                compileFirstStage();
                compileSecondStage();
                compileThirdStage();
                return mcolCompilation;
            }
        }

        public IPAddress ipAddress { get => mvarAddress; }

        //TODO: Tengo que comprobar que la compilación funciona.


        #endregion Código objeto
        public Client()
        {
            mcolClients = new List<ClientNamedElement>();
            mcolServos = new List<Servo> { new Servo() };
            mcolMotors = new List<l298n> { new l298n() };
            mcolCompilation = new MemoDump();
            mcolSignal = new List<Signal>();
            mcolCircuit = new List<Circuit>();
            mcolLayoutUnits = new List<LayoutUnit>();
            mvarAddress = new IPAddress(new byte[]{ 192,168,0,1});
            mcolMAC = new byte[] { 0, 0, 0, 0, 0, 0 };
        }
        public override ClientNamedElement? search(string name)
        {
            ClientNamedElement? client = base.search(name);
            if(client != null) return client;
            foreach (Signal sign in mcolSignal)
            {
                client = sign.search(name);
                if(client != null) return client;
            }
            foreach(Circuit circuit in mcolCircuit)
            {
                client = circuit.search(name);
                if (client != null) return client;
            }
            foreach (LayoutUnit unit in mcolLayoutUnits)
            {
                client = unit.search(name);
                if(client != null) return client;
            }
            return null;                            
        }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            
            if(detailed)
            {
                sb.AppendFormat("Client id {0}, {1}:\n", id, name);
                sb.AppendFormat("It has {0} signals, {1} detectors and {2} layout sections.\n",mcolSignal.Count,mcolCircuit.Count,mcolLayoutUnits.Count);
                string interfaceName = string.Empty;
                switch(userInterface)
                {
                    case InterfaceType.none:
                        interfaceName = "has no direct interface.";break;
                    case InterfaceType.lcd:
                        interfaceName = "is provided with lcd shield console.";break;
                    case InterfaceType.uep:
                        interfaceName = "has display and keyboard interface.";break;
                    case InterfaceType.other:
                        interfaceName = "has an unknown onboard user interface.";break;
                }
                sb.AppendFormat("Device {0} {1}\n",name,interfaceName);
                if (mcolSignal.Count > 0)
                {
                    sb.AppendLine("Signals:");
                    foreach(Signal signal in mcolSignal) 
                        signal.describe(sb,detailed);
                }
                if(mcolCircuit.Count > 0)
                {
                    sb.AppendLine("Detectors:");
                    foreach (Circuit circuit in mcolCircuit)
                        circuit.describe(sb,detailed);
                }
                if(mcolLayoutUnits.Count>0)
                {
                    sb.AppendLine("Layout sections:");
                    foreach(LayoutUnit unit in mcolLayoutUnits)
                        unit.describe(sb,detailed);
                }
            }
            else
                sb.AppendFormat("Client id {0}, {1}", id, name);
        }
        private bool parseComm(XmlNode node)
        {
            string auxComPort = stringParam("com");
            //mvarPort = new SerialPort(string.Format("COM{0}", auxComPort),intParam("baud"));
            return true;
        }       
        private bool parseEthernet(XmlNode node)
        {

            return false;
        }
        private byte[] parseMac(string rhs)
        {
            byte[] salida = new byte[] { 0, 0, 0, 0, 0, 0 };
            if(rhs.Contains(':'))
            {
                string[] entrada = rhs.Split(':');
                if(entrada.Length>5)
                {
                    salida = new byte[entrada.Length];
                    for(int i = 0; i < entrada.Length; i++)
                    {
                        if (entrada[i].Length==2)
                        {
                            salida[i] = byte.Parse(entrada[i],System.Globalization.NumberStyles.HexNumber);
                        }
                        else
                        {
                            //Aquí tengo que señalizar algún tipo de error.
                        }
                    }
                }                
            }
            return salida;
        }
        private string macString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for(int i = 0;i<mcolMAC.Length;i++)
            {
                sb.Append(mcolMAC[i].ToString("X2"));
                if(i<5) sb.Append(":");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            if(!parseComm(node)) return false;
            userInterface = (InterfaceType)byteParam("interface");            
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if(hijo.NodeType == XmlNodeType.Element)
                {
                    switch(hijo.Name)
                    {
                        case "ethernet": //Datos de conexión Ethernet
                            mvarNode = hijo;
                            mcolMAC = parseMac(stringParam("mac"));
                            IPAddress? auxAddress = new IPAddress(new byte[] { 192, 168, 0, 1 });
                            IPAddress.TryParse(stringParam("ip"), out auxAddress);
                            if (null != auxAddress) mvarAddress = auxAddress;
                            mvarMask = byteParam("mask");
                            break;
                        case "signal":
                            Signal nuevaSignal = new Signal();
                            if (!nuevaSignal.parse(hijo))
                                return false; //Ha habido un problema para parsear
                            mcolSignal.Add(nuevaSignal);
                            mcolClients.Add(nuevaSignal);
                            break;
                        case "layout":
                            LayoutUnit nuevoCir = new LayoutUnit();
                            if (!nuevoCir.parse(hijo)) return false;
                            mcolLayoutUnits.Add(nuevoCir);
                            mcolClients.Add(nuevoCir);
                            break;
                        case "circuit":
                            Circuit nuevoCircuito = new Circuit();
                            if (!nuevoCircuito.parse(hijo)) return false;
                            mcolCircuit.Add(nuevoCircuito);
                            mcolClients.Add(nuevoCircuito);
                            break;
                    }
                }
            }
            return true;
        }
        public override void makeTree(int indent, StringBuilder sb, ClientElement? selection)
        {
            base.makeTree(indent, sb, selection);
            sb.AppendFormat("Address: {0} Mask: {1} MAC: {2}\n", mvarAddress.ToString(), mvarMask, macString());
            indent++;            
            foreach(Signal sina in mcolSignal)
                sina.makeTree(indent, sb, selection);
            foreach(LayoutUnit la in mcolLayoutUnits)
                la.makeTree(indent, sb, selection);
            foreach(Circuit circuit in mcolCircuit)
                circuit.makeTree(indent, sb, selection);
            indent--;
        }
    }
}
