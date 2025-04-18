﻿using System.Text;

namespace TopacioCTC.Components
{
    /// <summary>
    /// Esto es un cliente genérico que consume un servicio HTTP rest.
    /// </summary>
    public abstract class HttpClientBase
    {
        public string controllerId { get; private set; }
        internal readonly HttpClient mvarClient;

        public HttpClientBase(HttpClient httpClient, string controllerId)
        {
            mvarClient = httpClient;
            this.controllerId = controllerId;
        }

        internal string composeUri(string command)
        {
            return string.Format("/{0}/{1}", controllerId, command);
        }

        internal string composeCommand(string command, params requestParam[] arguments)
        {
            if (0 == arguments.Length)
            {
                return composeUri(command);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                bool primera = true;
                foreach (var arg in arguments)
                {
                    if (primera)
                        sb.Append("?");
                    else
                        sb.Append("&");
                    primera = false;
                    sb.Append(arg.key);
                    sb.Append("=");
                    sb.Append(arg.value);
                }
                return string.Format("{0}{1}", composeUri(command), sb.ToString());
            }
        }
        /// <summary>
        /// Get es para recibir un envío masivo de datos con pocos parámetros.
        /// </summary>
        /// <param name="request">Cadena de petición (comando+argumentos)</param>
        /// <returns>HttpResponse</returns>
        internal async Task<HttpResponseMessage> sendGetRequest(string request)
        {
            HttpResponseMessage salida = await mvarClient.GetAsync(request);
            salida.EnsureSuccessStatusCode();
            return salida;
        }

        /// <summary>
        /// Put es para editar un elemento que ya existe en la base de datos
        /// </summary>
        /// <param name="commandId">Comando del servidor</param>
        /// <param name="jsonString">Objeto del modelo en formato json</param>
        /// <returns>HttpResponse</returns>
        internal async Task<HttpResponseMessage> sendPutRequest(string commandId, string jsonString)
        {
            HttpContent paquete = new StringContent(jsonString, Encoding.UTF8, "application/json");
            string auxCommand = composeUri(commandId);
            HttpResponseMessage salida = await mvarClient.PutAsync(auxCommand, paquete);
            salida.EnsureSuccessStatusCode();
            return salida;
        }

        /// <summary>
        /// Post es para generar un nuevo elemento que no existía en la base de datos
        /// </summary>
        /// <param name="commandId">Comando del servidor</param>
        /// <param name="jsonString">Objeto del modelo en formato json</param>
        /// <returns></returns>
        internal async Task<HttpResponseMessage> sendPostRequest(string commandId, string jsonString)
        {
            HttpContent paquete = new StringContent(jsonString, Encoding.UTF8, "application/json");
            string auxCommand = composeUri(commandId);
            HttpResponseMessage salida = await mvarClient.PostAsync(auxCommand, paquete);
            salida.EnsureSuccessStatusCode();
            return salida;
        }


        public class requestParam
        {
            public requestParam(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
            public string key { get; private set; }
            public string value { get; private set; }
        }
    }
}
