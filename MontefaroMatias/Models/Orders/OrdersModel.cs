using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Models.Orders
{
    public class OrdersModel:BaseModel
    {
        public List<OrderModel> ords { get; set; }
        public OrdersModel():base()
        {
            ords = new List<OrderModel>();
        }        
    }
}
