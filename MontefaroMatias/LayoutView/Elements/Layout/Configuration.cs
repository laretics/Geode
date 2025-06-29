using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.LayoutView.Elements.Portables;

namespace MontefaroMatias.LayoutView.Elements.Layout
{
    /// <summary>
    /// Es un estado del circuito de vía, que afecta a la colocación de los aparatos y a los tramos activos o inactivos
    /// </summary>
    internal class Configuration:StaticElement
    {
        internal List<PortableTrace> mcolActive { get; private set; } //Lista de trazos activos en esta configuración.
        internal List<PortableTrace> mcolPasive { get; private set; } //Lista de trazos inactivos en esta configuración.
        internal string activeIndex { get; set; } //Índice de la configuración activa.
        internal string pasiveIndex { get; set; } //Índice de la configuración pasiva.
        internal Configuration()
        {
            mcolActive = new List<PortableTrace>();
            mcolPasive = new List<PortableTrace>();
            activeIndex = string.Empty;
            pasiveIndex = string.Empty;
        }
        public void CompileSVGex(SVGRender renderer, bool visibleStart=false)
        {
            activeIndex = renderer.openGroup("ac", visibleStart,"stroke-width:5;stroke:yellow;");
            foreach (PortableTrace trace in mcolActive)
                renderer.line(trace.x0, trace.y0, trace.x1, trace.y1);
            renderer.closeGroup();
            pasiveIndex = renderer.openGroup("pa", visibleStart, "stroke-width:5;stroke:yellow;");
            foreach(PortableTrace trace in mcolPasive)
                renderer.line(trace.x0, trace.y0, trace.x1, trace.y1);
            renderer.closeGroup();
        }
        public override bool parse(XmlNode node)
        {
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if(hijo.NodeType == XmlNodeType.Element)
                {
                    switch (hijo.Name)
                    {
                        case "active":
                            parseGeneric(hijo,mcolActive);
                            break;
                        case "inactive":
                            parseGeneric(hijo,mcolPasive);
                            break;
                    }
                }
            }
            return true;
        }
        public PortableLayoutConfig portable
        {
            get
            {
                PortableLayoutConfig salida = new PortableLayoutConfig();
                foreach (PortableTrace traza in mcolActive)
                    salida.act.Add(traza);
                foreach (PortableTrace traza in mcolPasive)
                    salida.psv.Add(traza);
                return salida;
            }
            set 
            {
                if (null == value) return;
                mcolActive.Clear();
                mcolPasive.Clear();
                foreach (PortableTrace traza in value.act)
                    mcolActive.Add(traza);
                foreach (PortableTrace traza in value.psv)
                    mcolPasive.Add(traza);
            }
        }
        internal void parseGeneric(XmlNode node, List<PortableTrace> destination)
        {
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element && hijo.Name == "tr")
                {
                    PortableTrace traza = new PortableTrace();
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
