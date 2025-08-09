using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Identity framework
builder.Services.AddIdentityCore<Gamer>()
    .AddEntityFrameworkStores<GameDbContext>();

builder.Services.AddSqlite<GameDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));

// Configure minimal API JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new BoolArray2DConverter());
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// Load GameSettings from appsettings.json
builder.Services.Configure<GameSettings>(builder.Configuration.GetSection("GameSettings"));

var app = builder.Build();

// Adds the exception handling middleware in non-Development environments.
// This middleware should be added early in the pipeline to catch exceptions from all other middleware.
if (!app.Environment.IsDevelopment())
{
    app.UseApiExceptionHandler();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGameEndpoints();
app.Map("/", () => Results.Redirect("/swagger"));

app.Run();
