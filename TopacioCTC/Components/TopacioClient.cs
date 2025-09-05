using MontefaroMatias;
using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias.Models.Elements;
using MontefaroMatias.Users;
using System.Net.Http.Json;
using System.Text.Json;

namespace TopacioCTC.Components
{
    //Este objeto es el responsable de pedir al servidor la información de representación
    public class TopacioClient:HttpClientBase
    {        
        public TopacioClient(HttpClient httpClient):base (httpClient,"layout") {}
        public async Task<LayoutModel?> getLayoutMessage()
        {
            string request = composeCommand("topo");
            try
            {
                HttpResponseMessage respuesta = await sendGetRequest(request);
                HttpContent contenido = respuesta.Content;
                LayoutModel? salida = await contenido.ReadFromJsonAsync<LayoutModel?>();
                return salida;
            }
            catch(Exception e) { return null; }
        }

        public async Task<Views?> getViews()
        {
            string request = composeCommand("views");
            try
            {
                HttpResponseMessage respuesta = await sendGetRequest(request);
                HttpContent contenido = respuesta.Content;
                Views? salida = await contenido.ReadFromJsonAsync<Views>();
                return salida;
            }
            catch (Exception e) { return null; }
        }

        public async Task<DateTime> getLastUpdateTime()
        {
            string request = composeCommand("upd");
            HttpResponseMessage respuesta = await sendGetRequest(request);
            HttpContent contenido = respuesta.Content;
            DateTime auxHora = await contenido.ReadFromJsonAsync<DateTime>();
            return auxHora;
        }

        public async Task<string> getConfiguration()
        {
            string request = composeCommand("conf");
            HttpResponseMessage respuesta = await sendGetRequest(request);
            HttpContent contenido = respuesta.Content;
            string salida = await contenido.ReadAsStringAsync();
            return salida;
        }

        public async Task<User?> tryLogin(User? rhs)
        {
            string request = composeCommand("user");
            string json = JsonSerializer.Serialize(rhs, SharedSerializeContext.Default.User);
            HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage respuesta = await mvarClient.PutAsync(request, content);
            if (!respuesta.IsSuccessStatusCode)
                return null; //Recibió respuesta equivocada.
            using (Stream respuestaStream = await respuesta.Content.ReadAsStreamAsync())
            {
                //string? contenidoComoCadena = await new StreamReader(respuestaStream).ReadToEndAsync();
                User? salida = await JsonSerializer.DeserializeAsync(respuestaStream, SharedSerializeContext.Default.User);
                return salida;
            }
        }
        public async Task LayoutRefreshRequest()
        {
            HttpResponseMessage respuesta = await sendPostRequest("rfh", "");
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
