using MontefaroMatias;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.LayoutView.Elements;
using MontefaroMatias.XML;

namespace TopacioCTC.Components
{
    /// <summary>
    /// En esta clase voy a almacenar la estructura de visualización del CTC, a partir del
    /// archivo XML que le voy a pasar con una ruta absoluta.
    /// </summary>
    public class Storage
    {
        internal Topology topology { get; set; }
        internal Storage() 
        { 
            XMLImporter importer = new XMLImporter();
            topology = new Topology();
            if(importer.loadSchemeFromResource("Parque"))
            {
                LayoutSystem? auxSistema = importer.getSystem();
                if(null!=auxSistema)
                    topology = auxSistema.Topology;                
            }

        }
        internal void Dai()
        {
            topology.Dai();
        }

        internal void ChangeStatus(string circuitList, Common.layoutTraceStatus status)
        {
            topology.ChangeStatus(circuitList, status);
        }

    }
}
