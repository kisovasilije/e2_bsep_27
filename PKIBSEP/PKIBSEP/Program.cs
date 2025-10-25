using PKIBSEP.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services.ConfigureBSEP(builder.Configuration);
builder.Services.ConfigureCors(builder.Configuration);

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

app.MapControllers();

app.Run();
