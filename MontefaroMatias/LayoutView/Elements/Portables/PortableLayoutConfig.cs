using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    //Elemento que contiene los trazos de una configuración determinada.
    public class PortableLayoutConfig : PortableElement
    {
        public PortableLayoutConfig() : base(3)
        {
            act = new List<PortableTrace>();
            psv = new List<PortableTrace>();
        }
        public List<PortableTrace> act { get; set; } //Lista de trazos activos de la configuración.
        public List<PortableTrace> psv { get; set; } //Lista de trazos inactivos o pasivos de la configuración.
    }
}
