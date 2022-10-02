using StackExchange.Redis;
using Core.Entities.Identity;


var builder = WebApplication.CreateBuilder(args);

// add services to the container

builder.Services.AddAutoMapper(typeof(MappingProfiles));

builder.Services.AddControllers();

builder.Services.AddDbContext<StoreContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppIdentityDbContext>(x =>
{
    x.UseNpgsql(builder.Configuration.GetConnectionString("IdentityConnection"));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
    });
});

// configure the http request pipeline

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwaggerDocumentation();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

// serves anything inside the WWWRoot folder
app.UseStaticFiles();
// but now we've moved our images outside of that folder. We need to tell it about another location.
// and because our angular applications are going to be served from the WWWRoot folder, we'll need to
// add another use static files, middleware, but point it to a different location.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Content")
    ),
    RequestPath = "/content"
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
// because we're going to be serving our Angular content from our API, then we need to tell our API about the endpoints.
// At the moment is got endpoints for our controllers, but we also need to tell it about an end point
// for Angular, because when a request comes to our index.html, then that's the point that Angular routine takes over.
// And we don't want our API server to be attempting to find roots for our angular content.
// We only want it to be concerned about roots for our actual API.
// So what we can do to account for that is we can add another endpoints.
// MapFallbackToController is intended to handle cases where URL path of the request does not contain a file name, and no other endpoint has matched. This is convenient for routing requests for dynamic content to a SPA framework, while also allowing requests for non-existent files to result in an HTTP 404.
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
try
{
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context, loggerFactory);

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
    await identityContext.Database.MigrateAsync();
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred during migration");
}

await app.RunAsync();
