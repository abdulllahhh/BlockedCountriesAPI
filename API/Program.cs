using Business.Interfaces;
using Infrastructure.Services;
using Infrastructure.Services.GeoProviders;
using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();

            // Dependency Injection
            builder.Services.AddSingleton<IBlockedCountriesStore, BlockedCountriesStore>();
            builder.Services.AddSingleton<IRequestLogStore, RequestLogStore>();
            builder.Services.AddHttpClient<IGeoProvider, IpapiProvider>();
            
            builder.Services.AddHostedService<TemporaryBlockCleanupService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            // 1. Forwarded Headers (MUST be first)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (app.Environment.IsDevelopment())
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

