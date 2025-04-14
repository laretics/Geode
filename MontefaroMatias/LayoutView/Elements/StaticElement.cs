using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    public abstract class StaticElement:Element
    {
        internal string? cssClass { get; set; } //Este elemento sirve para dar color y forma a los objetos estáticos.

        public StaticElement():base(){}

        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) return false;
            cssClass = parseString(node,"class");
            return true;
        }
    }
    
}
