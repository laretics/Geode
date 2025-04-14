using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using static BootLoader.Compiler.CmdParser.CommandParser;
using MontefaroMatias.Clients;
using MontefaroMatias.Compiler;

namespace BootLoader.Compiler.CmdParser
{
    /// <summary>
    /// Sube el último código compilado a la placa de Arduino
    /// </summary>
    internal class Upload:SelectedElementCommand
    {
        internal const int ETHERNET_MAIN_PORT = 1100; //Este es el puerto que usaré para casi todo.        
        internal Upload(CommandParser parser) : base(parser) { }
        internal override string[] Tokens => new string[] { "upload", "up" };
        internal override void invokeSecure(string[] arguments)
        {            
            if (null == mvarParent.mvarCompiledCode)
            {
                results.Add(new warningResult(6, "Upload request without compiled code", -1));
            }
            else
            {
                Debug.Assert(null != mvarParent.mvarSelection);
                Debug.Assert(mvarParent.mvarSelection is Client);
                Client? cliente = mvarParent.mvarSelection as Client;
                Debug.Assert(null != cliente);

                //Formato de la información a enviar por ethernet:
                //0     069     Cabecera (valor fijo)
                //1     107     Cabecera (valor fijo)
                //2     033     Cabecera (valor fijo)
                //3     001     Instrucción Update Firmware
                //4     N       Número de datos a transmitir (byte bajo)
                //5     N       Número de datos a transmitir (byte alto)
                //6     x       Primer dato
                
                byte[] auxCode = cliente.code.Buffer(); //Código a subir
                string auxIp = "192.168.0.254"; //Ip por defecto de los módulos nada más salir de fábrica.
                if(isAvailable(cliente.ipAddress.ToString()))
                {
                    auxIp = cliente.ipAddress.ToString();
                    Console.WriteLine(string.Format("Detected client {0}", auxIp));
                }
                if(!isAvailable(auxIp))
                {
                    results.Add(new errorResult(16, string.Format("Device with ip {0} is not present",auxIp), -1));
                    return;
                }
                //Aquí es donde subo el código al arduino
                Console.WriteLine(string.Format("Uploading firmware to client {0}", auxIp));
                try
                {
                    Console.WriteLine("Opening connection");
                    TcpClient auxCliente = new TcpClient(auxIp, ETHERNET_MAIN_PORT);
                    NetworkStream auxCorriente = auxCliente.GetStream();
                    Console.WriteLine("Writing header");
                    auxCorriente.WriteByte(069); //Cabecera
                    auxCorriente.WriteByte(107); //Cabecera
                    auxCorriente.WriteByte(033); //Cabecera
                    auxCorriente.WriteByte(1); //Instrucción de upload firmware.
                    byte[] auxNum = BitConverter.GetBytes(auxCode.Length);
                    if (auxNum.Length > 1)
                    {
                        auxCorriente.WriteByte(auxNum[0]);
                        auxCorriente.WriteByte(auxNum[1]);
                    }
                    else
                    {
                        auxCorriente.WriteByte(auxNum[0]);
                        auxCorriente.WriteByte(0);
                    }
                    Console.WriteLine("Writing code");
                    auxCorriente.Write(auxCode, 0, auxCode.Length);
                    Console.WriteLine("Done. Awaiting result");

                    //Daré una pausa a Arduino para que procese los datos y confirme que todo ha llegado correctamente.
                    //0     111     Cabecera (valor fijo)
                    //1     255     Cabecera (valor fijo)
                    //2     069     Cabecera (valor fijo)
                    //3     N       Código de respuesta.
                    int auxReintentos = 10;
                    Thread.Sleep(3000);
                    while (!auxCorriente.CanRead && auxReintentos > 0)
                    {
                        Thread.Sleep(200);
                        Console.WriteLine(string.Format("No response. {0} tryouts.", auxReintentos--));
                    }
                    if (auxCorriente.CanRead && auxCorriente.DataAvailable)
                    {
                        byte[] auxResponse = new byte[16];
                        int numDatos = auxCorriente.Read(auxResponse);
                        if (numDatos < 4)
                        {
                            results.Add(new errorResult(8, "Incomplete response from remote device", -1));
                        }
                        else
                        {
                            bool auxCabeceraCorrecta = true;
                            if (auxResponse[0] != 111) auxCabeceraCorrecta = false;
                            if (auxResponse[1] != 255) auxCabeceraCorrecta = false;
                            if (auxResponse[2] != 069) auxCabeceraCorrecta = false;
                            if (!auxCabeceraCorrecta)
                            {
                                results.Add(new errorResult(9, "Corrupt response header from remote device", -1));
                            }
                            else
                            {
                                switch (auxResponse[3])
                                {
                                    case 0:
                                        results.Add(new sucessResult()); break;
                                    case 1:
                                        results.Add(new errorResult(10, "CRC error from remote device trying to update its firmware", -1));
                                        break;
                                    case 2:
                                        results.Add(new errorResult(11, "Wrong firmware version in remote device trying to update its firmware", -1));
                                        break;
                                    case 4:
                                        results.Add(new errorResult(12, "Wrong header read in remote device trying to update its firmware", -1));
                                        break;
                                    case 5:
                                        results.Add(new errorResult(13, "Upload data amount illegal", -1));
                                        break;
                                    default:
                                        results.Add(new errorResult(14, "Unknown response from remote device trying to update its firmware", -1));
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        results.Add(new errorResult(7, "Ehernet timeout requesting write confirmation on remote device", -1));
                    }
                }
                     catch(Exception ex)
                {
                    results.Add(new errorResult(15, string.Format("Unknown internal error: {0} ({1})",ex.StackTrace,ex.Message), -1));
                }
            }
        }
        internal override string sucessString => "Upload ended successfuly. Please, restart controller to execute new version.";
        internal override string HelpString => "Upload last successful code to physical device controller.\nIf not specified, port and baudRate are extracted from XML source code.";
        internal override string SyntaxString => string.Format("{0} [comPort[,baudRate]] (Ej: {0} COM3,9600)", Tokens[0]);


        /// <summary>
        /// Función que resuelve (mediante "ping") si una determinada dirección está disponible.
        /// </summary>
        /// <param name="ipString">Dirección en formato texto</param>
        /// <returns>True si el dispositivo con esa dirección está contestando</returns>
        internal bool isAvailable(string ipString)
        {
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(ipString);
            return reply.Status==IPStatus.Success;
        }
    }
}
