using Microsoft.JSInterop;

namespace TopacioCTC.Authentication
{
    public class AuthService
    {
        protected static IJSRuntime mvarJSRuntime;
        public AuthService(IJSRuntime runtime)
        {
            mvarJSRuntime = runtime;
        }
        private readonly Dictionary<string, string> mcolUsers = new Dictionary<string, string>
        {
            {"root" ,"qwer" },  //Administrador
            { "ctc", "matias699" }, //Permiso de operación
            { "guest", "guest" } //Sin permisos... sólo puede ver
        };

        public bool Authenticate(string username, string password)
        {
            return mcolUsers.TryGetValue(username, out var storedPassword) && storedPassword == password;
        }

        public static async Task<bool> isSessionOpen()
        {
            return await GetToken() > DateTime.Now;
            //La sesión se considera abierta mientras el valor de "token" sea superior a la fecha y hora actuales.
        }

        private static async Task<DateTime> GetToken()
        {
            DateTime salida = DateTime.MinValue;
            string? valor = await GetDataFromSession("token");
            if(null!=valor)
            {
                DateTime.TryParse(valor, out salida);
            }
            return salida;
        }
        public static async Task<string?> GetUserName()
        {
            return await GetDataFromSession("userName");
        }
        public static async Task<bool> SetUserName(string username)
        {
            return await SetDataToSession("userName", username);
        }
        public static async Task<bool> SetToken()
        {
            DateTime horaFin = DateTime.Now.AddHours(4);
            string auxTexto = horaFin.ToString();
            return await SetDataToSession("token", auxTexto);
        }
        public static async Task<bool> Logout()
        {
            await mvarJSRuntime.InvokeVoidAsync("localStorage.removeItem", "token");
            await mvarJSRuntime.InvokeVoidAsync("localStorage.removeItem", "userName");
            return true;
        }
        private static async Task<string?> GetDataFromSession(string key)
        {
            string? salida = await mvarJSRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            return salida;
        }
        private static async Task<bool> SetDataToSession(string key, string value)
        {
            await mvarJSRuntime.InvokeVoidAsync("localStorage.setItem",key,value);
            return true;
        }
    }
}
