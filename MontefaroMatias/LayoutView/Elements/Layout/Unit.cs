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
    /// </summary>
    public class Unit:DynamicElement
    {
        public int Id { get; set; }
        public bool Enabled { get; set; } 
        //Nota: Los miembros que se refieren a la situación actual serán deserializados en otra estructura.
        //de momento los voy a mantener porque es muy difícil gestionar tantos cambios juntos.
        public byte CurrentPosition { get; set; } //Combinación de posiciones de las marmitas.
        public Common.layoutTraceStatus CurrentStatus { get; set; } //Estado del trazado de la unidad. 
        private string frogsIndex { get; set; } //Índice de las marmitas asociadas a la unidad. Se usa para el renderizado dinámico.
        internal List<Frog> mcolFrogs { get; private set; } //Lista de marmitas asociadas a la unidad.
        internal StateTraces[] mcolConfigurations { get; private set; }
        internal string[] mcolConfigIndexes { get;private set; }

        public Unit() : base()
        {
            mcolFrogs = new List<Frog>();
            frogsIndex = string.Empty; //Inicialmente no hay marmitas asociadas a la unidad.
            mcolConfigurations = new StateTraces[0]; //Inicialmente no hay configuraciones.
            mcolConfigIndexes = new string[0]; //Inicialmente no hay configuraciones.
            this.Id = 0;
            this.Enabled = true;
            this.labelX = int.MinValue;
            this.labelY = int.MinValue;
            this.CurrentPosition = 0;
            this.CurrentStatus = Common.layoutTraceStatus.ltDisabled;
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            Id = parseInt(node, "id");
            Enabled = !parseBoolean(node, "enabled"); //Por defecto, la unidad está habilitada.
            foreach(XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    switch (hijo.Name)
                    {
                        case "frogs":
                            if(!parseFrogs(hijo)) return false;
                            break;
                        case "traces":
                            if (!parseTraces(hijo)) return false;
                            break;
                        case "label":
                            parseLabel(hijo);
                            break;
                    }
                }
            }
            return true;
        }
        private bool parseFrogs(XmlNode node)
        {
            foreach(XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element && hijo.Name == "frog")
                {
                    Frog frog = new Frog();
                    if (!frog.parse(hijo)) return false;
                    mcolFrogs.Add(frog);
                }
            }
            return true;
        }
        private bool parseTraces(XmlNode node)
        {
            List<StateTraces> bloque = new List<StateTraces>();
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element && hijo.Name == "v")
                {
                    StateTraces conf = new StateTraces();
                    if (!conf.parse(hijo)) return false;
                    bloque.Add(conf);
                }
            }
            mcolConfigurations = bloque.ToArray();
            return true;
        }
        private bool parseLabel(XmlNode node)
        {
            labelX = parseInt(node, "x");
            labelY = parseInt(node, "y");
            return true;
        }

        public override void CompileSVG(SVGRender renderer)
        {
            base.CompileSVG(renderer);
            renderer.openGroup(string.Format("lty_{0}", Id), true);
            frogsIndex = renderer.openGroup("frg",true, "stroke:white;fill:white;");
            foreach(Frog frog in mcolFrogs)
                frog.CompileSVG(renderer);
            renderer.closeGroup();
            mcolConfigIndexes = new string[mcolConfigurations.Length];
            bool primero = true;
            int index = 0;
            foreach (StateTraces conf in mcolConfigurations)
            {
                conf.CompileSVGex(renderer,index++,primero);
                primero = false;
            }
            if (int.MinValue != labelX)
            {
                renderer.bagde(labelX, labelY, 24, 14, "white", "#404040", name);
            }
            renderer.closeGroup();
        }
    }
}
