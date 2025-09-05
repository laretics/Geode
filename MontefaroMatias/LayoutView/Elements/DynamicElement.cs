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
        protected long minX { get; set; }
        protected long maxX { get; set; }
        protected long minY { get; set; }
        protected long maxY { get; set; }
        protected int labelX { get; set; }
        protected int labelY { get; set; }

        public bool selected { get; set; }        

//        protected void openContainerRegion()
//        {
            //resetContainer();
            //mvarBuilder.OpenElement(0, "g");
            //mvarBuilder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, e => AddElementCallback?.Invoke(this?.name)));
//        }
        //internal void resetContainer()
        //{
        //    minX = long.MaxValue;
        //    maxX = long.MinValue;
        //    minY = long.MaxValue;
        //    maxY = long.MinValue;
        //}

        /// <summary>
        /// Infla la zona del contenedor para poder marcar bien el elemento
        /// </summary>
        /// <param name="width">Unidades a añadir de ancho</param>
        /// <param name="height">Unidades a añadir de alto</param>
        //protected void inflateContainer(int width, int height)
        //{
        //    minX -= width;
        //    maxX += width;
        //    minY -= height;
        //    maxY += height;
        //}

        //protected override long xScale(int xx)
        //{
        //    long salida = base.xScale(xx);
        //    if (salida < minX) minX = salida;
        //    if (salida > maxX) maxX = salida;
        //    return salida;
        //}
        //protected override long yScale(int yy)
        //{
        //    long salida = base.yScale(yy);
        //    if (salida < minY) minY = salida;
        //    if (salida > maxY) maxY = salida;
        //    return salida;
        //}
        //protected void mergeContainer(DynamicElement element)
        //{
        //    if(element.minX<minX) minX = element.minX;
        //    if(element.minY<minY) minY = element.minY;
        //    if (element.maxX > maxX) maxX = element.maxX;
        //    if (element.maxY>maxY) maxY = element.maxY;            
        //}

        /// <summary>
        /// Añade el rectángulo de selección, para que sea más fácil encontrar un determinado elemento.
        /// </summary>
        //protected void addContainerRegion(bool selected)
        //{
        //    mvarBuilder.OpenElement(20, "rect");
        //    mvarBuilder.AddAttribute(21, "x", minX);
        //    mvarBuilder.AddAttribute(22, "y", minY);
        //    mvarBuilder.AddAttribute(23, "width", maxX - minX);
        //    mvarBuilder.AddAttribute(24, "height", maxY - minY);
        //    mvarBuilder.AddAttribute(25, "fill", "transparent");
        //    if(selected) 
        //        mvarBuilder.AddAttribute(26, "style", "stroke:gray;stroke-width:1;stroke-dasharray:6,7");
        //    mvarBuilder.CloseElement();
        //}
        //protected void closeContainerRegion()
        //{
        //    addContainerRegion(selected);
        //    mvarBuilder.CloseElement();
        //}
        public bool HasChanged { get; set; }
        public DynamicElement():base()
        {
            labelX = -1;
            labelY = -1;
        }
            
    }
}
