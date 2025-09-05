using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using MontefaroMatias.Models.Elements;

namespace MontefaroMatias.Models
{
    /// <summary>
    /// Jerarquía de modelos preparados para ser serializados
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName ="$type")]
    [JsonDerivedType(typeof(LayoutModel),"pmd")]
    [JsonDerivedType(typeof(SignalModel),"psg")]
    [JsonDerivedType(typeof(LayoutUnitModel),"pci")]    
    public abstract class ElementModel:BaseModel
    {
        public string? nme { get; set; } //Nombre del elemento.
        public int id { get; set; } //Número del cliente.
    }
}
