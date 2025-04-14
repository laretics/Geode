using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MontefaroMatias.Compiler;

namespace MontefaroMatias.Clients
{
    /// <summary>
    /// Esto es un elemento que contiene un cliente.
    /// De momento puede ser una señal, un circuito de vía o un conjunto de motores de desvíos.
    /// </summary>  
    public abstract class ClientElement
    {
        internal const byte NOT_ASSIGNED = 255;
        #region Source Code
        public virtual bool parse(XmlNode node) //Obtiene los datos desde el archivo XML.
        {
            mvarNode = node;
            return true; //Por defecto el parseo es correcto.
        }
        public abstract void describe(StringBuilder sb, bool detailed = false); //Descripción de este elemento en un archivo de texto
        public virtual ClientNamedElement? search(string name) //Busca un elemento por su nombre de identificador.
        { return null; }
        public virtual void makeTree(int indent, StringBuilder sb, ClientElement? selection) { }
        public string tree (ClientElement? selection)
        {
            StringBuilder sb = new StringBuilder();
            makeTree(0, sb,selection);
            return sb.ToString();
        }
        public string description
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                this.describe(sb, false);
                return sb.ToString();
            }
        }
        public string detailed
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                this.describe(sb, true);
                return sb.ToString();
            }
        }
        #endregion Source Code

        protected virtual void pp(string rhs)
        {
            Console.WriteLine(rhs);
        }

        #region Object Code
        internal UInt16 address { get; set; } //Dirección de comienzo de este elemento.
        internal virtual UInt16 size { get => 0; } //Longitud del objeto en función en bytes.
        public virtual MemoDump code { get => new MemoDump(); }//Código compilado (se usará para llenar la EEPROM)
        #endregion Object Code

        #region XML
        protected XmlNode? mvarNode;
        protected string stringParam(string paramName)
        {
            if (null== mvarNode || null == mvarNode.Attributes) return string.Empty;
            foreach (XmlAttribute atributo in mvarNode.Attributes)
            {
                if (atributo.Name.Equals(paramName))
                    return atributo.Value.ToString();
            }
            return string.Empty;
        }
        protected byte byteParam(string paramName)
        {
            string auxValor = stringParam(paramName);
            byte salida = 255;
            if (byte.TryParse(auxValor, out salida))
                return salida;
            return 255;
        }
        protected int intParam(string paramName)
        {
            string auxValor = stringParam(paramName);
            int salida = 255;
            int.TryParse(auxValor, out salida);
            return salida;
        }
        protected UInt16 uintParam(string paramName)
        {
            string auxValor = stringParam(paramName);
            UInt16 salida = 0;
            UInt16.TryParse(auxValor, out salida);
            return salida;
        }
        protected bool boolParam(string paramName)
        {
            string auxValor = stringParam(paramName);
            bool salida = false;
            bool.TryParse(auxValor, out salida);
            return salida;
        }
        protected TimeSpan timestampParam(string paramName)
        {            
            string auxValor = stringParam(paramName);
            int segundos = 0;
            if(int.TryParse(auxValor, out segundos))
                return new TimeSpan(0,0,segundos);           

            DateTime salida = DateTime.MinValue;
            if (DateTime.TryParse(auxValor, out salida))
                return salida.TimeOfDay;
            else
                return TimeSpan.Zero;
        }
        #endregion
    }
    public abstract class ClientNamedElement:ClientElement
    {
        public string? name { get; set; } //Identificador mnemónico de este elemento.
        public byte id { get; set; } //Número del elemento en la descripción general
        protected override void pp(string rhs)
        {
            base.pp(string.Format("Client {0} (id{1}): {2}",name,id,rhs));
        }
        public override bool parse(XmlNode node)
        {
            if (!base.parse(node)) 
                return false;

            this.id = byteParam("id");
            this.name = stringParam("name");
            return true;
        }
        public override ClientNamedElement? search(string name)
        {
            if(null==this.name) return null;
            if (this.name.Contains(name)) return this;
            return null;
        }
        public override void makeTree(int indent, StringBuilder sb, ClientElement? selection)
        {
            for (int i = 0; i < indent; i++) sb.Append("\t"); //Tabulaciones
            if(selection==this)                
                sb.AppendFormat("[{0}]\n",this.name); //Nombre del elemento con selección.
            else
                sb.AppendFormat(" {0} \n", this.name); //Nombre del elemento sin selección.
        }
        public override MemoDump code
        {
            get
            {
                MemoDump salida = base.code;
                salida.add(id, string.Format("Device {0}", name), "Id");
                return salida;
            }
        }
        internal override ushort size => (ushort)(base.size + 1);

    }
    //internal abstract class ClientAllocatedElement : ClientNamedElement
    //{
    //    protected Client parent { get; private set; }

    //    internal bool parse(XmlNode node, Client parent)
    //    {
    //        this.parent = parent;
    //        return parse(node);
    //    }
    //}

}
