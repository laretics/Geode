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
        public static int scaleValue { get; set; } = 1;
        public int X { get; set; } //Es la posición a representar de este elemento
        public int Y { get; set; } //Su contenido se representa relativa a la posición total.        
        public string? name { get; set; }
        public Element():base()
        {}
        public override bool parse(XmlNode node)
        {
            X = parseInt(node, "x");
            Y = parseInt(node, "y");
            name = parseString(node, "name");
            if (X >= 0 && Y >= 0) return true;
            return false;
        }        

        /// <summary>
        /// Esta función genera el elemento para su visualización en un DOM.
        /// </summary>
        /// <param name="engine"></param>

        #region "Compilación SVG Estática"

        public virtual void CompileSVG(MontefaroMatias.LayoutView.SVGRender renderer)
        {
            renderer.setOffset(this.X, this.Y);
        }
        #endregion "Compilación SVG Estática"

        //Parámetros XML

        protected Common.Orientation parseOrientation(XmlNode node, string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            if (null != entrada)
            {
                char car = entrada.ToUpper()[0];
                switch (car)
                {
                    case 'N':
                        return Common.Orientation.North;
                    case 'S':
                        return Common.Orientation.South;
                    case 'E':
                        return Common.Orientation.East;
                    case 'W':
                        return Common.Orientation.West;
                }
            }
            return Common.Orientation.North;
        }
    }


}
