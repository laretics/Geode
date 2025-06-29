using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MontefaroMatias.LayoutView.Elements.Portables;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    class Platform:StaticElement
    {
        public Common.Orientation orientation { get; set; }
        public int length { get; set; } //Longitud del andén.
        public Platform():base(){}
        protected static int ComposeId = 0; //Identificador de composición para este elemento, para evitar colisiones con otros elementos que se puedan crear en el futuro.

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

        public override void CompileSVG(SVGRender renderer)
        {
            base.CompileSVG(renderer);
            renderer.openGroup(string.Format("ptf_{0}", ComposeId++), true);
            renderer.openGroup("solid", true, "stroke:white;stroke-width:4");
            switch (orientation)
            {
                case Common.Orientation.North:
                    renderer.line(0, 2, length, 2);
                    break;
                case Common.Orientation.South:
                    renderer.line(0, 6, length, 6);
                    break;
                case Common.Orientation.East:
                    renderer.line(10, 0, 10, length);
                    break;
                case Common.Orientation.West:
                    renderer.line(4, 0, 4, length);
                    break;
            }
            renderer.closeGroup();
            renderer.openGroup("pattern", true, "stroke:white;stroke-width:4;stroke-dasharray:4,5");
            switch(orientation)
            {
                case Common.Orientation.North:
                    renderer.line(0, 6, length, 6);
                    break;
                case Common.Orientation.South:
                    renderer.line(0, 2, length, 2);
                    break;
                case Common.Orientation.East:
                    renderer.line(3, 0, 3, length);
                    break;
                case Common.Orientation.West:
                    renderer.line(11, 0, 11, length);
                    break;
            }
            renderer.closeGroup();
            renderer.closeGroup();
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
