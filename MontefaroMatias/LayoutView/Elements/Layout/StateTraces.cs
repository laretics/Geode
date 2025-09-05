using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements.Layout
{
    /// <summary>
    /// Es un estado del circuito de vía, que afecta a la colocación de los aparatos y a los tramos activos o inactivos
    /// Lo único que me interesa son los trazos.
    /// </summary>
    public class StateTraces : StaticElement
    {
        internal List<Trace> mcolActive { get; private set; } //Lista de trazos activos en esta configuración.
        internal List<Trace> mcolPasive { get; private set; } //Lista de trazos inactivos en esta configuración.
        internal string activeIndex { get; set; } //Índice de la configuración activa.
        internal string pasiveIndex { get; set; } //Índice de la configuración pasiva.
        internal StateTraces()
        {
            mcolActive = new List<Trace>();
            mcolPasive = new List<Trace>();
            activeIndex = string.Empty;
            pasiveIndex = string.Empty;
        }
        public void CompileSVGex(SVGRender renderer,int id, bool visibleStart = false)
        {
            string auxIndex = string.Format("ac_{0}_{1}", id, name);
            activeIndex = renderer.openGroup(auxIndex, visibleStart, "stroke-width:5;stroke:yellow;");
            foreach (Trace trace in mcolActive)
                renderer.line(trace.x0, trace.y0, trace.x1, trace.y1);
            renderer.closeGroup();
            auxIndex = string.Format("pa_{0}_{1}", id, name);
            pasiveIndex = renderer.openGroup(auxIndex, visibleStart, "stroke-width:5;stroke:yellow;");
            foreach (Trace trace in mcolPasive)
                renderer.line(trace.x0, trace.y0, trace.x1, trace.y1);
            renderer.closeGroup();
        }
        public override bool parse(XmlNode node)
        {
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    switch (hijo.Name)
                    {
                        case "active":
                            parseGeneric(hijo, mcolActive);
                            break;
                        case "inactive":
                            parseGeneric(hijo, mcolPasive);
                            break;
                    }
                }
            }
            return true;
        }

        internal void parseGeneric(XmlNode node, List<Trace> destination)
        {
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element && hijo.Name == "tr")
                {
                    Trace traza = new Trace();
                    traza.x0 = parseInt(hijo, "x1");
                    traza.y0 = parseInt(hijo, "y1");
                    traza.x1 = parseInt(hijo, "x2");
                    traza.y1 = parseInt(hijo, "y2");
                    destination.Add(traza);
                }
            }
        }
    }
}
