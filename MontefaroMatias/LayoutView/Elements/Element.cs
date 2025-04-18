﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MontefaroMatias.LayoutView.Elements
{
    public abstract class Element
    {
        public long X { get; set; } //Es la posición a representar de este elemento
        public long Y { get; set; } //Su contenido se representa relativa a la posición total.        
        public string? name { get; set; }

        public static View? currentView { get; set; } //Asigna la vista actual para calcular las coordenadas a representar.
        public Element()
        {}
        public virtual bool parse(XmlNode node)
        {
            X = parseLong(node, "x");
            Y = parseLong(node, "y");
            name = parseString(node, "name");
            if (X >= 0 && Y >= 0) return true;
            return false;
        }

        public virtual PortableElement portableElement
        {
            get
            {
                PortableElement salida = new PortableElement(0);
                salida.X = X;
                salida.Y = Y;
                salida.nme = name;
                return salida;
            }
            set
            {
                deserializeFromPortable(value);
            }
        }
        protected virtual void deserializeFromPortable(PortableElement rhs)
        {
            this.X = rhs.X;
            this.Y = rhs.Y;
            this.name = rhs.nme;
        }
        //Invoca las funciones de dibujo de este elemento.
        public virtual void compose(RenderTreeBuilder builder)
        {
            mvarBuilder = builder;
        }

        protected RenderTreeBuilder mvarBuilder { get; set; }


        //Estas funciones llaman a las correspondientes de la estructura que componen los trazos.
        protected void addCircle(int xx, int yy, int r, string fill)
        {
            mvarBuilder.OpenElement(0, "circle");
            mvarBuilder.AddAttribute(1, "cx", xScale(xx));
            mvarBuilder.AddAttribute(2, "cy", yScale(yy));
            mvarBuilder.AddAttribute(3, "r", r);
            mvarBuilder.AddAttribute(4, "fill", fill);
            mvarBuilder.CloseElement();
        }
        protected void addLine(int xx0, int yy0, int xx1, int yy1, string stroke, int width, bool dashArray = false)
        {
            mvarBuilder.OpenElement(10, "line");
            mvarBuilder.AddAttribute(11, "x1", xScale(xx0));
            mvarBuilder.AddAttribute(12, "y1", yScale(yy0));
            mvarBuilder.AddAttribute(13, "x2", xScale(xx1));
            mvarBuilder.AddAttribute(14, "y2", yScale(yy1));
            if(dashArray)
                mvarBuilder.AddAttribute(15, "style", string.Format("stroke:{0};stroke-width:{1};stroke-dasharray:6,7", stroke, width));
            else
                mvarBuilder.AddAttribute(15, "style", string.Format("stroke:{0};stroke-width:{1}", stroke, width));

            mvarBuilder.CloseElement();
        }
        protected void addRectangle(int xx0, int yy0, int xx1, int yy1, string fill)
        {
            mvarBuilder.OpenElement(20, "rect");
            mvarBuilder.AddAttribute(21, "x", xScale(xx0));
            mvarBuilder.AddAttribute(22, "y", yScale(yy0));
            mvarBuilder.AddAttribute(23, "width", xx1 - xx0);
            mvarBuilder.AddAttribute(24, "height",yy1 - yy0);
            mvarBuilder.AddAttribute(25, "fill", fill);
            mvarBuilder.CloseElement();
        }

        protected void addSquare(int xx, int yy, int side, string fill)
        {
            int mitad = side / 2;
            addRectangle(xx - mitad, yy - mitad, xx + mitad, yy + mitad, fill);
        }
        protected void addText(int xx, int yy, string? text, string fill)
        {
            if (string.IsNullOrEmpty(text)) return;
            mvarBuilder.OpenElement(30, "text");
            mvarBuilder.AddAttribute(31, "x", xScale(xx));
            mvarBuilder.AddAttribute(32, "y", yScale(yy));
            mvarBuilder.AddAttribute(33, "fill", fill);
            mvarBuilder.AddContent(34, text);
            mvarBuilder.CloseElement();
        }
        
        protected int openForeign(bool absolute,int xx, int yy, int w, int h )
        {
            mvarBuilder.OpenElement(30, "foreignObject");
            if(absolute)
            {
                mvarBuilder.AddAttribute(31, "x", xx);
                mvarBuilder.AddAttribute(32, "y", yy);
            }
            else
            {
                mvarBuilder.AddAttribute(31, "x", xScale(xx));
                mvarBuilder.AddAttribute(32, "y", yScale(yy));
            }
            mvarBuilder.AddAttribute(33, "width", w);
            mvarBuilder.AddAttribute(34, "height", h);
            return 35;
        }
        protected void closeForeign()
        {
            mvarBuilder.CloseElement();
        }

        protected void addLabel(bool absolute,int xx, int yy, int w, int h, string? text)
        {
            int num = openForeign(absolute,xx, yy, w, h);
            mvarBuilder.OpenElement(num++, "p");
            mvarBuilder.OpenElement(num++, "small");
            mvarBuilder.AddContent(num++,text);
            mvarBuilder.CloseElement();
            mvarBuilder.CloseElement();
            mvarBuilder.CloseElement();
        }
        protected virtual long xScale(int xx){ return X + xx;}
        protected virtual long yScale(int yy) { return Y + yy;}

        //Parámetros XML
        protected string? parseString(XmlNode node,string attributeName)
        {
            string? salida = null;
            salida = node.Attributes?[attributeName]?.Value ?? null;                
            return salida;            
        }
        protected bool parseBoolean(XmlNode node,string attributeName) 
        {
            bool salida = false;
            string? entrada = parseString(node, attributeName);
            if(null!=entrada)
            {
                return 
                    (
                    entrada.ToUpper().Contains("T")||
                    entrada.ToUpper().Contains("1")
                    );
            }
            return salida;
        }
        protected int parseInt(XmlNode node,string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            int salida = -1;
            if (!string.IsNullOrEmpty(entrada))
                int.TryParse(entrada, out salida);
            return salida;
        }
        protected long parseLong(XmlNode node, string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            long salida = -1;
            if(!string.IsNullOrEmpty(entrada))
                long.TryParse(entrada, out salida);
            return salida;
        }
        protected Common.Orientation parseOrientation(XmlNode node, string attributeName)
        {
            string? entrada = parseString(node, attributeName);
            if (null != entrada)
            {
                char car = entrada.ToUpper()[0];
                switch (car)
                {
                    case 'N':
                        return Common.Orientation.North;
                    case 'S':
                        return Common.Orientation.South;
                    case 'E':
                        return Common.Orientation.East;
                    case 'W':
                        return Common.Orientation.West;
                }
            }
            return Common.Orientation.North;
        }
    }

    /// <summary>
    /// Este elemento es como el normal, pero está preparado para ser serializado
    /// </summary>
    public class PortableElement
    {
        public PortableElement(byte type) 
        { 
            typ = type; 
        }
        public void setBase(long x, long y, string? name)
        {
            this.X = x;
            this.Y = y;
            this.nme = name;
        }
        public long X { get; set; } //Es la posición a representar de este elemento
        public long Y { get; set; } //Su contenido se representa relativa a la posición total.
        public string? nme { get; set; }
        public virtual byte typ { get; private set; } //Este tipo sirve para la deserialización
    }
}
