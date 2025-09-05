using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Models.Elements
{
    public class LayoutUnitModel:ElementModel
    {
        public LayoutUnitModel() : base() { }
        public byte stt { get; set; } //Estado actual
        public byte con { get; set; } //Configuración actual
    }
}
