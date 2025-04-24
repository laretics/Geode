using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml;

using System.Text.Json.Serialization;
namespace MontefaroMatias.Users
{
    public class User:BasicSerializableElement
    {
        public User()
        {
            Id = 0;
            Name = null;
            Level = 0;
            Pwd = null;
        }
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public string? Pwd { get; set; }
        public override bool parse(XmlNode node)
        {
            Id= parseInt(node, "id");
            Name = parseString(node, "name");
            Pwd = parseString(node, "pwd");
            Level = parseInt(node, "level");
            return true;
        }
    }
}
