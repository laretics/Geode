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
    /// </summary>
    public class StorageService
    {
        IServiceProvider mvarServiceProvider;
        private DateTime mvarPreviousUpdate = DateTime.MinValue;
        private Timer? mvarTimer;
        public event Func<Task>? OnUpdateReceived;

        public StorageService(IServiceProvider serviceProvider)
        {
            mvarServiceProvider = serviceProvider;
            mvarTimer = new Timer(async (e) => await OnTimerCallback(), null, 2000, 1000);
        }

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
