using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BootLoader.SVG
{
    /// <summary>
    /// El compilador de SVG genera un documento estático con índices que luego puede manipular
    /// el programa de CTC sin sobrecargar el lado del cliente.
    /// </summary>
    internal class SVGCompiler
    {
        
        internal LayoutSystem? Layout { get; private set; }
        internal string OutputFilename { get; set; } = MontefaroMatias.LayoutView.SVGRender.DEFAULT_FILENAME; //Nombre del archivo de salida.
        
        internal SVGCompiler(LayoutSystem? rhs) 
            { this.Layout = rhs; }
        internal string Compile()
        {
            try
            {
                if (System.IO.File.Exists(OutputFilename))
                    System.IO.File.Delete(OutputFilename); //Borro el archivo si existe.
                MontefaroMatias.LayoutView.SVGRender factory = new MontefaroMatias.LayoutView.SVGRender();
                factory.openGroup("background", true, "fill:black");
                factory.rectangle(0, 0, "100%", "100%"); //Fondo del SVG
                factory.closeGroup();

                if (null!=Layout)
                {
                    foreach (Element el in Layout.Topology.Elements)
                        el.CompileSVG(factory);
                }
                //insertLogo(factory);

                factory.Close(OutputFilename);
                return string.Empty;
            }
            catch (IOException ex)
            {
                return ex.ToString();
            }            
        }

        internal void insertLogo(MontefaroMatias.LayoutView.SVGRender ff)
        {
            ff.openGroup("footer",true,"fill:navy");
            ff.rectangle(0,0, "20%", "20%");
            ff.openGroup("ft",true, "font-size:14px; font-family:Arial; text-anchor:middle; dominant-baseline:middle; fill:white");
            ff.text(x: "50%", y: "50%", text: "MontefaroMatias LayoutView SVG Compiler - © 2025");
            ff.closeGroup();
            ff.closeGroup();
        }
    }
}


