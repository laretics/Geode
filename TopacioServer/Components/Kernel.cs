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
        internal LayoutSystem mainSystem { get; set; }
        internal UdpSender senderUdp { get; set; }
        internal string FileName { get;set; }
        internal bool UdpEnabled { get; set; }
        internal string? UdpPort { get; set; }
        internal string? UdpDestination { get; set; }

        public Kernel()
        {
            mainSystem = new LayoutSystem();
            this.FileName = Environment.GetEnvironmentVariable("KERNEL_FILENAME") ?? "Parque.xml";
            loadScheme();
            UdpDestination = Environment.GetEnvironmentVariable("UDP_DESTINATION");
            UdpPort = Environment.GetEnvironmentVariable("UDP_PORT");
            string? auxUdpEnabled = Environment.GetEnvironmentVariable("UDP_ENABLED");
            UdpEnabled = (null != auxUdpEnabled && auxUdpEnabled.Length > 0 && auxUdpEnabled.ToUpper()[0] == 'T');
            senderUdp = new UdpSender(UdpDestination, UdpPort);
            Topology.OnUpdateCallback = () => doSendUdp();
        }
        public void loadScheme()
        {
            XMLImporter auxImporter = new XMLImporter();            
            if (auxImporter.loadScheme(this.FileName))
            {
                LayoutSystem? auxSistema = auxImporter.getSystem();
                if (null != auxSistema)
                    mainSystem = auxSistema;

                mainSystem.Topology.Dai();
            }
        }
        private void doSendUdp()
        {
            if(UdpEnabled&&null!=senderUdp&&null!=mainSystem)
                senderUdp.send(mainSystem.Topology);
        }
    }
}
