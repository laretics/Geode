using MontefaroMatias.LayoutView;
using MontefaroMatias.LayoutView.Elements;
using System.Net;
using System.Net.Sockets;
namespace TopacioServer.Components
{
    public class UdpSender
    {
        private IPEndPoint mvarDestinationPoint;
        public UdpSender(string? destinationIp, string? port)
        {
            IPAddress? auxAddress;
            int auxPort = 5000;
            if(!IPAddress.TryParse(destinationIp, out auxAddress))
                auxAddress = IPAddress.Broadcast;
            int.TryParse(port, out auxPort);
            mvarDestinationPoint = new IPEndPoint(auxAddress, auxPort);
        }
        public bool send(Topology topo)
        {
            bool salida = true;
            List<Signal> auxSignals = topo.signalsChanged();
            List<LayoutUnit> auxCircuits = topo.circuitsChanged();
            int auxPayLoad = 8 +  //Cabecera
                (auxSignals.Count * 2) + //Id Señal + valor
                (auxCircuits.Count * 2); //Id Circuito + valor
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
            foreach (Signal auxSignal in auxSignals)
            {
                paquete[puntero++] = (byte)auxSignal.id;
                paquete[puntero++] = (byte)auxSignal.Order;
            }
            foreach (LayoutUnit circuit in auxCircuits)
            {
                paquete[puntero++] = (byte)circuit.id;
                paquete[puntero++] = (byte)circuit.currentPosition;
            }
            //Ahora hacemos el envío
            UdpClient auxClient = new UdpClient();
            try
            {                
                auxClient.Send(paquete, paquete.Length, mvarDestinationPoint);
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
