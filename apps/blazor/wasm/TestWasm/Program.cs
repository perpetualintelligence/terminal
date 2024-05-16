using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using OneImlx.Terminal.Apps.TestWasm.WebTerminal;

namespace OneImlx.Terminal.Apps.TestWasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddHttpClient("base", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            builder.Services.AddSingleton<TerminalWasmHostProvider>();

            builder.Services.AddFluentUIComponents();

            await builder.Build().RunAsync();
        }
    }
}
