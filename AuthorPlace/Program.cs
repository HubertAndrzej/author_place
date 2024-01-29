using AuthorPlace.Models.Services.Application.Implementations;
using AuthorPlace.Models.Services.Application.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();
builder.Services.AddScoped<IAlbumService, AlbumService>();

WebApplication app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
