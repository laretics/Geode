using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using MontefaroMatias.Compiler;


namespace BootLoader.Compiler
{
    /// <summary>
    /// Elemento que se comunica con Arduino y sube el firmware
    /// </summary>
    internal class ArduinoLayoutUpdater
    {
        protected SerialPort auxPort; //Puerto serie de programación
        protected MemoDump mvarCode; //Código a subir.
        internal List<commandResult> mcolErrors; //Lista de errores generados para mostrar en el proceso.
        internal ArduinoLayoutUpdater(SerialPort port, MemoDump code)
        {
            this.auxPort = port;
            this.mvarCode = code;
            mcolErrors = new List<commandResult>();
        }
         
        internal bool upload()
        {
            mcolErrors.Clear();
            auxPort.ReadTimeout = 2000;
            auxPort.WriteTimeout = 2000;
            auxPort.Open();
            if (sendHeader()) //Enviando cabecera.
            {
                //Ahora enviamos la longitud del código
                int tamano = mvarCode.Lines.Count + 2;
                byte[] auxBuffer = new byte[tamano];
                int auxIndex = 0;
                auxBuffer[auxIndex++] = (byte)(mvarCode.Lines.Count);
                foreach (MemoDump.MemoDumpLine line in mvarCode.Lines)
                    auxBuffer[auxIndex++] = line.code;
                auxBuffer[auxIndex++] = mvarCode.CRC;
                auxPort.Write(auxBuffer, 0, auxIndex);
                Thread.Sleep(100);
                auxPort.Close();
                return true;
            }
            auxPort.Close();
            return false;             
        }

        private bool sendHeader()
        {
            //Envía al arduino una cabecera que lo ponga en modo de actualización
            byte[] auxHeader = new byte[] { 120, 240, 0, 3 };
            try
            {
                auxPort.Write(auxHeader, 0, 4);
            }
            catch (Exception e) 
            {
                mcolErrors.Add(new errorResult(100,
                    string.Format("Error sending header to device: {0}", e.Message), -1));
            }
            Thread.Sleep(100);
            byte[] buffer = new byte[4];
            try
            {
                auxPort.Read(buffer, 0, 4);
            }
            catch ( System.TimeoutException ex)
            {
                mcolErrors.Add(new errorResult(101, 
                    string.Format("Timeout trying to read confirmation sending header to device. ({0})",ex.Message), -1));
                return false;
            }            
            byte[] auxComparer = new byte[] { 30, 16, 254, 3 };
            for (int i = 0; i < buffer.Length; i++)
            {
                if (auxComparer[i] != buffer[i])
                {
                    mcolErrors.Add(new errorResult(102, "Bad response from device while sending header.", -1));
                    return false;
                }
            }
            return true;
        }


    }
}
