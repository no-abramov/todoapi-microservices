using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using System.Reflection;
using System.Text;
using TasksService.Data;

using Asp.Versioning;
using TasksService.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Регистрация поддержки кодировки для корректного отображения символов
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

// Подключение к локальной базе данных через Entity Framework
builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// Настройка версий API
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Формат именования групп версий API
    options.SubstituteApiVersionInUrl = true; // Подставлять версию API в URL
});

// Конфигурация подключения к RabbitMQ
var factory = new ConnectionFactory
{
    HostName = "rabbitmq", // Имя хоста RabbitMQ (из docker-compose.yml)
    UserName = "guest",
    Password = "guest"
};

// Регистрация подключения к RabbitMQ как Singleton
builder.Services.AddSingleton<IConnection>(sp => factory.CreateConnection());

// Регистрация RabbitMQ Consumer как фоновую службу
builder.Services.AddHostedService<RabbitMqUserConsumer>();

// Настройка контроллеров
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Настройка Swagger для документирования API
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tasks Service API",
        Version = "v1",
        Description = "API для управления задачами.",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@example.com"
        }
    });
});

var app = builder.Build();

// Настройка обработки ошибок
app.UseExceptionHandler("/error");

// Подключение Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks Service API V1"));

// Настройка маршрутов контроллеров
app.MapControllers();

// Запуск приложения
app.Run();