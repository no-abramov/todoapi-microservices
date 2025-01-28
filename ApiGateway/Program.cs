using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� Ocelot
builder.Services.AddOcelot();

// ��������� ������������ Ocelot �� ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// ���������� Ocelot middleware
await app.UseOcelot();

app.Run();