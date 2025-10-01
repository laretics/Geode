using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace MontefaroMatias.LayoutView.Elements
{
    public abstract class Element:BasicSerializableElement
    {
        public string? name { get; set; }
        public Element():base()
        {}
        public override bool parse(XmlNode node)
        {
            name = parseString(node, "name");
            return (!string.IsNullOrEmpty(name));
        }        

        /// <summary>
        /// Esta función genera el elemento para su visualización en un DOM.
        /// </summary>
        /// <param name="engine"></param>

        //Parámetros XML
    }


}
