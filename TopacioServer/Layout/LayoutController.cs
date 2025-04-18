using System.Text.Json;
using System.Text.Json.Serialization;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias.XML;
using TopacioServer.Components;

namespace TopacioServer.Layout
{
    [JsonSerializable(typeof(Topology))]
    internal partial class TopologySerializerContext : JsonSerializerContext { }
    [JsonSerializable(typeof(portableOrders))]
    internal partial class OrdersSerializerContext : JsonSerializerContext { }
    [JsonSerializable(typeof(Views))]
    internal partial class ViewsSerializerContext : JsonSerializerContext { }

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

            string cadenaJson = JsonSerializer.Serialize(vistas, ViewsSerializerContext.Default.Views);
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
    }
}
