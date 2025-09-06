using MontefaroMatias;
using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias.Models.Elements;
using MontefaroMatias.Models.Orders;
using MontefaroMatias.Users;
using System.Net.Http.Json;
using System.Text.Json;

namespace TopacioCTC.Components
{
    //Este objeto es el responsable de pedir al servidor la información de representación
    public class TopacioClient:HttpClientBase
    {        
        public TopacioClient(HttpClient httpClient):base (httpClient,"layout") {}
        //Devuelve la indicación de las señales,
        //el estado de los pasos a nivel y
        //el estado y configuración de los circuitos de vía.
        public async Task<LayoutModel?> getLayoutCurrentStatus()
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
        // Lista de vistas parciales del enclavamiento.
        public async Task<Views?> getViewsList()
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

        // Configuración actual del kernel, para consulta en un cliente.
        public async Task<string> getConfiguration()
        {
            string request = composeCommand("conf");
            HttpResponseMessage respuesta = await sendGetRequest(request);
            HttpContent contenido = respuesta.Content;
            string salida = await contenido.ReadAsStringAsync();
            return salida;
        }

        // Recupera el usuario que está intentando abrir sesión
        public async Task<User?> Login(User? rhs)
        {
            string request = composeCommand("login");
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

        // Intenta ejecutar un comando de texto en el servidor
        public async Task<string> processTextOrder(string order)
        {
            HttpResponseMessage respuesta = await sendPostRequest("cmd", order);
            string salida = await respuesta.Content.ReadAsStringAsync();
            return salida;
        }
        // Intenta ejecutar un comando OrderModel en el servidor
        public async Task<int> processLayoutOrder(OrdersModel orders)
        {
            string mensaje = JsonSerializer.Serialize(orders);
            HttpResponseMessage respuesta = await sendPostRequest("lcmd", mensaje);
            string salidaTexto = await respuesta.Content.ReadAsStringAsync();
            if (int.TryParse(salidaTexto, out int salida))
                return salida;
            return -1; //Error inesperado.
        }

        public async Task<int> occupancy(string circuitId)
        {
            OrderModel orden = new OrderModel();
            orden.id = "OCC";
            orden.or = circuitId;
            orden.ds = circuitId;
            OrdersModel ordenes = new OrdersModel();
            ordenes.ords.Add(orden);
            return await processLayoutOrder(ordenes);
        }
    }
}
