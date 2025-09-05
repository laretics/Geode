using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Models.Elements
{
    public class LayoutModel:ElementModel
    {
        public List<LayoutUnitModel> units { get; set; }
        public LayoutModel():base()
        {
            units = new List<LayoutUnitModel>();

        }
        
    }
}
