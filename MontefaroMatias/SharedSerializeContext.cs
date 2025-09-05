using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MontefaroMatias
{
    [JsonSerializable(typeof(User))]
    [JsonSerializable(typeof(Views))]
    [JsonSerializable(typeof(Topology))]    
    public partial class SharedSerializeContext:JsonSerializerContext
    {
    }
}
