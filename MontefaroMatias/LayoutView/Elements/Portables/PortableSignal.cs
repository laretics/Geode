using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    public class PortableSignal : PortableElement
    {
        public PortableSignal() : base(3)
        { }
        public int id { get; set; }
        public bool sh { get; set; }
        public byte or { get; set; }
        public byte com { get; set; } //Orden actual de esta señal
    }
}
