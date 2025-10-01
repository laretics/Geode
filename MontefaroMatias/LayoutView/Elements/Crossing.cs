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
            return true;            
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
