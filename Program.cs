using Factory.DB;
using Microsoft.Extensions.DependencyInjection;
using Razor01.Global;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

var _configuration = builder.Configuration;


// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<GlobalConfig>(GlobalConfig.Instance);
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDatabaseService, DBContext>(provide=>{  
    return new DBContext(GlobalConfig.Instance.ConnectionString);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
