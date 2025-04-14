using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    public class LayoutUnit:DynamicElement
    {
        public int id {  get; set; }
        internal List<Frog> mcolFrogs;
        internal List<Trace> mcolTraces;
        public Common.layoutTraceStatus currentStatus { get; set; } //Estado actual del enclavamiento
        private byte mvarCurrentPosition;

        public byte currentPosition
        //Combinación de agujas actual
        {
            get => mvarCurrentPosition; 
            set
            {
                if (value!=mvarCurrentPosition)
                {
                    mvarCurrentPosition = value;
                    this.HasChanged = true;
                }
            }
        } 

        public LayoutUnit(): base()
        {
            mcolFrogs = new List<Frog>();
            mcolTraces = new List<Trace>();
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            this.id = parseInt(node, "id");
            foreach (XmlNode child in node.ChildNodes)
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    if(child.Name.Equals("frogs"))
                    {
                        if(!parseFrogs(child)) 
                            return false;
                    }
                    else if(child.Name.Equals("traces"))
                    {
                        if(!parseTraces(child)) 
                            return false;
                    }
                }                
            }
            return true;
        }
        private bool parseFrogs(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    if(child.Name.Equals("frog"))
                    {
                        Frog nuevo = new Frog();
                        if(!nuevo.parse(child)) return false;
                        nuevo.X = this.X;
                        nuevo.Y = this.Y;
                        nuevo.parent = this;
                        mcolFrogs.Add(nuevo);
                    }
                }
            }
            return true;
        }

        private bool parseTraces(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    if(child.Name.Equals("v"))
                    {
                        if (!parseTracesEx(child)) 
                            return false;
                    }
                }
            }
            return true;
        }
        private bool parseTracesEx(XmlNode node)
        {
            int position = parseInt(node,"id");
            foreach(XmlNode child in node.ChildNodes)
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    if(child.Name.Equals("inactive"))
                    {
                         foreach(XmlNode child2 in child.ChildNodes)
                        {
                            if (child2.NodeType == XmlNodeType.Element)
                            {
                                if(child2.Name.Equals("tr"))
                                {
                                    if (!parseTrace(child2, position, false))
                                        return false;
                                }

                            }
                        }
                    }
                    else if (child.Name.Equals("active"))
                    {
                        foreach (XmlNode child2 in child.ChildNodes)
                        {
                            if (child2.NodeType == XmlNodeType.Element)
                            {
                                if (child2.Name.Equals("tr"))
                                {
                                    if (!parseTrace(child2, position, true))
                                        return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private bool parseTrace(XmlNode node, int id, bool active)
        {
            Trace nuevo = new Trace(this);            
            nuevo.active = active;
            nuevo.layoutSelector = id;
            mcolTraces.Add(nuevo);
            if (!nuevo.parse(node)) return false;
            nuevo.X = this.X;
            nuevo.Y = this.Y;
            return true;
        }

        public override void compose(RenderTreeBuilder builder)
        {
            base.compose(builder);
            openContainerRegion();
            //addText(0, 0, name, "magenta");
            foreach (Frog frog in mcolFrogs)
                frog.compose(builder);
            foreach (Trace trace in mcolTraces)
                trace.compose(builder);
            closeContainerRegion();
        }
        public class Trace:DynamicElement
        {
            public Trace(LayoutUnit parent):base()
            {
                this.parent = parent;
            }
            public int x0 { get; set; }
            public int y0 { get; set; }
            public int x1 { get; set; }
            public int y1 { get; set; }
            public int layoutSelector {  get; set; } //Es el índice para el que este elemento se dibuja
            public bool active { get; set; } //Indica si este trazo es activo o no
            public LayoutUnit parent { get; set; } //Referencia al padre de este trazo.

            internal string mainColor
            {
                get
                {
                    if (parent.currentPosition != layoutSelector)
                        return "transparent";

                    if (Common.layoutTraceStatus.ltDisabled == parent.currentStatus)
                        return "grey";

                    if (!active)
                        return "yellow";

                    switch(parent.currentStatus)
                    {
                        case Common.layoutTraceStatus.ltFree:
                            return "yellow";
                        case Common.layoutTraceStatus.ltLocked:
                            return "green";
                        case Common.layoutTraceStatus.ltShunt:
                            return "blue";
                        case Common.layoutTraceStatus.ltOccupied:
                            return "red";                        
                    }
                    
                    return "magenta"; //Trazo por defecto
                }
            }
            public override void compose(RenderTreeBuilder builder)
            {
                base.compose(builder);
                addLine(x0, y0, x1, y1, mainColor, 5);
            }
            public override bool parse(XmlNode node)
            {
                //if (!base.parse(node)) return false;
                x0 = parseInt(node, "x1");
                x1 = parseInt(node, "x2");
                y0 = parseInt(node, "y1");
                y1 = parseInt(node, "y2");
                return true;
            }
        }

        public class Frog:DynamicElement
        {
            public LayoutUnit parent {  get; set; }
            public int xx { get; set; }
            public int yy { get; set; }
            public int width {  get; set; }
            public int height { get; set; }
            public override void compose(RenderTreeBuilder builder)
            {
                base.compose(builder);
                addRectangle(xx, yy, xx+width, yy+height, myFill);
            }
            public override bool parse(XmlNode node)
            {
                //if (!base.parse(node)) return false;
                xx = parseInt(node, "x");
                yy = parseInt(node, "y");
                width = parseInt(node,"w");
                height = parseInt(node,"h");
                return true;
            }
            public string myFill 
            {
                get
                {
                    if(null!=parent)
                    {
                        switch (parent.currentStatus)
                        {
                            case Common.layoutTraceStatus.ltFree:
                                return "white";
                            case Common.layoutTraceStatus.ltLocked:
                            case Common.layoutTraceStatus.ltShunt:
                            case Common.layoutTraceStatus.ltOccupied:
                                return "blue";
                        }
                    }
                    return "magenta";
                }
            }
        }
    }
}
