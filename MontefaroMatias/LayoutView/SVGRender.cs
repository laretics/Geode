using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MontefaroMatias.LayoutView
{       
    public class SVGRender
    {
        public XmlDocument Document { get; private set; }
        public static string DEFAULT_FILENAME { get; } = "ctc.svg"; //Nombre por defecto del archivo de salida.
        protected int mvarIndex;
        protected int mvarOffsetX, mvarOffsetY; //Desplazamiento del elemento en el DOM, para poder dibujar en la posición correcta.
        protected Stack<XmlElement> mcolParent; //Objeto actual del que colgamos los elementos nuevos.
        protected Stack<string> mcolGroupId; //Pila de identificadores de grupos para poder cerrar los grupos correctamente.
        public void Reset()
        {
            mcolGroupId = new Stack<string>();
            mcolParent = new Stack<XmlElement>();
            mvarIndex = 0;
        }
        public SVGRender()
        {
            Document = new XmlDocument();
            mcolGroupId = new Stack<string>();
            mcolParent = new Stack<XmlElement>();
            mvarIndex = 0;
            opele("svg", "http://www.w3.org/2000/svg");
            aat("width", "100%"); //Ancho del SVG
            aat("height", "100%"); //Alto del SVG               
        }
        public void Close (string fileName)
        {
            clele();
            if (string.IsNullOrEmpty(fileName))
                fileName = DEFAULT_FILENAME;
            Document.Save(fileName);
        }
        private void aat(string key, string value)
        {
            if(mcolParent.Count>0)
                mcolParent.Peek().SetAttribute(key, value);
        }
        private void aat(string key, int value)
        {
            aat(key, value.ToString());
        }
        private void aac(string? content)
        {
            if (mcolParent.Count > 0)
                mcolParent.Peek().InnerText = content ?? string.Empty;
        }
        protected string pushId(string id)
        {
            mcolGroupId.Push(id);
            StringBuilder sb = new StringBuilder();
            foreach(string tt in mcolGroupId)
            {
                if(sb.Length > 0)
                    sb.Append("_");
                sb.Append(tt);
            }
            return sb.ToString();
        }
        protected void popId() {mcolGroupId.Pop();}
        protected void opele(string id, string? parameter = null)
        {
            XmlElement nuevo; 
            if(null==parameter)
                nuevo= Document.CreateElement(id, "http://www.w3.org/2000/svg");
            else
                nuevo = Document.CreateElement(id, parameter); 
            mcolParent.Push(nuevo);
        }
        protected void clele()
        {
            if (mcolParent.Count > 0)
            {
                XmlElement actual = mcolParent.Peek();
                mcolParent.Pop();
                if (mcolParent.Count > 0)
                    mcolParent.Peek().AppendChild(actual);
                else
                    Document.AppendChild(actual);
            }
        }
        public string openGroup(string id, bool visible=false, string? style=null)
        {
            opele("g");
            string salida = pushId(id);
            aat("id", salida);
            if(!visible)
                aat("display", "none");
            if(null!=style)
                aat("style", style);
            return salida;
        }
        public void closeGroup()
        {
            clele();
            popId();
        }
        public void setOffset(int x, int y)
        {
            mvarOffsetX = x;
            mvarOffsetY = y;
        }
        internal string openForeign(string id, int x, int y, int w, int h)
        {
            opele("foreignObject");
            string salida = pushId(id);
            aat("id", salida);
            aat("x", x + mvarOffsetX);
            aat("y", y + mvarOffsetY);
            aat("width", w);
            aat("height", h);
            return salida;
        }
        internal void closeForeign()
        {
            closeGroup();
        }
        public void circle(int xx, int yy, int r)
        {
            opele("circle");
            aat("cx", xx + mvarOffsetX);
            aat("cy", yy + mvarOffsetY);
            aat("r", r);
            clele();
        }
        public void line(int x1, int y1, int x2, int y2)
        {
            opele("line");
            aat("x1", x1 + mvarOffsetX);
            aat("y1", y1 + mvarOffsetY);
            aat("x2", x2 + mvarOffsetX);
            aat("y2", y2 + mvarOffsetY);
            clele();
        }
        public void rectangle(int x1, int y1, int x2, int y2)
        {
            opele("rect");
            aat("x", x1 + mvarOffsetX);
            aat("y", y1 + mvarOffsetY);
            aat("width", x2 - x1);
            aat("height", y2 - y1);
            clele();
        }
        public void rectangle(int x1, int y1, int x2, int y2, int rx, int ry, string style="")
        {
            opele("rect");
            aat("x", x1 + mvarOffsetX);
            aat("y", y1 + mvarOffsetY);
            aat("width", x2 - x1);
            aat("height", y2 - y1);
            aat("rx", rx);
            aat("ry", ry);
            if (style.Length > 0)
                aat("style", style);
            clele();
        }
        public void rectangle (int x, int y, string width, string height)
        {
            opele("rect");
            aat("x", x + mvarOffsetX);
            aat("y", y + mvarOffsetY);
            aat("width", width);
            aat("height", height);
            clele();
        }
        public void square(int x, int y, int side)
        {
            int mitad = side / 2;
            rectangle(x - mitad, y - mitad, x + mitad, y + mitad);
        }
        public void text(int x, int y, string? text)
        {
            opele("text");
            aat("x", x + mvarOffsetX);
            aat("y", y + mvarOffsetY);
            aac(text);
            clele();
        }
        public void text(string x,string y, string? text)
        {
            opele("text");
            aat("x", x);
            aat("y", y);
            aac(text);
            clele();
        }
        public void label(int x, int y, int w, int h, string color, string? caption)
        {
            opele("text");
            aat("x", x + mvarOffsetX + (w / 2));
            aat("y", y + mvarOffsetY + (h / 2));
            aat("style", string.Format("fill:{0};font-family:Arial;text-anchor:middle;dominant-baseline:middle;font-size:9px", color));
            aac(caption);
            clele();
        }
        public void bagde(int x, int y, int w, int h, string color, string bgColor, string? caption)
        {
            if (null != caption)
            {
                opele("rect");
                aat("x", x + mvarOffsetX);
                aat("y", y + mvarOffsetY);
                aat("width", w);
                aat("height", h);
                aat("rx", 5);
                aat("ry", 5);
                aat("style", string.Format("fill:{0};stroke:transparent;",bgColor));
                clele();
                opele("text");
                aat("x", x + mvarOffsetX+(w/2));
                aat("y", y + mvarOffsetY+(h/2));
                aat("style", string.Format("fill:{0};font-family:Arial;text-anchor:middle;dominant-baseline:middle;font-size:9px",color));
                aac(caption);
                clele();
            }
        }
    }
}


/*
 *    <!-- Pie de página -->
    <g id="footer" transform="translate(0, 95%)">
        <!-- Fondo del pie de página -->
        <rect x="0" y="0" width="100%" height="5%" fill="black" />
        <!-- Texto del pie de página -->
        <text x="50%" y="50%" fill="white" font-size="14" font-family="Arial" text-anchor="middle" dominant-baseline="middle">
            Producido por Montefaro Matías - © 2023
        </text>
    </g>
 * */