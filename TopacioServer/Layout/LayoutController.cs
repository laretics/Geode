using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MontefaroMatias.Clients;
using MontefaroMatias.LayoutView;
using MontefaroMatias.Locking;
using MontefaroMatias;
using MontefaroMatias.XML;
using TopacioServer.Components;
using MontefaroMatias.Users;
using System.Text;
using MontefaroMatias.Models.Elements;
using MontefaroMatias.Models.Orders;

namespace TopacioServer.Layout
{
    [ApiController]
    [Route("api/layout")]
    public class LayoutController : ControllerBase
    {
        private readonly Kernel mvarKernel;

        public LayoutController(Kernel kernel) {mvarKernel = kernel;}

        // GET: api/layout/topo
        [HttpGet("topo")]
        //Posición actual del enclavamiento:
        //Devuelve la indicación de las señales,
        //el estado de los pasos a nivel y
        //el estado y configuración de los circuitos de vía.
        public ActionResult<LayoutModel> GetTopology()
        {
            var salida = mvarKernel.mainSystem.Topology.UDPSnapshot();
            return Ok(salida);
        }

        // GET: api/layout/views
        // Lista de vistas parciales del enclavamiento.
        [HttpGet("views")]
        public ActionResult<string> GetViews()
        {
            var vistas = mvarKernel.mainSystem.Views;
            string cadenaJson = JsonSerializer.Serialize(vistas, SharedSerializeContext.Default.Views);
            return Ok(cadenaJson);
        }

        // GET: api/layout/conf
        // Configuración actual del kernel, para consulta en un cliente.
        [HttpGet("conf")]
        public ActionResult<string> GetConfiguration()
        {
            string salida = string.Format("{0},{1},{2},{3}",
                mvarKernel.FileName, mvarKernel.UdpPort, mvarKernel.UdpDestination, mvarKernel.UdpEnabled);
            return Ok(salida);
        }

        // PUT: api/layout/user
        // Recupera el usuario que está intentando abrir sesión
        [HttpPut("login")]
        public async Task<ActionResult<string?>> Login()
        {
            using var reader = new StreamReader(Request.Body);
            string? userString = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(userString)) return BadRequest();

            try
            {
                User? auxUser = JsonSerializer.Deserialize(userString, SharedSerializeContext.Default.User);
                User? auxRespuesta = mvarKernel.mainSystem.LoginRequest(auxUser);
                string salida = JsonSerializer.Serialize(auxRespuesta, SharedSerializeContext.Default.User);
                return Ok(salida);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializando el objeto User: {ex.Message}");
                return StatusCode(500, "Error deserializando el objeto User");
            }
        }

        // POST: api/layout/cmd
        // Intenta ejecutar un comando de texto en el servidor
        [HttpPost("cmd")]
        public async Task<IActionResult> ProcessTextOrder()
        {
            using var reader = new StreamReader(Request.Body);
            string orderText = await reader.ReadToEndAsync();
            string salida = mvarKernel.mainSystem.Topology.ExecuteOperation(orderText);
            return Ok(salida);
        }

        // POST: api/layout/lcmd
        [HttpPost("lcmd")]
        public async Task<IActionResult> ProcessLayoutOrder()
        {
            try
            {
                using StreamReader lector = new StreamReader(Request.Body);
                string orderText = await lector.ReadToEndAsync();
                OrdersModel? modelo = JsonSerializer.Deserialize<OrdersModel>(orderText);
                return Ok(mvarKernel.mainSystem.Topology.ExecuteOperations(modelo));
            }
            catch (Exception e)
            {
                return Ok(-2); //Error de deserialización.
            }            
        }
        /// <summary>
        /// Lista de códigos de error:
        /// 0   : OK
        /// -1  : Error inesperado
        /// -2  : Error de comunicación cliente/servidor
        /// 
        /// </summary>        


        // POST: api/layout/occ
        [HttpPost("occ")]
        public async Task<IActionResult> ProcessOccupancy()
        {
            using var reader = new StreamReader(Request.Body);
            string orderId = await reader.ReadToEndAsync();
            mvarKernel.mainSystem.Topology.ExecuteOccupancy(orderId);
            return Ok();
        }

    }
}