using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MontefaroMatias.Compiler;

namespace BootLoader.Compiler.CmdParser
{
    internal class Operate: LoadedStructureCommand
    {
        internal const int ETHERNET_UDP_PORT = 1101; //Puerto para la recepción de datos del enclavamiento.
        internal Operate(CommandParser parser): base(parser) { }
        internal override string[] Tokens => new string[] { "operate", "op" };
        internal override void invokeSecure(string[] arguments)
        {
            //Primero vamos a compilar la lista de órdenes.
            elementOrders signalOrders = new elementOrders();
            elementOrders circuitOrders = new elementOrders();
            elementOrders? coleccion = null;
            byte elementoId = 255;
            foreach (string s in arguments)
            {
                string mayusculo = s.ToUpper().Trim();
                if(mayusculo.StartsWith("S"))
                {
                    coleccion = signalOrders;
                }
                else if(mayusculo.StartsWith("C")||mayusculo.StartsWith("L"))
                {
                    coleccion = circuitOrders;
                }
                else
                {
                    if(null!= coleccion)
                    {
                        if(255==elementoId)
                        {
                            byte.TryParse(s, out elementoId);
                        }
                        else
                        {
                            byte valorId = 0;
                            string ma = s.ToUpper().Trim();
                            if (ma.StartsWith("V"))
                                valorId = 0;
                            else if (ma.StartsWith("A"))
                                valorId = 2;
                            else if (ma.StartsWith("PR"))
                                valorId = 3;
                            else if (ma.StartsWith("RE"))
                                valorId = 4;
                            else if (ma.StartsWith("R"))
                                valorId = 1;
                            else
                                byte.TryParse(s, out valorId);
                            
                            coleccion.orders.Add(new elementOrder(elementoId, valorId));
                            elementoId = 255;
                        }
                    }
                }
            }
            if(signalOrders.orders.Count>0 || circuitOrders.orders.Count>0)
            {
                string multicastIp = "239.255.0.1";
                UdpClient auxUdpClient = new UdpClient();
                IPEndPoint auxPuntoRemoto = new IPEndPoint(IPAddress.Parse(multicastIp), ETHERNET_UDP_PORT);
                int auxDatos = 6+((signalOrders.orders.Count+circuitOrders.orders.Count)*2);
                byte[] carga = new byte[auxDatos];
                int puntero = 0;
                carga[puntero++] = 11; //Cabecera 0
                carga[puntero++] = 22; //Cabecera 1
                carga[puntero++] = 33; //Cabecera 2
                carga[puntero++] = 1; //Instrucción de actualización
                carga[puntero++] = (byte)signalOrders.orders.Count;
                carga[puntero++] = (byte)circuitOrders.orders.Count;
                foreach(elementOrder elemento in signalOrders.orders)
                {
                    carga[puntero++] = elemento.id;
                    carga[puntero++] = elemento.order;
                }
                foreach (elementOrder elemento in circuitOrders.orders)
                {
                    carga[puntero++] = elemento.id;
                    carga[puntero++] = elemento.order;
                }
                //Ahora enviamos el array por el puerto UDP
                try
                {
                    auxUdpClient.Send(carga, carga.Length, auxPuntoRemoto);
                    Console.WriteLine(string.Format("Sent {0} bytes.", carga.Length));
                    results.Add(new sucessResult());
                }
                catch (Exception e)
                {
                    results.Add(new errorResult(20, e.Message, -1));
                }
                finally
                {
                    auxUdpClient?.Close();
                }
            }
        }
        internal override string HelpString => "Send a command to all clients related to any signal or layout";
        internal override string SyntaxString => string.Format("{0} S,id,val,id,val...,C,id,val,id,val... (Ex: {0} S,2,3,3,4,C,1,0,2,1)", Tokens[0]);


        internal class elementOrder
        {
            internal elementOrder(byte id, byte order)
            {
                this.id = id;
                this.order = order;
            }
            internal byte id { get; set; }
            internal byte order { get; set; }
        }
        internal class  elementOrders
        {
            internal List<elementOrder> orders { get; private set; } = new List<elementOrder>();                                        
        }

    }


}
