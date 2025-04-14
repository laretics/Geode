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
    class Crossing:DynamicElement
    {                  
        public int id { get; set; } //Por si algún día tengo un objeto paso con índice
        public Common.Orientation orientation { get; set; }
        public Common.crossingStatus status { get; set; }
        public int length { get; set; } //Longitud del paso a nivel

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
        public override void compose(RenderTreeBuilder builder)
        {
            base.compose(builder);
            openContainerRegion();
            int mitad = length / 2;
            int extremo1 = -1 * mitad;
            int extremo2 = mitad;
            int cuerno = 8;
            switch(orientation)
            {
                case Common.Orientation.North:
                case Common.Orientation.South:
                    addLine(0, extremo1, 0, extremo2, statusColor, 4);
                    addLine(-cuerno, extremo1 - cuerno, 0, extremo1, statusColor, 4);
                    addLine(cuerno, extremo1 - cuerno, 0, extremo1, statusColor, 4);
                    addLine(-cuerno, extremo2 + cuerno, 0, extremo2, statusColor, 4);
                    addLine(cuerno, extremo2 + cuerno, 0, extremo2, statusColor, 4);
                    break;
                case Common.Orientation.East:
                case Common.Orientation.West:
                    addLine(extremo1,0,extremo2,0, statusColor, 4);
                    addLine(extremo1 - cuerno,-cuerno, extremo1,0, statusColor, 4);
                    addLine(extremo1 - cuerno,cuerno, extremo1,0, statusColor, 4);
                    addLine(extremo2 + cuerno,-cuerno, extremo2,0, statusColor, 4);
                    addLine(extremo2 + cuerno,cuerno, extremo2,0, statusColor, 4);
                    break;
            }
            closeContainerRegion();
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
