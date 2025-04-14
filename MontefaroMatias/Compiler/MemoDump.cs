using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MontefaroMatias.Compiler
{
    /// <summary>
    /// Objeto que contiene un código compilado con comentarios e información de depuración.
    /// </summary>
    public class MemoDump
    {
        public List<MemoDumpLine> Lines; //Listado completo.
        private byte mvarCRC;

        public MemoDump()
        {
            Lines = new List<MemoDumpLine>();
            mvarCRC = 0;
        }
        public void add(MemoDumpLine line)
        {
            mvarCRC ^= line.code;
            Lines.Add(line);
        }
        public void add(byte data, string? comment1 = null, string? comment2 = null)
        {
            MemoDumpLine nuevo = new MemoDumpLine();
            nuevo.code = data;
            nuevo.comment1 = comment1;
            nuevo.comment2 = comment2;
            add(nuevo);
        }
        public List<warningResult> Errors
        {
            get
            {
                List<warningResult> salida = new List<warningResult>();
                foreach (var line in Lines)
                {
                    if (null != line.mcolErrors)
                    {
                        foreach (var error in line.mcolErrors)
                            salida.Add(error);
                    }
                }
                return salida;
            }
        }
        /// <summary>
        /// Añade el código que se le pasa al listado actual.
        /// </summary>
        /// <param name="rhs"></param>
        public void add(MemoDump rhs)
        {
            foreach (MemoDumpLine line in rhs.Lines) add(line);
        }

        public ushort currentAddress { get => (ushort)Lines.Count; }

        public byte CRC { get => mvarCRC; }

        /// <summary>
        /// Listado en formato texto
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Machine code dump for {0} bytes.", Lines.Count);
            sb.AppendLine();
            sb.AppendLine("Address\tCode\t  (Bin) \tId\t\tComments");
            int address = 0;
            foreach (MemoDumpLine line in Lines)
                line.show(sb, address++);
            sb.AppendFormat("\t\t CRC: {0}", mvarCRC);
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Obtiene un buffer de datos tal como tienen que aparecer en Arduino
        /// </summary>
        /// <returns>El array</returns>
        public byte[] Buffer()
        {
            byte[] salida = new byte[Lines.Count + 1]; //Sumamos 1 por el CRC
            int index = 0;
            foreach (MemoDumpLine line in Lines)
                salida[index++] = line.code;
            salida[index++] = mvarCRC;
            return salida;
        }

        /// <summary>
        /// Línea de código (o dirección de memoria compilada.
        /// </summary>
        public class MemoDumpLine
        {
            internal List<warningResult>? mcolErrors; //Errores y warnings en la compilación de esta línea.
            public byte code { get; set; } //Código compilado
            public string? comment1 { get; set; } //Comentarios de está línea de código. Puede ser nulo.                       
            public string? comment2 { get; set; }

            public void show(StringBuilder sb, int address)
            {
                string cadenaBinaria = Convert.ToString(code, 2);
                cadenaBinaria = cadenaBinaria.Replace("0", "_");
                cadenaBinaria = cadenaBinaria.Replace("1", "#");
                cadenaBinaria = cadenaBinaria.PadLeft(8, '_');

                sb.AppendFormat("{0:X} {0:000}\t{1}\t{2}\t{3}\t\t{4}\n",
                    address,
                    code,
                    cadenaBinaria,
                    null == comment1 ? string.Empty : comment1,
                    null == comment2 ? string.Empty : comment2);
            }
        }

    }
}
