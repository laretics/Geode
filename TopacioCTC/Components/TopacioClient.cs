using MontefaroMatias.LayoutView;
using System.Net.Http.Json;

namespace TopacioCTC.Components
{
    //Este objeto es el responsable de pedir al servidor la información de representación
    public class TopacioClient:HttpClientBase
    {
        public TopacioClient(HttpClient httpClient):base (httpClient,"layout") 
        {}


        public async Task<Topology?> getTopology()
        {
            string request = composeCommand("topo");
            try
            {
                HttpResponseMessage respuesta = await sendGetRequest(request);
                HttpContent contenido = respuesta.Content;
                Topology auxTopo = new Topology();
                PortableTopology? portaTopo = await contenido.ReadFromJsonAsync<PortableTopology?>();
                if (null != portaTopo)
                    auxTopo.portableElement = portaTopo;
                return auxTopo;
            }
            catch (Exception e) { return null; }
        }
    }
}
