using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    public class portableOp : PortableElement
    {
        public portableOp() : base(255)
        {
            id = string.Empty;
            or = string.Empty;
            ds = string.Empty;
            sh = string.Empty;
        }
        public string id { get; set; } //Código de la orden
        public string? gr { get; set; } //Grupo
        public string or { get; set; } //Origin
        public string ds { get; set; } //Destination
        public string sh { get; set; } //Indica si la orden es de maniobra o no.   
    }
}
