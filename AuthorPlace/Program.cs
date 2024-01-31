using AuthorPlace.Models.Services.Application.Implementations;
using AuthorPlace.Models.Services.Application.Interfaces;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();
builder.Services.AddScoped<IAlbumService, AdoNetAlbumService>();
builder.Services.AddScoped<IDatabaseAccessor, SqliteDatabaseAccessor>();

WebApplication app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
