using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{   
    /// <summary>
    /// Elemento de tipo Servo a disposición de una señal o un enclavamiento
    /// Hereda de "frogInterface" porque es un dispositivo primario de los enclavamientos.
    /// </summary>
    public class Servo:frogInterface
    {
        #region Source Code
        internal byte port { get; set; } //Número del puerto (se supone que es una salida analógica Ax)
        internal bool tomeu {  get; set; } //Este servo anula la luz de una señal mientras se está moviendo.
        internal bool dynamic { get; set; } //Indica si el dispositivo debe mantener la energía cuando esté en reposo
        internal byte speed { get; set; } //Valor entre 0 y 3 que define la velocidad de desplazamiento de un semáforo.
        internal string tag { get; set; } //Anotación de la utilización de este servo.
        private byte[] mcolAngles; //Array con los dos ángulos extremos
        #endregion Source Code
        #region Object Code
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                salida.add(port, "Servo", this.tag);
                byte valorSalida;
                valorSalida = (byte)(mcolAngles[0] / 2);
                salida.add(valorSalida, null, "Angle(0)/2 amount");
                valorSalida = (byte)(mcolAngles[1] / 2);
                salida.add(valorSalida, null, "Angle(1)/2 amount");
                byte flags = 0; //Opciones adicionales del servo o controlador
                StringBuilder sb = new StringBuilder();
                if (tomeu) { flags |= (0x01); sb.Append("tomeu "); }
                if (dynamic){ flags |= (0x02); sb.Append("dynamic "); }
                if (speed % 2 != 0) { flags |= (0x04); };
                if (speed>1) { flags |= (0x08); };
                sb.AppendFormat("Speed {0}", speed);
                salida.add(flags, "Flags", sb.ToString());
                return salida;
            }
        }
        internal override ushort size => 4;
        #endregion Object Code
        internal Servo():base()
        {
            mcolAngles = new byte[2];
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) 
                return false;
            string auxPort = stringParam("port").ToUpper();
            if (auxPort.Contains("A"))
            {
                byte numeroPuerto = 0;
                string auxCadena = auxPort.Replace("A", "");
                if (byte.TryParse(auxCadena, out numeroPuerto))
                {
                    port = numeroPuerto;
                    //Intentamos leer un servo de tipo enclavamiento
                    int auxInt = 0;                   
                    if (null != node.Attributes["rectAngle"])
                    {
                        //Es una marmita
                        auxInt = intParam("rectAngle");
                        mcolAngles[0] = (byte)(auxInt / 2);  //Desvío a directa
                        auxInt = intParam("curveAngle");
                        mcolAngles[1] = (byte)(auxInt / 2); ; //Desvío a desviada
                    }
                    else if (null != node.Attributes["stopAngle"])
                    {
                        //Es un semáforo
                        auxInt = intParam("stopAngle");
                        mcolAngles[0] = (byte)(auxInt / 2);
                        auxInt = intParam("allowAngle");
                        mcolAngles[1] = (byte)(auxInt / 2);
                        tomeu = boolParam("tomeu");
                        dynamic = boolParam("dynamic");
                        speed = byteParam("speed");
                        if (speed > 3) speed = 3;
                    }
                    return (mcolAngles[0]<181 && mcolAngles[1]< 181); //Valores correctos de servo.
                }                                  
            }
            else
            {
                pp("Servo port must begin with 'A'");
            }
                return false;
        }
        internal byte angle(bool rhs) { return mcolAngles[rhs ? 1 : 0]; }
        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Servo -> port A{0} angle:{1}-{2}\n", port, mcolAngles[0], mcolAngles[1]);
        }
    }
}
