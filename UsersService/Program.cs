using Asp.Versioning;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using RabbitMQ.Client;

using System.Reflection;
using System.Text;

using UsersService.Data;

var builder = WebApplication.CreateBuilder(args);

// Регистрация поддержки кодировки для корректного отображения символов
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

// Подключение к базе данных через Entity Framework
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// Настройка версионирования API
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Формат именования версий API
    options.SubstituteApiVersionInUrl = true; // Подстановка версии API в URL
});

// Конфигурация подключения к RabbitMQ
var factory = new ConnectionFactory
{
    HostName = "rabbitmq",
    UserName = "guest",
    Password = "guest"
};

// Создание подключения и канала RabbitMQ
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

// Регистрация зависимостей RabbitMQ в контейнере DI
builder.Services.AddSingleton(connection);
builder.Services.AddSingleton(channel);

// Регистрация контроллеров
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Регистрация HttpClient для внешних запросов
builder.Services.AddHttpClient();

// Настройка Swagger для документирования API
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users Service API",
        Version = "v1",
        Description = "API для управления пользователями.",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@example.com"
        }
    });
});

var app = builder.Build();

// Настройка обработки ошибок
app.UseExceptionHandler("/error"); // Обработчик ошибок

// Подключение Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users Service API V1"));

// Настройка маршрутов контроллеров
app.MapControllers();

// Запуск приложения
app.Run();