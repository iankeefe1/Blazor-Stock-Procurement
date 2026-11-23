using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using StockApp.Web;
using StockApp.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IPageInfoService, InMemoryPageInfoService>();
builder.Services.AddScoped<IEmployeeService, InMemoryEmployeeService>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMudServices();

await builder.Build().RunAsync();
