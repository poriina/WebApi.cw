using Telegram.Bot;
using WebApi.BotHandlers;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AnimeSer>();
builder.Services.AddScoped<MangaSer>();
builder.Services.AddScoped<FavorDbService>();


builder.Services.AddSingleton<AnimeHandler>();

var botToken = builder.Configuration.GetValue<string>("TelegramBot:Token");
var botClient = new TelegramBotClient(botToken);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


var handler = app.Services.GetRequiredService<AnimeHandler>();
var commands = new[]
{
    new Telegram.Bot.Types.BotCommand { Command = "start", Description = "Відкрити головне меню" }
};
botClient.SetMyCommands(commands).Wait();
botClient.StartReceiving(
    updateHandler: (client, update, ct) => handler.HandleUpdateAsync(client, update, app.Services),
    errorHandler: handler.HandlePollingErrorAsync 
);

Console.WriteLine("Бот запущений на чистому Telegram.Bot!");

app.Run();