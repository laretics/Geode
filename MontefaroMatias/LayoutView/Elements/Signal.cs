using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static MontefaroMatias.Common;

namespace MontefaroMatias.LayoutView.Elements
{
    public class Signal:DynamicElement
    {
        public int id {  get; set; }
        public bool shunt { get; set; }
        public string? circuit { get; set; } //Circuito que protege esta señal
        public string? advance { get; set; } //Señal de avanzada de esta señal        
        public Common.Orientation orientation { get; set; }

        private Common.orderType mvarOrder;
        public Common.orderType Order 
        {
            get => mvarOrder;
            set
            {
                if (mvarOrder != value)
                {
                    mvarOrder = value;
                    this.HasChanged = true;
                }
            }
        }
        public Signal() : base() 
        {
            this.Order = orderType.toParada;
        }
        public Signal(int xx, int yy,Common.Orientation orient,int id, string key,bool shunt=false):base()
        {
            X = xx; Y = yy;
            orientation = orient;
            this.id = id;
            this.shunt = shunt;
            name = key;
            this.Order = orderType.toParada;
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            id = parseInt(node, "id");            
            shunt = parseBoolean(node, "shunt");
            orientation = parseOrientation(node, "orientation");
            circuit = parseString(node, "circuit");
            advance = parseString(node, "advance");
            return true;
        }

        public override void CompileSVG(SVGRender renderer)
        {
            base.CompileSVG(renderer);
            renderer.openGroup(string.Format("sig_{0}", id), true);
            switch(orientation)
            {
                case Orientation.North:
                    renderer.openGroup("mst", true, "stroke:red;stroke-width:2");
                    renderer.openGroup("bs",true, "stroke-width:3");
                    renderer.line(-5, 23, 5, 23); //Base
                    renderer.closeGroup();
                    renderer.line(0, 5, 0, 22); //Mástil
                    renderer.closeGroup();
                    renderer.openGroup("rbs", true, "fill:transparent");
                    renderer.square(0, 10, 9); //Rebase autorizado
                    renderer.closeGroup();
                    renderer.openGroup("cir", true, "fill:red");
                    renderer.circle(0, 0, 5); //Círculo
                    renderer.closeGroup();
                    renderer.openGroup("lbl",true, "fill:lightgrey;");
                    renderer.label(-8, 24, 24, 14, "white", name);
                    //renderer.text(x: -8, y: 24, text: name);
                    renderer.closeGroup();
                    break;
                case Orientation.South:
                    renderer.openGroup("mst", true, "stroke:red;stroke-width:2");
                    renderer.openGroup("bs", true, "stroke-width:3");
                    renderer.line(-5, -5, 5, -5); //Base
                    renderer.closeGroup();
                    renderer.line(0, -4, 0, 14); //Mástil
                    renderer.closeGroup();
                    renderer.openGroup("rbs", true, "fill:transparent");
                    renderer.square(0, 9, 9); //Rebase autorizado
                    renderer.closeGroup();
                    renderer.openGroup("cir", true, "fill:red");
                    renderer.circle(0, 19, 5); //Círculo
                    renderer.closeGroup();
                    renderer.openGroup("lbl", true, "fill:lightgrey;");
                    //renderer.text(x: -16, y: -26, text: name);
                    renderer.label(-16, -26, 24, 14, "white", name);
                    renderer.closeGroup();
                    break;
                case Orientation.East:
                    renderer.openGroup("mst", true, "stroke:red;stroke-width:2");
                    renderer.openGroup("bs", true, "stroke-width:3");
                    renderer.line(-2, -5, -2, 5); //Base
                    renderer.closeGroup();
                    renderer.line(-2, 0, 16, 0); //Mástil
                    renderer.closeGroup();
                    renderer.openGroup("rbs", true, "fill:transparent");
                    renderer.square(11, 0, 9); //Rebase autorizado
                    renderer.closeGroup();
                    renderer.openGroup("cir", true, "fill:red");
                    renderer.circle(21, 0, 5); //Círculo
                    renderer.closeGroup();
                    renderer.openGroup("lbl", true, "fill:lightgrey;");
                    //renderer.text(x: -30, y: -8, text: name);
                    renderer.label(-30, -8, 24, 14, "white", name);
                    renderer.closeGroup();
                    break;
                case Orientation.West:
                    renderer.openGroup("mst", true, "stroke:red;stroke-width:2");
                    renderer.openGroup("bs", true, "stroke-width:3");
                    renderer.line(23, -5, 23, 5); //Base
                    renderer.closeGroup();
                    renderer.line(5, 0, 22, 0); //Mástil
                    renderer.closeGroup();
                    renderer.openGroup("rbs", true, "fill:transparent");
                    renderer.square(10, 0, 9); //Rebase autorizado
                    renderer.closeGroup();
                    renderer.openGroup("cir", true, "fill:red");
                    renderer.circle(0, 0, 5); //Círculo
                    renderer.closeGroup();
                    renderer.openGroup("lbl", true, "fill:lightgrey;");
                    //renderer.text(x: 20, y: -8, text: name);
                    renderer.label(20, -8, 24, 14, "white", name);
                    renderer.closeGroup();
                    break;
            }
            renderer.closeGroup();
        }

        public void Roll()
        {
            switch (Order)
            {
                case Common.orderType.toParada:
                    Order = Common.orderType.toAvisoDeParada; break;
                case Common.orderType.toAvisoDeParada:
                    Order = Common.orderType.toViaLibre; break;
                case Common.orderType.toViaLibre:
                    Order = Common.orderType.toParada; break;
                default:
                    Order = Common.orderType.toParada; break;
            }
        }

        private string MastilColor
        {
            get
            {
                switch (Order)
                {
                    case Common.orderType.toViaLibre:                    
                        return "green";
                    case Common.orderType.toAvisoDeParada:
                    case Common.orderType.toPrecaucion:
                        return "yellow";
                    case Common.orderType.toParada:
                    case Common.orderType.toRebaseAutorizado:
                        return "red";
                    default:
                        return "magenta";
                }
            }
        }
        private string SquareColor
        {
            get
            {
                if (!shunt) return "transparent";
                switch (Order)
                {
                    case Common.orderType.toParada:
                        return "red";
                    case Common.orderType.toRebaseAutorizado:
                        return "white";
                    default:
                        return "transparent";
                }
            }
        }
        private string CircleColor
        {
            get
            {
                switch (Order)
                {
                    case Common.orderType.toViaLibre:
                    case Common.orderType.toPrecaucion:
                        return "green";                    
                    case Common.orderType.toAvisoDeParada:
                        return "yellow";
                    case Common.orderType.toParada:
                    case Common.orderType.toRebaseAutorizado:                    
                        return "red";
                    default:
                        return "magenta";
                }
            }
        }
    }

    
}
