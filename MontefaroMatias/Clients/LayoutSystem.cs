﻿using MontefaroMatias.LayoutView;
using MontefaroMatias.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MontefaroMatias.Clients
{
    /// <summary>
    /// Colección de elementos del enclavamiento que trabajan como sistema.
    /// </summary>
    public class LayoutSystem:ClientElement
    {
        internal List<Client> mcolClients; //Colección de clientes.
        internal Topology mvarTopology; //Topología de representación.
        internal Views mcolViews; //Colección de vistas de enclavamiento.
        public string fileName { get; set; } //Nombre del archivo.
        public string name { get;private set; } //Nombre del proyecto.
        public string version { get;private set; } //Versión del proyecto.
        public string hversion { get; private set; } //Versión del hardware (para incompatibilidades)
        public Topology Topology { get=> mvarTopology;}
        public Views Views { get => mcolViews; }
        public List<User> Users { get; private set; }
        public LayoutSystem()
        {
            mcolClients = new List<Client>();
            mvarTopology = new Topology();
            mcolViews = new Views();
            Users = new List<User>();
            fileName = string.Empty;
            name = string.Empty;
            version = "1.0";
            hversion = "1.0";
        }

        public override bool parse(XmlNode node)
        {
            if(!base.parse(node)) return false;
            name = stringParam("title");
            version = stringParam("version");
            hversion = stringParam("hversion");
            foreach (XmlNode hijo in node.ChildNodes)
            {
                if (hijo.NodeType == XmlNodeType.Element)
                {
                    if(hijo.Name.Equals("clients"))
                    {
                        if (!parseClients(hijo)) 
                            return false;
                    }
                    if(hijo.Name.Equals("users"))
                    {
                        if(!parseUsers(hijo))
                            return false;
                    }
                    else if(hijo.Name.Equals("topology"))
                    {
                        if( !mvarTopology.parse(hijo))
                            return false;
                    }
                    else if (hijo.Name.Equals("itineraries"))
                    {
                        if (!mvarTopology.parseOperations(hijo))
                            return false;
                    }
                    else if (hijo.Name.Equals("views"))
                    {
                        if(!parseViews(hijo))
                            return false;
                    }                    
                }
            }               
            return true;
        }
        protected bool parseUsers(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                User usuario = new User();
                if (!usuario.parse(child)) return false;
                Users.Add(usuario);
            }
            return true;
        }
        public User? LoginRequest(User? user)
        {
            if (user == null) return null;
            foreach (User candidato in  Users)
            {
                if(candidato.Name.Equals(user.Name))
                {
                    if(candidato.Pwd.Equals(user.Pwd))
                    {
                        User salida = new User();
                        salida.Id = candidato.Id;
                        salida.Name = candidato.Name;
                        salida.Level = candidato.Level;
                        return salida;
                    }
                }
            }
            return null;
        }

        protected bool parseClients(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                Client cliente = new Client();
                if(!cliente.parse(child)) return false;
                mcolClients.Add(cliente);
            }
            return true;
        }
        protected bool parseViews(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                View vista = new View();
                if (!vista.parse(child)) return false;
                mcolViews.Add(vista);
            }
            return true;
        }

        public override void describe(StringBuilder sb, bool detailed = false)
        {
            sb.AppendFormat("Project {0} (ver {1}), has {2} clients.\n",name,version, mcolClients.Count);
            foreach (Client cliente in mcolClients)
                cliente.describe(sb, detailed);
        }
        public override void makeTree(int indent, StringBuilder sb,ClientElement? selection)
        {
            base.makeTree(indent, sb, selection);
            indent++;
            foreach (Client client in mcolClients)
                client.makeTree(indent, sb, selection);
            indent--;
        }
        public override ClientNamedElement? search(string name)
        {
            ClientNamedElement? salida = base.search(name);
            if (null != salida) return salida;
            foreach (Client cliente in mcolClients)
            {
                salida = cliente.search(name);
                if (null != salida) return salida;
            }
            return null;
        }
    }
}
