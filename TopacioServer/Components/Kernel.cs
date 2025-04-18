using MontefaroMatias.LayoutView;
using MontefaroMatias.Clients;
using MontefaroMatias.XML;

namespace TopacioServer.Components
{
    /// <summary>
    /// Este es el componente central del proyecto, en cuya instancia están todos los componentes.
    /// </summary>
    public class Kernel
    {
        internal Topology topology { get; set; }
        internal UdpSender senderUdp { get; set; }
        internal bool udpEnabled { get; set; }
        public Kernel()
        {
            topology = new Topology();
            XMLImporter auxImporter = new XMLImporter();
            string auxProjectFile = Environment.GetEnvironmentVariable("KERNEL_FILENAME") ?? "Parque.xml";
            if (auxImporter.loadScheme(auxProjectFile))
            {
                LayoutSystem? auxSistema = auxImporter.getSystem();
                if (null != auxSistema)
                {
                    topology = auxSistema.Topology;
                    topology.Dai();
                }
            }
            string? auxIpUDPDestination = Environment.GetEnvironmentVariable("UDP_DESTINATION");
            string? auxPortUDPDestination = Environment.GetEnvironmentVariable("UDP_PORT");
            string? auxUdpEnabled = Environment.GetEnvironmentVariable("UDP_ENABLED");
            udpEnabled = (null != auxUdpEnabled && auxUdpEnabled.Length > 0 && auxUdpEnabled.ToUpper()[0] == 'T');
            senderUdp = new UdpSender(auxIpUDPDestination, auxPortUDPDestination);
            Topology.OnUpdateCallback = () => doSendUdp();
        }
        private void doSendUdp()
        {
            if(udpEnabled&&null!=senderUdp&&null!=topology)
                senderUdp.send(topology);
        }
    }
}
