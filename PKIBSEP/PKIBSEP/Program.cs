using DotNetEnv;
using PKIBSEP.Common;
using PKIBSEP.Middlewares;
using PKIBSEP.Startup;

Env.Load(".env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.ConfigureBSEP(builder.Configuration);
builder.Services.AddDataProtection(); // protects CaUserKey.ProtectedWrapKey
var keystoreFolder = builder.Configuration["Keystore:Folder"];
Directory.CreateDirectory(keystoreFolder!);
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("_allowDevClients");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SessionMiddleware>();

app.MapControllers();

app.Run();
