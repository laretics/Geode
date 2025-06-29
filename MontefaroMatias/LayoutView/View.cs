using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MontefaroMatias.LayoutView.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.LayoutView
{
    public class View : BasicSerializableElement
    {
        public View()
        {
            Name = null;
            Id = -1;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Comment = null;
            Scale = 1;
            IconIndex = 0;
        }
        public int Id { get; set; } //Identificación de la vista.
        public string? Name { get; set; } //Nombre de la vista.
        public long X { get; set; } //Posición en el eje X.
        public long Y { get; set; } //Posición en el eje Y.
        public long Width { get; set; } //Ancho de la vista.
        public long Height { get; set; } //Alto de la vista.
        public string? Comment { get; set; } //Descripción de la vista.
        public float Scale { get; set; } //Escala de la vista.
        public byte IconIndex { get; set; } //Índice del icono de la vista.
        public override bool parse(XmlNode node)
        {
            Name = parseString(node, "name");
            Id = parseInt(node, "id");
            X = parseLong(node, "x");
            Y = parseLong(node, "y");
            Scale = ((float)parseInt(node, "scale"))/100;
            Width = parseLong(node, "w");
            Height = parseLong(node, "h");
            Comment = parseString(node, "comment");
            IconIndex = (byte)parseInt(node, "icon");
            return true;
        }
    }
    public class Views : List<View>
    {
        public Views() : base()
        {
        }
        public View? view(int index)
        {
            foreach (View v in this)
            {
                if (v.Id == index)
                    return v;
            }
            return null;
        }
    }
}
