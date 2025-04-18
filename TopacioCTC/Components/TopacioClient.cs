using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using System.Net.Http.Json;

namespace TopacioCTC.Components
{
    //Este objeto es el responsable de pedir al servidor la información de representación
    public class TopacioClient:HttpClientBase
    {        
        public TopacioClient(HttpClient httpClient):base (httpClient,"layout") {}

        public async Task<PortableTopology?> getPortableTopology()
        {
            string request = composeCommand("topo");
            try
            {
                HttpResponseMessage respuesta = await sendGetRequest(request);
                HttpContent contenido = respuesta.Content;
                PortableTopology? salida = await contenido.ReadFromJsonAsync<PortableTopology?>();
                return salida;
            }
            catch (Exception e) { return null; }
        }
        public async Task<portableOrders?> getOrders()
        {
            string request = composeCommand("ordr");
            try
            {
                HttpResponseMessage respuesta = await sendGetRequest(request);
                HttpContent contenido = respuesta.Content;
                portableOrders? salida = await contenido.ReadFromJsonAsync<portableOrders?>();
                return salida;
            }
            catch(Exception e) { return null; }
        }

        public async Task<DateTime> getLastUpdateTime()
        {
            string request = composeCommand("upd");
            HttpResponseMessage respuesta = await sendGetRequest(request);
            HttpContent contenido = respuesta.Content;
            DateTime auxHora = await contenido.ReadFromJsonAsync<DateTime>();
            return auxHora;
        }

        public async Task processOrder(string order)
        {
            HttpResponseMessage respuesta = await sendPostRequest("cmd", order);
        }
        public async Task occupancy(string circuitId)
        {
            HttpResponseMessage respuesta = await sendPostRequest("occ", circuitId);
        }
    }
}
