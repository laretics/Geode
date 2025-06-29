using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.LayoutView.Elements.Portables;

namespace MontefaroMatias.LayoutView.Elements.Layout
{
    internal class Frog:DynamicElement
    {
        public Unit parent { get; set; } //Unidad a la que pertenece la marmita.
        public int X { get; set; } //Posición X de la marmita.
        public int Y { get; set; } //Posición Y de la marmita.
        public int width { get; set; } //Ancho de la marmita.
        public int height { get; set; } //Alto de la marmita.

        public PortableFrog portableFrog 
        { 
            get
            {
                PortableFrog pf = new PortableFrog();
                pf.xf = this.X;
                pf.yf = this.Y;
                pf.wf = this.width;
                pf.hf = this.height;
                return pf;
            }                
            set
            {
                this.X = value.xf;
                this.Y = value.yf;
                this.width = value.wf;
                this.height = value.hf;
            }
        }
        public override void CompileSVG(SVGRender renderer)
        {
            renderer.rectangle(X,Y,X+width, Y + height);
        }
        public override bool parse(XmlNode node)
        {
            X = parseInt(node, "x");
            Y = parseInt(node, "y");
            width = parseInt(node, "w");
            height = parseInt(node, "h");
            return true;
        }
    }
}
