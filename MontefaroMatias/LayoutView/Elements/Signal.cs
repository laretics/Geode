using Microsoft.AspNetCore.Components;
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
        public Signal(int id, string key,bool shunt=false):base()
        {
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
            circuit = parseString(node, "circuit");
            advance = parseString(node, "advance");
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

    
}
