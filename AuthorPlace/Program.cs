using AspNetCore.ReCaptcha;
using AuthorPlace.Customizations.Authorizations;
using AuthorPlace.Customizations.Claims;
using AuthorPlace.Customizations.ModelBinders;
using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Enums;
using AuthorPlace.Models.Options;
using AuthorPlace.Models.Services.Application.Implementations.Albums;
using AuthorPlace.Models.Services.Application.Implementations.Errors;
using AuthorPlace.Models.Services.Application.Implementations.Songs;
using AuthorPlace.Models.Services.Application.Interfaces.Albums;
using AuthorPlace.Models.Services.Application.Interfaces.Errors;
using AuthorPlace.Models.Services.Application.Interfaces.Songs;
using AuthorPlace.Models.Services.Infrastructure.Implementations;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using AuthorPlace.Models.Validators.Albums;
using AuthorPlace.Models.Validators.Identity;
using AuthorPlace.Models.Validators.Songs;
using AuthorPlace.Models.Validators.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

Persistence persistence = Persistence.EFCore;
PaymentType paymentType = PaymentType.Stripe;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddUserSecrets("secrets.json");
builder.Host.UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration));
builder.Services.AddMvc(options =>
{
    CacheProfile cacheProfile = new();
    builder.Configuration.Bind("ResponseCache:Home", cacheProfile);
    options.CacheProfiles.Add("Home", cacheProfile);
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Privacy");
});
builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<AlbumCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AlbumUpdateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AlbumDeleteValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AlbumSubscribeValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AlbumVoteValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SongCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SongUpdateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SongDeleteValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserRoleValidator>();
builder.Services.AddFluentValidationClientsideAdapters(clientSide => clientSide.Add(typeof(IRemotePropertyValidator), (context, description, validator) => new RemoteClientValidator(description, validator)));
IdentityBuilder? identityBuilder = builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 4;
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
    .AddPasswordValidator<CommonPasswordValidator<ApplicationUser>>();
IdentityBuilder? identityStore = persistence switch
{
    Persistence.AdoNet => identityBuilder.AddUserStore<AdoNetUserStore>(),
    Persistence.EFCore => identityBuilder.AddEntityFrameworkStores<AuthorPlaceDbContext>(),
    _ => identityBuilder.AddEntityFrameworkStores<AuthorPlaceDbContext>(),
};
IServiceCollection? albumService = persistence switch
{
    Persistence.AdoNet => builder.Services.AddTransient<IAlbumService, AdoNetAlbumService>(),
    Persistence.EFCore => builder.Services.AddTransient<IAlbumService, EFCoreAlbumService>(),
    _ => builder.Services.AddScoped<IAlbumService, EFCoreAlbumService>()
};
IServiceCollection? songService = persistence switch
{
    Persistence.AdoNet => builder.Services.AddTransient<ISongService, AdoNetSongService>(),
    Persistence.EFCore => builder.Services.AddTransient<ISongService, EFCoreSongService>(),
    _ => builder.Services.AddScoped<ISongService, EFCoreSongService>()
};
IServiceCollection? paymentGateway = paymentType switch
{
    PaymentType.PayPal => builder.Services.AddScoped<IPaymentGateway, PayPalPaymentGateway>(),
    PaymentType.Stripe => builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>(),
    _ => builder.Services.AddTransient<IPaymentGateway, StripePaymentGateway>()
};
builder.Services.AddScoped<IDatabaseAccessor, SqliteDatabaseAccessor>();
builder.Services.AddScoped<IAuthorizationHandler, AlbumAuthorRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AlbumSubscriberRequirementHandler>();
builder.Services.AddDbContextPool<AuthorPlaceDbContext>(optionsBuilder => optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("Default")!));
builder.Services.AddSingleton<IErrorViewSelectorService, ErrorViewSelectorService>();
builder.Services.AddSingleton<IImagePersister, MagickNetImagePersister>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
builder.Services.AddSingleton<IEmailClient, MailKitEmailSender>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, MultiAuthorizationPolicyProvider>();
builder.Services.AddTransient<ICachedAlbumService, MemoryCacheAlbumService>();
builder.Services.AddTransient<ICachedSongService, MemoryCacheSongService>();
builder.Services.AddResponseCaching();
builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<AlbumsOptions>(builder.Configuration.GetSection("Albums"));
builder.Services.Configure<MemoryCacheOptions>(builder.Configuration.GetSection("MemoryCache"));
builder.Services.Configure<CacheDurationOptions>(builder.Configuration.GetSection("CacheDuration"));
builder.Services.Configure<ImageSizeOptions>(builder.Configuration.GetSection("ImageSize"));
builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection("Kestrel"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection("Users"));
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(nameof(Policy.AlbumAuthor), builder =>
    {
        builder.Requirements.Add(new AlbumAuthorRequirement());
    });
    options.AddPolicy(nameof(Policy.AlbumSubscriber), builder =>
    {
        builder.Requirements.Add(new AlbumSubscriberRequirement());
    });
});
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

WebApplication app = builder.Build();
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.UseEndpoints(routeBuilder =>
{
    routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
    routeBuilder.MapRazorPages().RequireAuthorization();
});
app.UseHttpsRedirection();
app.Run();
