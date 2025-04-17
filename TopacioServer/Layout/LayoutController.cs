using System.Text.Json.Serialization;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.XML;

namespace TopacioServer.Layout
{
    [JsonSerializable(typeof(Topology))]
    internal partial class TopologySerializerContext : JsonSerializerContext { }
    public class LayoutController
    {
        internal Topology myTopology { get; set; }

        public LayoutController(WebApplication app)
        {
            XMLImporter auxImporter = new XMLImporter();
            myTopology = new Topology();
            if (auxImporter.loadScheme("Parque"))
            {
                LayoutSystem? auxSistema = auxImporter.getSystem();
                if (null != auxSistema)
                {
                    myTopology = auxSistema.Topology;
                }
            }

            RouteGroupBuilder? salida = app.MapGroup("layout/topo");
            salida.MapGet("/", () => getTopology());
        }

        private PortableTopology getTopology()
        {
            PortableTopology salida = myTopology.portableElement;
            return salida;
        }
    }
}
