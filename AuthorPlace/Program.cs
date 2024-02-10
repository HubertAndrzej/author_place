using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Implementations;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using Microsoft.EntityFrameworkCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Host.UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration));
builder.Services.AddMvc();
builder.Services.AddScoped<IAlbumService, EFCoreAlbumService>();
builder.Services.AddDbContextPool<AuthorPlaceDbContext>(optionsBuilder => optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("Default")!));
builder.Services.AddSingleton<IErrorViewSelectorService, ErrorViewSelectorService>();
builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<AlbumsOptions>(builder.Configuration.GetSection("Albums"));

WebApplication app = builder.Build();
app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapFallbackToController("{*path}", "Index", "Error");
app.Run();
