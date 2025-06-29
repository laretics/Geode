using Microsoft.AspNetCore.Components;
using MontefaroMatias;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.LayoutView.Elements;
using MontefaroMatias.Locking;
using MontefaroMatias.XML;

namespace TopacioCTC.Components
{
    /// <summary>
    /// En esta clase voy a almacenar la estructura de visualización del CTC, a partir del
    /// archivo XML que le voy a pasar con una ruta absoluta.
    /// En una nueva versión, el StorageService contiene la lista de drawables.
    /// </summary>
    public class StorageService
    {
        IServiceProvider mvarServiceProvider;
        private DateTime mvarPreviousUpdate = DateTime.MinValue;
        private Timer? mvarTimer;
        public Views Views { get; set; } = new Views(); //Vistas del enclavamiento posibles.
        public Topology Topology // Componente principal de topología.
        {get;set;} 

        public RenderFragment? RenderCache { get; set; } // Compilación renderizada del CTC, que se mostrará en la interfaz de usuario.
        public List<portableOrders> Orders { get; set; } = new List<portableOrders>(); // Lista de órdenes que se van a mostrar en el CTC.
        public event Func<Task>? OnUpdateReceived;

        public StorageService(IServiceProvider serviceProvider)
        {
            mvarServiceProvider = serviceProvider;
            mvarTimer = new Timer(async (e) => await OnTimerCallback(), null, 2000, 1000);
        }

        /// <summary>
        /// Esta función genera el caché de renderizado del CTC.
        /// </summary>
        public void ComposeRenderCache()
        {
            RenderCache = GetRenderCache();
        }
        private RenderFragment GetRenderCache() => 
            builder =>
        {
            MontefaroMatias.LayoutView.Elements.StaticRender engine = new MontefaroMatias.LayoutView.Elements.StaticRender(builder);
            foreach (Element el in Topology.Elements)
            {
                 el.RenderStatic(engine);
            }
        };

        private async Task OnTimerCallback()
        {

            using (var scope = mvarServiceProvider.CreateScope())
            {
                TopacioClient auxClient = scope.ServiceProvider.GetRequiredService<TopacioClient>();

                DateTime auxHora = await auxClient.getLastUpdateTime();
                if (auxHora > PreviousUpdate)
                {
                    if(null!=OnUpdateReceived)
                        await OnUpdateReceived.Invoke();
                    PreviousUpdate = auxHora;
                }
            }
        }
        public DateTime PreviousUpdate { get => mvarPreviousUpdate; set => mvarPreviousUpdate = value; }

    }
}
