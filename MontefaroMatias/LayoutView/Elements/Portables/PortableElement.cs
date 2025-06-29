using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.LayoutView.Elements.Portables
{
    /// <summary>
    /// Este elemento es como el normal, pero está preparado para ser serializado
    /// </summary>
    public class PortableElement
    {
        public PortableElement(byte type)
        {
            typ = type;
        }
        public void setBase(int x, int y, string? name)
        {
            this.X = x;
            this.Y = y;
            this.nme = name;
        }
        public int X { get; set; } //Es la posición a representar de este elemento
        public int Y { get; set; } //Su contenido se representa relativa a la posición total.
        public string? nme { get; set; }
        public virtual byte typ { get; private set; } //Este tipo sirve para la deserialización
    }
}
