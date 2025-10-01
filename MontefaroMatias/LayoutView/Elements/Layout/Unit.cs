using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements.Layout
{
    /// <summary>
    /// Circuito de vía.
    /// Modificación: Los circuitos ya no van a tener la descripción gráfica en la estructura.
    /// El SVG será cosa de InkScape.
    /// </summary>
    public class Unit:DynamicElement
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }  //Un circuito puede estar desactivado por no tener aparatos ni sensores.
        public byte CurrentPosition { get; set; } //Combinación de posiciones de las marmitas.
        public Common.layoutTraceStatus CurrentStatus { get; set; } //Estado del trazado de la unidad. 
        internal string[] mcolConfigIndexes { get;private set; }

        public Unit() : base()
        {
            mcolConfigIndexes = new string[0]; //Inicialmente no hay configuraciones.
            this.Id = 0;
            this.Enabled = true;
            this.CurrentPosition = 0;
            this.CurrentStatus = Common.layoutTraceStatus.ltDisabled;
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            Id = parseInt(node, "id");
            Enabled = !parseBoolean(node, "disabled"); //Por defecto, la unidad está habilitada.
            return true;
        }
    }
}
