using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BootLoader.Compiler
{
    /// <summary>
    /// Compilador del código máquina de configuración del cliente en EEPROM
    /// </summary>
    internal class Compiler
    {
        internal const int MAX_EEPROM = 4096;
        internal const int MAX_OF_SERVOS = 8; //Máximo de servos que puede controlar una sola placa
        internal const int MAX_OF_CIRCUITS = 8; //Máximo de circuitos de vía en una sola placa
        internal const int MAX_OF_SIGNALS = 8; //Máximo de señales que puede controlar una sola placa

        internal enum interfaceType : byte
        {
            none = 0, //Sin interface
            uep = 1, //Shield multifunción
            tft = 2, //Pantalla táctil

            other = 255 //Interface de usuario no definida
        }
        internal enum signalColor
        {
            green = 0,
            red = 1,
            yellow = 2,
            white = 3
        }
        internal enum signalOrder
        {
            toViaLibre = 0,
            toParada = 1,
            toAvisoDeParada = 2,
            toPrecaucion = 3,
            toRebaseAutorizado = 4
        }

        internal byte[] mvarMemory = new byte[MAX_EEPROM];
        uint mvarPointer; //Puntero de la siguiente línea de código
        private byte mvarDeviceId;
        private interfaceType mvarDeviceInterface;
        private List<servoUnit> mcolServos;
        private List<signalUnit> mcolSignals;
        private signalUnit mvarCurrentSignal;

        internal signalUnit currentSignal { get => mvarCurrentSignal; }
        internal uint currentPointer { get => mvarPointer; } //Información de depuración.


        /// <summary>
        /// Inicia la clase y pone a cero la estructura.
        /// </summary>
        internal Compiler(byte deviceId, interfaceType deviceInterface)
        {
            mvarPointer = 0;
            mcolServos = new List<servoUnit>();
            mcolSignals = new List<signalUnit>();
            mvarDeviceId = deviceId;
            mvarDeviceInterface = deviceInterface;
        }

        internal byte[] outputCode()
        {
            byte[] salida = new byte[mvarPointer];
            for (int i = 0; i < mvarPointer; i++) salida[i] = mvarMemory[i];
            return salida;
        }

        internal string memoryDump()
        {
            //Obtiene un volcado de toda la estructura
            StringBuilder salida = new StringBuilder();
            salida.AppendLine("Address\t\tValue\tReference");
            logicUnit? elemento = null;
            byte[] buffer = outputCode();
            for (int i = 0; i < buffer.Length; i++)
            {
                elemento = searchReference(buffer[i]);
                salida.AppendFormat("{0}\t{0:X}\t{1}\t", i, buffer[i]);
                if (null != elemento)
                    salida.Append(elemento.ToString());
                salida.AppendLine();
            }
            return salida.ToString();
        }
        private logicUnit? searchReference(uint address)
        {
            foreach (signalUnit unit in mcolSignals)
            {
                if (unit.debugIndex == address) return unit;
                foreach (signalLight light in unit.colors)
                {
                    if (light.debugIndex == address) return light;
                }
            }
            foreach (servoUnit unit in mcolServos)
            {
                if (!unit.debugIndex.Equals(address)) return unit;
            }
            return null;
        }



        internal void addSignal(string name, byte id, signalOrder defaultOrder = signalOrder.toParada)
        {
            mvarCurrentSignal = new signalUnit();
            mvarCurrentSignal.id = id;
            mvarCurrentSignal.name = name;
            mvarCurrentSignal.defaultOrder = defaultOrder;
            mvarCurrentSignal.tomeuMode = false;
            mcolSignals.Add(mvarCurrentSignal);
        }
        internal void addSemaphore(string name, byte id, byte servoPort, byte angle0, byte angle1, byte servoSpeed, bool tomeuMode, signalOrder defaultOrder = signalOrder.toParada)
        {
            mvarCurrentSignal = new signalUnit();
            mvarCurrentSignal.id = id;
            mvarCurrentSignal.name = name;
            mvarCurrentSignal.defaultOrder = defaultOrder;
            mvarCurrentSignal.tomeuMode = tomeuMode;
            servoUnit servo = new servoUnit(servoPort, angle0, angle1, servoSpeed);
            mvarCurrentSignal.servoUnit = servo;
            mcolServos.Add(servo);
            mcolSignals.Add(mvarCurrentSignal);
        }

        /// <summary>
        /// Rellena la estructura de la EEPROM en función de los elementos que contiene este cliente.
        /// </summary>
        internal void compile()
        {
            wb(mvarDeviceId);
            wb((byte)mvarDeviceInterface);
            wb((byte)mcolServos.Count);
            wb((byte)mcolSignals.Count);
            foreach (servoUnit auxServo in mcolServos)
                auxServo.compile(this);
            foreach (signalUnit auxSenal in mcolSignals)
                auxSenal.compile(this);
        }
        internal byte servoIndex(servoUnit servo)
        {
            byte index = 0;
            foreach (servoUnit auxServo in mcolServos)
            {
                if (auxServo == servo) return index;
                index++;
            }
            return 0xff;
        }





        private void wb(byte rhs)
        {
            mvarMemory[mvarPointer++] = rhs;
        }

        internal abstract class logicUnit
        {
            internal virtual void compile(Compiler parent)
            {
                debugIndex = parent.currentPointer;
            }
            internal uint debugIndex { get; set; }
        }
        internal class servoUnit : logicUnit
        {
            internal byte port { get; set; } //Puerto del servo (Puerto analógico)
            internal byte angle0 { get; set; } //Primer ángulo
            internal byte angle1 { get; set; } //Segundo ángulo
            internal byte speed { get; set; } //Velocidad del servo (para semáforos)
            internal byte index { get; set; } //Número del servo
            internal servoUnit(byte port, byte angle0, byte angle1, byte speed)
            {
                this.port = port;
                this.angle0 = angle0;
                this.angle1 = angle1;
                this.speed = speed;
            }
            internal override void compile(Compiler parent)
            {
                base.compile(parent);
                parent.wb(port);
                parent.wb(angle0);
                parent.wb(angle1);
                parent.wb(speed);
            }
            public override string ToString()
            {
                return string.Format("Servo Unit {0} Port A{1} {2}-{3} at speed {4}", index, port, angle0, angle1, speed);
            }
        }
        internal class signalUnit : logicUnit
        {
            internal byte id { get; set; } //Dirección en el array general de señales de la estructura.
            internal string name { get; set; } //Sólo para propósitos de comprobación. No se almacena.
            internal signalOrder defaultOrder { get; set; } //Señal por defecto al encenderse el sistema.
            internal bool tomeuMode { get; set; } //Sólo en semáforos... indica si tiene modo Tomeu en las luces.
            internal List<signalLight> colors { get; set; } //Colección de colores.
            internal servoUnit? servoUnit { get; set; } //Servo (sólo para semáforos)
            internal signalUnit()
            {
                colors = new List<signalLight>();
                servoUnit = null;
                name = string.Empty;
            }
            public override string ToString()
            {
                return string.Format("Signal {0} (id {1})", name, id);
            }
            internal override void compile(Compiler parent)
            {
                base.compile(parent);
                parent.wb(id);
                parent.wb((byte)defaultOrder);
                if (null == servoUnit)//Sólo es una señal luminosa sin servo
                {
                    parent.wb(0xff); //No hay tomeuMode
                }
                else
                {
                    parent.wb(parent.servoIndex(servoUnit));
                    parent.wb((byte)(tomeuMode ? 0xff : 0x00));
                }
                colors.Sort(); //Primero ordenamos los colores.
                parent.wb((byte)colors.Count);
                byte auxInverted = 0x00;
                byte auxIndex = 0;
                foreach (signalLight color in colors)
                {
                    if (color.inverted)
                        auxInverted |= (byte)(2 ^ auxIndex++);
                    color.compile(parent);
                }
                parent.wb(auxInverted);
            }
        }
        internal class signalLight : logicUnit, IComparable<signalLight>
        {
            internal signalColor color { get; set; } //Color
            internal byte port { get; set; } //Puerto arduino de esta luz
            internal bool inverted { get; set; } //Indica si esta luz está invertida

            public int CompareTo(signalLight? other)
            {
                if (null == other) return 0;
                return color.CompareTo(other.color);
            }
            public override string ToString()
            {
                return string.Format("{0} Light. Port {1} {2}", color, port, inverted ? "(Inverted)" : "");
            }
            internal override void compile(Compiler parent)
            {
                base.compile(parent);
                parent.wb(port);
            }
        }
    }
}
