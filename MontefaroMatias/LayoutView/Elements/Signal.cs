﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static MontefaroMatias.Common;

namespace MontefaroMatias.LayoutView.Elements
{
    public class Signal:DynamicElement
    {
        public int id {  get; set; }
        public bool shunt { get; set; }
        public string? circuit { get; set; } //Circuito que protege esta señal
        public string? advance { get; set; } //Señal de avanzada de esta señal        
        public Common.Orientation orientation { get; set; }

        public override PortableElement portableElement 
        { 
            get
            {
                PortableSignal salida = new PortableSignal();
                salida.setBase(X, Y, name);
                salida.or = (byte)orientation;
                salida.sh = shunt;
                salida.id = id;
                salida.com = (byte)Order;
                return salida;
            }
        }
        protected override void deserializeFromPortable(PortableElement rhs)
        {
            base.deserializeFromPortable(rhs);
            PortableSignal xSignal = (PortableSignal)rhs;
            this.orientation=(Common.Orientation)xSignal.or;
            this.shunt = xSignal.sh;
            this.id = xSignal.id;
            this.Order = (Common.orderType)xSignal.com;
        }

        private Common.orderType mvarOrder;
        public Common.orderType Order 
        {
            get => mvarOrder;
            set
            {
                if (mvarOrder != value)
                {
                    mvarOrder = value;
                    this.HasChanged = true;
                }
            }
        }
        public Signal() : base() 
        {
            this.Order = orderType.toParada;
        }
        public Signal(long xx, long yy,Common.Orientation orient,int id, string key,bool shunt=false):base()
        {
            X = xx; Y = yy;
            orientation = orient;
            this.id = id;
            this.shunt = shunt;
            name = key;
            this.Order = orderType.toParada;
        }
        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            id = parseInt(node, "id");            
            shunt = parseBoolean(node, "shunt");
            orientation = parseOrientation(node, "orientation");
            circuit = parseString(node, "circuit");
            advance = parseString(node, "advance");
            return true;
        }
        public override bool compose(RenderTreeBuilder builder, View view)
        {
            if (!base.compose(builder, view)) return false;
            openContainerRegion();
            switch (orientation)
            {
                case Common.Orientation.North:
                    addLine(- 5, 23,  5, 23, MastilColor, 3); //Base
                    addLine(0, 5, 0, 22, MastilColor, 2); //Mástil
                    addSquare(0, 10, 9, SquareColor); //Rebase autorizado
                    addCircle(0, 0, 5, CircleColor); //Círculo
                    labelX = -8;labelY = 24;
                    break;
                case Common.Orientation.South:
                    addLine(- 5, - 5, 5, - 5, MastilColor, 3); //Base
                    addLine(0, - 4, 0, 14, MastilColor, 2); //Mástil
                    addSquare(0, 9, 9, SquareColor); //Rebase autorizado
                    addCircle(0, 19, 5, CircleColor); //Círculo
                    labelX = -16; labelY = -26;
                    break;
                case Common.Orientation.East:
                    addLine(- 2, -5,- 2, 5, MastilColor, 3); //Base
                    addLine(- 2, 0, 16, 0, MastilColor, 2); //Mástil
                    addSquare(11, 0, 9, SquareColor); //Rebase autorizado
                    addCircle(21, 0, 5, CircleColor); //Círculo
                    labelX = -30; labelY = -8;
                    break;
                case Common.Orientation.West:
                    addLine(23, - 5, 23, 5, MastilColor, 3); //Base
                    addLine( 5, 0, 22, 0, MastilColor, 2); //Mástil
                    addSquare( 10, 0, 9, SquareColor); //Rebase autorizado
                    addCircle(0, 0, 5, CircleColor); //Círculo
                    labelX = 20; labelY = -8;
                    break;
            }
            addLabel(false,labelX, labelY,32,32,"light",name);
            inflateContainer(10, 10);
            closeContainerRegion();
            return true;
        }

        public void Roll()
        {
            switch (Order)
            {
                case Common.orderType.toParada:
                    Order = Common.orderType.toAvisoDeParada; break;
                case Common.orderType.toAvisoDeParada:
                    Order = Common.orderType.toViaLibre; break;
                case Common.orderType.toViaLibre:
                    Order = Common.orderType.toParada; break;
                default:
                    Order = Common.orderType.toParada; break;
            }
        }

        private string MastilColor
        {
            get
            {
                switch (Order)
                {
                    case Common.orderType.toViaLibre:                    
                        return "green";
                    case Common.orderType.toAvisoDeParada:
                    case Common.orderType.toPrecaucion:
                        return "yellow";
                    case Common.orderType.toParada:
                    case Common.orderType.toRebaseAutorizado:
                        return "red";
                    default:
                        return "magenta";
                }
            }
        }
        private string SquareColor
        {
            get
            {
                if (!shunt) return "transparent";
                switch (Order)
                {
                    case Common.orderType.toParada:
                        return "red";
                    case Common.orderType.toRebaseAutorizado:
                        return "white";
                    default:
                        return "transparent";
                }
            }
        }
        private string CircleColor
        {
            get
            {
                switch (Order)
                {
                    case Common.orderType.toViaLibre:
                    case Common.orderType.toPrecaucion:
                        return "green";                    
                    case Common.orderType.toAvisoDeParada:
                        return "yellow";
                    case Common.orderType.toParada:
                    case Common.orderType.toRebaseAutorizado:                    
                        return "red";
                    default:
                        return "magenta";
                }
            }
        }
    }
    public class PortableSignal : PortableElement
    {
        public PortableSignal() : base(3)
        { }
        public int id { get; set; }
        public bool sh { get; set; }
        public byte or { get; set; }
        public byte com { get; set; } //Orden actual de esta señal
    }
    
}
