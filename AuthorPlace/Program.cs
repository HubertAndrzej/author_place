using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Implementations;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Host.UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration));
builder.Services.AddMvc(options =>
{
    CacheProfile cacheProfile = new CacheProfile();
    builder.Configuration.Bind("ResponseCache:Home", cacheProfile);
    options.CacheProfiles.Add("Home", cacheProfile);
});
builder.Services.AddScoped<IAlbumService, EFCoreAlbumService>();
builder.Services.AddDbContextPool<AuthorPlaceDbContext>(optionsBuilder => optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("Default")!));
builder.Services.AddSingleton<IErrorViewSelectorService, ErrorViewSelectorService>();
builder.Services.AddTransient<ICachedAlbumService, MemoryCacheAlbumService>();
builder.Services.AddResponseCaching();
builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<AlbumsOptions>(builder.Configuration.GetSection("Albums"));
builder.Services.Configure<MemoryCacheOptions>(builder.Configuration.GetSection("MemoryCache"));

WebApplication app = builder.Build();
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error");
app.UseStaticFiles();
app.UseResponseCaching();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
