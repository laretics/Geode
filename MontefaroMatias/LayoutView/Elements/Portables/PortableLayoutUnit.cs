using MontefaroMatias.LayoutView.Elements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    public class PortableLayoutUnit : PortableElement
    {
        public int id { get; set; }
        public int lbx { get; set; }
        public int lby { get; set; }
        public byte pos { get; set; }
        public byte sta { get; set; }
        public bool ena { get; set; }
        public List<PortableFrog> frg { get; set; }
        public List<PortableLayoutConfig> cfg { get; set; } //Lista de configuraciones de la unidad.
        public PortableLayoutUnit() : base(6)
        {
            frg = new List<PortableFrog>();
            cfg = new List<PortableLayoutConfig>();
        }
    }
}
