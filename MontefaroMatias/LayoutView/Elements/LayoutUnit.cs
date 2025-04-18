using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView.Elements
{
    public class LayoutUnit:DynamicElement
    {
        public int id {  get; set; }
        internal List<Frog> mcolFrogs { get; set; }
        internal List<Trace> mcolTraces { get; set; }
        public override PortableElement portableElement 
        { 
            get
            {
                PortableLayoutUnit salida = new PortableLayoutUnit();
                salida.setBase(X, Y, name);
                salida.id = id;
                salida.lbx = labelX;
                salida.lby = labelY;
                salida.pos = mvarCurrentPosition;
                salida.sta = (byte)currentStatus;
                foreach(Frog frog in mcolFrogs)
                {
                    PortableFrog xFrog = (PortableFrog)frog.portableElement;
                    salida.frg.Add(xFrog);                       
                }
                foreach(Trace trace in mcolTraces)
                {
                    PortableTrace xTrace = (PortableTrace)trace.portableElement;
                    salida.trc.Add(xTrace);
                }
                return salida;
            }
        }
        protected override void deserializeFromPortable(PortableElement rhs)
        {
            base.deserializeFromPortable(rhs);
            PortableLayoutUnit xUnit = (PortableLayoutUnit)rhs;
            id = xUnit.id;
            labelX = xUnit.lbx;
            labelY = xUnit.lby;
            mvarCurrentPosition = xUnit.pos;
            currentStatus = (Common.layoutTraceStatus)xUnit.sta;
            foreach (PortableFrog frog in xUnit.frg)
            {
                Frog auxFrog = new Frog();
                auxFrog.portableElement = frog;
                auxFrog.parent = this;
                mcolFrogs.Add(auxFrog);
            }
            foreach (PortableTrace trace in xUnit.trc)
            {
                Trace auxTrace = new Trace(this);                
                auxTrace.portableElement = trace;
                mcolTraces.Add(auxTrace);
            }
        }
        
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
                    else if(child.Name.Equals("label"))
                    {
                        if(!parseLabel(child)) 
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
        private bool parseLabel(XmlNode node)
        {
            labelX = parseInt(node, "x");
            labelY = parseInt(node, "y");
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
            {
                trace.resetContainer();
                trace.compose(builder);
                mergeContainer(trace);
            }
            if(labelX<0)
                addLabel(true, (int)((minX + maxX) / 2), (int)((minY + maxY) / 2), 32, 32, name);
            else
                addLabel(false,labelX,labelY,32,32, name);

                inflateContainer(10, 10);            
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
            public int layoutSelector { get; set; } //Es el índice para el que este elemento se dibuja            
            public override PortableElement portableElement 
            { 
                get
                {
                    PortableTrace salida = new PortableTrace();
                    salida.setBase(X, Y, name);
                    salida.x0 = x0;
                    salida.y0 = y0;
                    salida.x1 = x1;
                    salida.y1 = y1;
                    salida.sel = layoutSelector;
                    salida.ac = active;
                    return salida;
                }                
            }
            protected override void deserializeFromPortable(PortableElement rhs)
            {
                base.deserializeFromPortable(rhs);
                PortableTrace xTrace = (PortableTrace) rhs;
                this.x0 = xTrace.x0;
                this.y0 = xTrace.y0;
                this.x1 = xTrace.x1;
                this.y1 = xTrace.y1;
                this.active = xTrace.ac;
                this.layoutSelector = xTrace.sel;
            } 
            public LayoutUnit parent { get; set; } //Referencia al padre de este trazo.
            public bool active { get; set; } //Indica si este trazo es activo o no

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
        public class PortableTrace:PortableElement
        {
            public PortableTrace():base(5)
            { }
            public int x0 { get; set; }
            public int y0 { get; set; }
            public int x1 { get; set; }
            public int y1 { get; set; }
            public int sel { get; set; }
            public bool ac { get; set; } //Trazo activo.
        }
        public class Frog:DynamicElement
        {
            public LayoutUnit parent {  get; set; }
            public int xx { get; set; }
            public int yy { get; set; }
            public int width {  get; set; }
            public int height { get; set; }
            
            public override PortableElement portableElement 
            { 
                get
                {
                    PortableFrog salida = new PortableFrog();
                    salida.setBase(X, Y, name);
                    salida.xf = xx;
                    salida.yf = yy;
                    salida.wf = width;
                    salida.hf = height;
                    return salida;
                }
                set => deserializeFromPortable(value);
            }
            protected override void deserializeFromPortable(PortableElement rhs)
            {
                base.deserializeFromPortable(rhs);
                PortableFrog xFrog = (PortableFrog) rhs;
                this.xx = xFrog.xf;
                this.yy = xFrog.yf;
                this.width = xFrog.wf;
                this.height = xFrog.hf;
            }
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
        public class PortableFrog:PortableElement
        {
            public PortableFrog():base(4)
            {}
            public int xf { get; set; }
            public int yf { get; set; }
            public int wf { get; set; }
            public int hf { get; set; }

        }
    }
    public class PortableLayoutUnit:PortableElement
    {
        public int id { get; set; }
        public int lbx {  get; set; }
        public int lby { get; set; }
        public byte pos { get; set; }
        public byte sta {  get; set; }        
        public List<LayoutUnit.PortableFrog> frg { get; set; }
        public List<LayoutUnit.PortableTrace> trc { get; set; }
        public PortableLayoutUnit() : base(6) 
        {
            frg = new List<LayoutUnit.PortableFrog>();
            trc = new List<LayoutUnit.PortableTrace>();
        }
    }
}
