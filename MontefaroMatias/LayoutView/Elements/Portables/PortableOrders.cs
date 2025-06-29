using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    public class portableOrders
    {
        public List<portableOp> or { get; set; }
        public portableOrders()
        {
            or = new List<portableOp>();
        }
    }
}
