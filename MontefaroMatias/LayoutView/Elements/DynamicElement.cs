using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements
{
    /// <summary>
    /// Esto son los elementos que pueden cambiar de estado o de color
    /// Son también seleccionables desde una vista gráfica
    /// </summary>
    public abstract class DynamicElement:Element
    {
        public static Action<string?>? AddElementCallback { get; set; }

        //Coordenadas del rectángulo contenedor de este elemento

        public bool selected { get; set; }        

        public bool HasChanged { get; set; }
        public DynamicElement():base()
        {
        }
            
    }
}
