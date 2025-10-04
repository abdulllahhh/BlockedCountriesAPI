
using Business.Interfaces;
using Infrastructure.Services;
using Infrastructure.Services.GeoProviders;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IBlockedCountriesStore, BlockedCountriesStore>();
            builder.Services.AddHttpClient<IGeoProvider, IpapiProvider>();
            builder.Services.AddSingleton<IRequestLogStore, RequestLogStore>();
            builder.Services.AddHostedService<TemporaryBlockCleanupService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(); // ? Swagger



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
