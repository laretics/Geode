using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    public class PortableFrog : PortableElement
    {
        public PortableFrog() : base(4)
        { }
        public int xf { get; set; }
        public int yf { get; set; }
        public int wf { get; set; }
        public int hf { get; set; }
    }
}
