using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSqlite<GameDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new BoolArray2DConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

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
app.MapControllers();

app.Map("/", () => Results.Redirect("/swagger"));
app.MapGameEndpoints();

app.Run();