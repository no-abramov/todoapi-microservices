using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку Ocelot
builder.Services.AddOcelot();

// Загружаем конфигурацию Ocelot из ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// Подключаем Ocelot middleware
await app.UseOcelot();

app.Run(); 