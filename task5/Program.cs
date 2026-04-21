using task5.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<MusicGeneratorService>();
builder.Services.AddSingleton<CoverGeneratorService>();
builder.Services.AddSingleton<AudioGeneratorService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

// 🔥 обязательно после MapControllers
app.MapFallbackToFile("index.html");

app.Run();