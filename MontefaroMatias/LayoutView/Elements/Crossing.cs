using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    /// <summary>
    /// Paso a nivel en el enclavamiento.
    /// </summary>
    public class Crossing:DynamicElement
    {                  
        public int id { get; set; } //Por si algún día tengo un objeto paso con índice
        public Common.Orientation orientation { get; set; }        
        public int length { get; set; } //Longitud del paso a nivel
        public Common.crossingStatus status { get; set; }


        public Crossing():base()
        {
            status = Common.crossingStatus.csUnknown;
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            id = parseInt(node,"id");
            name = parseString(node,"name");
            orientation = parseOrientation(node, "orientation");
            length = parseInt(node,"l");
            return true;            
        }

        public override void CompileSVG(SVGRender renderer)
        {
            base.CompileSVG(renderer);
            int mitad = length / 2;
            int extremo1 = -1 * mitad;
            int extremo2 = mitad;
            int cuerno = 8;
            renderer.openGroup(string.Format("crs_{0}", id), true, $"stroke:red;stroke-width:4");
            switch (orientation)
            {
                case Common.Orientation.North:
                case Common.Orientation.South:
                    renderer.line(0, extremo1, 0, extremo2);
                    renderer.line(-cuerno, extremo1 - cuerno, 0, extremo1);
                    renderer.line(cuerno, extremo1 - cuerno, 0, extremo1);
                    renderer.line(-cuerno, extremo2 + cuerno, 0, extremo2);
                    renderer.line(cuerno, extremo2 + cuerno, 0, extremo2);
                    break;
                default:
                    renderer.line(extremo1, 0, extremo2, 0);
                    renderer.line(extremo1 - cuerno, -cuerno, extremo1, 0);
                    renderer.line(extremo1 - cuerno, cuerno, extremo1, 0);
                    renderer.line(extremo2 + cuerno, -cuerno, extremo2, 0);
                    renderer.line(extremo2 + cuerno, cuerno, extremo2, 0);
                    break;
            }
            renderer.closeGroup();
        }
        protected string statusColor
        {
            get
            {
                switch (status)
                {
                    case Common.crossingStatus.csOpen:
                        return "yellow";
                    case Common.crossingStatus.csClosed:
                        return "red";
                    case Common.crossingStatus.csDisabled:
                        return "grey";                        
                }
                return "magenta";
            }
        }

    }
}
