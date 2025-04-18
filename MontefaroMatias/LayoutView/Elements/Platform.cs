using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    class Platform:StaticElement
    {
        public Common.Orientation orientation { get; set; }
        public int length { get; set; } //Longitud del andén.
        public Platform():base(){}

        public override PortableElement portableElement 
        { 
            get
            {
                PortablePlatform salida = new PortablePlatform();
                salida.setBase(X, Y, name);
                salida.or=(byte)orientation;
                salida.l = length;                
                return salida;
            }           
        }
        protected override void deserializeFromPortable(PortableElement rhs)
        {
            base.deserializeFromPortable(rhs);
            PortablePlatform xPlatform = (PortablePlatform) rhs;
            this.orientation = (Common.Orientation)xPlatform.or;
            this.length = xPlatform.l;
        }

        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) return false;
            this.length = parseInt(node, "l");
            orientation = parseOrientation(node, "orientation");
            return true;
        }
        public override bool compose(RenderTreeBuilder builder, View view)
        {
            if (!base.compose(builder, view)) return false;
            switch(orientation)
            {
                case Common.Orientation.North:
                    addLine(0, 2, length, 2, "white", 4);
                    addLine(0, 6, length, 6, "white", 4,true);
                    break;
                case Common.Orientation.South:
                    addLine(0, 2, length, 2, "white", 4, true);
                    addLine(0, 6, length, 6, "white", 4);                    
                    break;
                case Common.Orientation.East:
                    addLine(3, 0, 3, length, "white", 4, true);
                    addLine(10,0,10,length, "white", 4);                    
                    break;
                case Common.Orientation.West:
                    addLine(4, 0, 4, length, "white", 4);
                    addLine(11, 0, 11, length, "white", 4, true);
                    break;
            }
            return true;
        }
    }
    public class PortablePlatform:PortableElement
    {
        public PortablePlatform() : base(1)
        { }
        public byte or { get; set; }
        public int l { get; set; } //Longitud del andén.
    }
}
