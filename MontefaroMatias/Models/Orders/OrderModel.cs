using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Models.Orders
{
    public class OrderModel:BaseModel
    {
        public OrderModel():base()
        {
            id = string.Empty;
            or = string.Empty;
            ds = string.Empty;
            sh = false;
        }
        public string id { get; set; } //Código de la orden
        public string? gr { get; set; } //Grupo de la orden
        public string or { get; set; } //Origen del movimiento
        public string ds { get; set; } //Destino del movimiento
        public bool sh { get; set; } //El movimiento es en régimen de maniobras
    }
}
