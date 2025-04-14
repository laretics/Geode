using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.LayoutView;
using MontefaroMatias.LayoutView.Elements;
using System.Diagnostics.SymbolStore;

namespace MontefaroMatias
{
    /// <summary>
    /// Objeto para enviar órdenes a los elementos bajo control mediante paquetes UDP.
    /// </summary>
    public class IpSender
    {
        internal const string MULTICAST_IP = "239.255.0.1";
        internal const int ETHERNET_UDP_PORT = 1101; //Puerto para la recepción de datos del enclavamiento.

        public bool send(Topology topo)
        {
            bool salida = true;
            List<Signal> auxSignals = topo.signalsChanged();
            List<LayoutUnit> auxCircuits = topo.circuitsChanged();
            int auxPayLoad = 8+  //Cabecera
                (auxSignals.Count*2)+ //Id Señal + valor
                (auxCircuits.Count*2); //Id Circuito + valor

            byte[] paquete = new byte[auxPayLoad];
            int puntero = 0;
            paquete[puntero++] = 11; //Cabecera 0
            paquete[puntero++] = 22; //Cabecera 1
            paquete[puntero++] = 33; //Cabecera 2
            paquete[puntero++] = (byte)DateTime.Now.Minute;
            paquete[puntero++] = (byte)DateTime.Now.Second;
            paquete[puntero++] = 1; //Instrucción de actualización
            paquete[puntero++] = (byte)auxSignals.Count; //Número de señales que cambian
            paquete[puntero++] = (byte)auxCircuits.Count; //Número de circuitos que cambian
            foreach(Signal auxSignal in auxSignals)
            {
                paquete[puntero++] = (byte)auxSignal.id;
                paquete[puntero++] = (byte)auxSignal.Order;
            }
            foreach(LayoutUnit circuit in auxCircuits)
            {
                paquete[puntero++] = (byte)circuit.id;
                paquete[puntero++] = (byte)circuit.currentPosition;
            }
            //Ahora hacemos el envío
            UdpClient auxClient = new UdpClient();
            try
            {                
                IPEndPoint auxPunto = new IPEndPoint(IPAddress.Parse(MULTICAST_IP), ETHERNET_UDP_PORT);
                auxClient.Send(paquete,paquete.Length, auxPunto);
                Console.WriteLine(string.Format("Sent {0} bytes.", paquete.Length));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                salida = false;
            }
            finally
            {
                auxClient.Close();
            }
            return salida;
        }
    }
}
