using Microsoft.JSInterop;
using MontefaroMatias.Models.Elements;
using System.Diagnostics.Metrics;

namespace TopacioCTC.Components
{
    /// <summary>
    /// Servicio de actualización y captura de eventos del nuevo control Blazor basado en un archivo SVG
    /// manipulado a nivel del DOM.
    /// (Cresta de la ola al 100% en 2025)
    /// </summary>
    public class TopacioLayoutService
    {
        private IJSRuntime mvarJSRuntime { get; set; }
        private const byte MAX_CONFIGS = 7; //Número máximo de configuraciones distintas que puede tener un circuito.
        private const string GROUP_COLOR_SET_FN = "setGroupColor";
        private const string GROUP_VISIBLE_SET_FN = "setGroupVisibility";

        public TopacioLayoutService(IServiceProvider serviceProvider, IJSRuntime jsRuntime)
        {
            mvarJSRuntime = jsRuntime;
        }

        /// <summary>
        /// Actualiza la imagen SVG del CanvasExpress con datos recibidos del servidor.
        /// </summary>
        /// <param name="model">Información recibida por UDP</param>        
        public async Task UpdateCanvasExpress(LayoutModel? model)
        {
            if(null!=model)
            {
                foreach(LayoutUnitModel unitModel in model.units)
                {
                    await SetColor(unitModel.id, unitModel.con, unitModel.stt);
                    await SetVisible(unitModel.id, unitModel.con);
                }
            }
        }
        private async Task SetVisible(int circuitId, byte currentConfiguration)
        {
            for (byte n = 0; n < MAX_CONFIGS; n++)
            {
                await mvarJSRuntime.InvokeVoidAsync(
                    GROUP_VISIBLE_SET_FN,
                    auxSVGCircuitIndex(circuitId, n, true),
                    currentConfiguration == n);
                await mvarJSRuntime.InvokeVoidAsync(
                    GROUP_VISIBLE_SET_FN,
                    auxSVGCircuitIndex(circuitId, n, false),
                    currentConfiguration == n);
            }
        }
        private async Task SetColor(int circuitId, byte currentConfiguration, byte status)
        {
            await mvarJSRuntime.InvokeVoidAsync(
                GROUP_COLOR_SET_FN, 
                auxSVGCircuitIndex(circuitId,currentConfiguration,true), 
                ColorByStatus(status));
        }
        private string ColorByStatus(byte sta)
        {
            switch (sta)
            {
                case 0: return "gray";
                case 1: return "yellow";
                case 2: return "green";
                case 3: return "blue";
                case 4: return "red";
                default: return "magenta";
            }
        }
        private string auxSVGCircuitIndex(int circuitId, int position, bool activePart)
        {
            return string.Format("{0}_{1}_lty_{2}", activePart ? "ac" : "pa", position, circuitId);
        }
    }
}
