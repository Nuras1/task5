using task5.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<CoverGeneratorService>(); 
builder.Services.AddSingleton<MusicGeneratorService>();
builder.Services.AddSingleton<AudioGeneratorService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();