using MontefaroMatias.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MontefaroMatias.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName ="$type")]
    [JsonDerivedType(typeof (ElementModel),"pelm")]
    [JsonDerivedType(typeof(OrderModel),"pord")]
    [JsonDerivedType(typeof(OrdersModel),"pords")]
    public abstract class BaseModel
    {
    }
}
