using WebApi.Services;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddControllers();//реЇстрац≥€ серв≥с≥в дл€ контролер≥в та swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            
            builder.Services.AddScoped<AnimeSer>();//реЇстрац≥€ серв≥с≥в (addscoped створюЇ екземпл€р дл€ кожного запиту)
            builder.Services.AddScoped<MangaSer>();
            builder.Services.AddScoped<FavorSer>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())//налаштуванн€ черговост≥ обробки запит≥в 
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers(); 

            app.Run();
        }
    }
}