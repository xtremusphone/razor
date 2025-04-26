using Factory;
using Factory.DB;
using Razor01.Global;
using static Factory.DB.DBContext;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<GlobalConfig>(GlobalConfig.Instance);
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddRazorComponents();
builder.Services.AddScoped<IDatabaseService, DBContext>(provide=>{  
    return new DBContext(GlobalConfig.Instance.ConnectionString, GlobalConfig.Instance.DBType.ToEnum<DBType>());
});
builder.Services.AddDistributedRedisCache(options => {
    options.Configuration = GlobalConfig.Instance.RedisServerName;
    options.InstanceName = "ASPSession_";
}
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.UseSession();
app.MapRazorPages().WithStaticAssets();

app.Run();
