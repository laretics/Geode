using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MontefaroMatias.Clients;
using System.Xml;
using MontefaroMatias.LayoutView;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2;

namespace MontefaroMatias.XML
{
    public class XMLImporter
    {
        protected XmlDocument mvarDocument; //Contenedor de la documentación XML del circuito.
        public XMLImporter()
        {
            mvarDocument = new XmlDocument();
        }
        public bool loadScheme(string fileName)
        {
            mvarDocument = new XmlDocument(); //Elimino cualquier estructura precedente.
            string auxFile = SearchFile(fileName, "xml");
            if (string.Empty == fileName) return false;
            try
            {
                mvarDocument.Load(auxFile);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public bool loadSchemeFromResource(string fileName)
        {
            mvarDocument = new XmlDocument();
            if (string.Empty == fileName) return false;
            try
            {
                mvarDocument.Load(string.Format("/wwwroot/data/{0}", fileName));
                return true;
            }
            catch (Exception ex) { return false; }
        }

        internal string SearchFile(string fileName, string defaultExtension)
        {
            if (!fileName.Contains("."))
            {
                fileName += "." + defaultExtension;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(AppContext.BaseDirectory);
            return SearchFileRecursive(directoryInfo, fileName);
        }

        private string SearchFileRecursive(DirectoryInfo directoryInfo, string fileName)
        {
            // Busca el archivo en el directorio actual
            FileInfo[] files = directoryInfo.GetFiles(fileName);
            if (files.Length > 0)
            {
                return files[0].FullName;
            }

            // Si no se encuentra y no estamos en el directorio raíz, busca en el directorio padre
            DirectoryInfo? parentDirectory = directoryInfo.Parent;
            if (parentDirectory != null)
            {
                return SearchFileRecursive(parentDirectory, fileName);
            }

            // Si llegamos al directorio raíz y no se encuentra el archivo, devuelve una cadena vacía
            return string.Empty;
        }


        private string searchFile(DirectoryInfo directoryInfo, string fileName)
        {
            if (!directoryInfo.Exists) return string.Empty;
            string auxArchivo = Path.Combine(directoryInfo.FullName, fileName);
            if (File.Exists(auxArchivo))
                return auxArchivo;
            foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
            {
                string auxBusca = searchFile(dirInfo, fileName);
                if (string.Empty != auxBusca) return auxBusca;
            }
            return string.Empty;
        }
        /// <summary>
        /// Una vez cargado el esquema desde XML lo deserializa
        /// </summary>
        /// <returns>Objeto LayoutSystem con la información deserializada</returns>
        public LayoutSystem? getSystem()
        {
            if (null != mvarDocument)
            {
                foreach (XmlNode node in mvarDocument.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element && node.Name.Equals("mtfComm"))
                    {
                        LayoutSystem salida = new LayoutSystem();
                        if (salida.parse(node)) return salida;
                    }
                }
            }
            return null;
        }
    }
}
