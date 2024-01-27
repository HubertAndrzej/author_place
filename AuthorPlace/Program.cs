WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();

WebApplication app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
