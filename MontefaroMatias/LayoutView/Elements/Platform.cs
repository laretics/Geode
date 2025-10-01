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
        protected static int ComposeId = 0; //Identificador de composición para este elemento, para evitar colisiones con otros elementos que se puedan crear en el futuro.

        public override bool parse(XmlNode node)
        {
            return (base.parse(node));
        }
    }
}
