using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Models.Elements
{
    public class SignalModel:ElementModel
    {
        public SignalModel():base()
        { }
        
        public byte com { get; set; } //Orden actual de la señal
    }
}
