using Microsoft.JSInterop;
using MontefaroMatias;
using MontefaroMatias.Users;
using System.Text.Json;
using System.Threading.Tasks;
using TopacioCTC.Components;

namespace TopacioCTC.Authentication
{
    public class TopacioAuthService
    {
        protected static IJSRuntime mvarJSRuntime;
        protected IServiceProvider mvarProvider;

        public TopacioAuthService(IServiceProvider serviceProvider)
        {
            mvarProvider = serviceProvider;
        }

        public void setJSRuntime(IJSRuntime runtime)
        {
            mvarJSRuntime = runtime;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            User container = new User();
            container.Name = username;
            container.Pwd = password;
            TopacioClient topacioClient = mvarProvider.GetRequiredService<TopacioClient>();
            User? auxUser = await topacioClient.tryLogin(container);
            await SetCurrentUser(auxUser);
            return auxUser != null;
        }

        public static async Task<User?> GetCurrentUser()
        {
            string? cadena = await GetDataFromSession("currentsession");
            if(null==cadena) return null;
            User? salida = JsonSerializer.Deserialize(cadena, SharedSerializeContext.Default.User);
            return salida;
        }
        public async static Task SetCurrentUser(User? rhs)
        {
            string cadena = JsonSerializer.Serialize(rhs,SharedSerializeContext.Default.User);
            await SetDataToSession("currentsession", cadena);
        }

        public static async Task<bool> Logout()
        {
            await SetCurrentUser(null);
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
