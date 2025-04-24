using System.Text.Json;
using System.Text.Json.Serialization;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias;
using MontefaroMatias.XML;
using TopacioServer.Components;
using MontefaroMatias.Users;

namespace TopacioServer.Layout
{

    public class LayoutController
    {
        private Kernel mvarKernel;
        public LayoutController(WebApplication app, Kernel kernel)
        {
            mvarKernel = kernel;
            RouteGroupBuilder? salida = app.MapGroup("layout");
            salida.MapGet("/topo", () => getTopology());
            salida.MapGet("/ordr", () => getOrders());
            salida.MapGet("/views", () => getViews());
            salida.MapGet("/upd", () => getLastUpdate());
            salida.MapPut("/user", (HttpRequest request) => processUser(request));
            salida.MapPost("/cmd", (HttpRequest request) => processOrder(request));
            salida.MapPost("/occ", (HttpRequest request) => processOccupancy(request));
        }

        private PortableTopology getTopology()
        {
            PortableTopology salida = mvarKernel.mainSystem.Topology.portableElement;
            return salida;
        }
        private portableOrders getOrders()
        {
            portableOrders salida = mvarKernel.mainSystem.Topology.portableOrders;
            return salida;
        }
        private string getViews()
        {
            Views vistas = mvarKernel.mainSystem.Views;

            string cadenaJson = JsonSerializer.Serialize(vistas,SharedSerializeContext.Default.Views);
            return cadenaJson;
        }
        //Obtiene el momento de la última actualización, para saber si el navegador
        //tiene que refrescar la imagen o no.
        private DateTime getLastUpdate()
        {
            return mvarKernel.mainSystem.Topology.lastUpdate;
        }
        private async Task processOrder(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            string orderId = await reader.ReadToEndAsync();
            mvarKernel.mainSystem.Topology.ExecuteOperation(orderId);
        }
        private async Task processOccupancy(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            string orderId = await reader.ReadToEndAsync();
            mvarKernel.mainSystem.Topology.ExecuteOccupancy(orderId);   
        }
        private async Task<string> processUser(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            string? userString = await reader.ReadToEndAsync();
            
            if (string.IsNullOrEmpty(userString)) return null;
            try
            {
                User? auxUser = JsonSerializer.Deserialize(userString, SharedSerializeContext.Default.User);
                User? auxRespuesta = mvarKernel.mainSystem.LoginRequest(auxUser);
                string salida = JsonSerializer.Serialize(auxRespuesta, SharedSerializeContext.Default.User);
                return salida;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializando el objeto User: {ex.Message}");
                return null;
            }
        }

    }
}
