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

// Load GameSettings from appsettings.json
builder.Services.Configure<GameSettings>(builder.Configuration.GetSection("GameSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();