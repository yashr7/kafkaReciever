// Application entry point 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using XoneExecutionAck.Configuration;
using XoneExecutionAck.Database;
using XoneExecutionAck.Database.CompiledModels;
using XoneExecutionAck.Database.Repositories;
using XoneExecutionAck.Kafka.Consumers;
using XoneExecutionAck.Kafka.Serialization;
using XoneExecutionAck.Processing;

namespace XoneExecutionAck
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register configuration
                    services.Configure<AppConfig>(hostContext.Configuration);

                    // Set up database
                    var connectionString = hostContext.Configuration.GetSection("Database:ConnectionString").Value;

                    // Add DbContext as a scoped service
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString);

                        // Always use the compiled model to ensure compatibility with Native AOT
                        options.UseModel(AppDbContextModel.GetModel());
                    });

                    // Add DbContextFactory as a singleton service
                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString);

                        // Always use the compiled model to ensure compatibility with Native AOT
                        options.UseModel(AppDbContextModel.GetModel());
                    });

                    // Register repositories as singletons to match ExecutionProcessor's lifetime
                    services.AddSingleton<IExecutionRepository, ExecutionRepository>();

                    // Register Kafka services
                    services.AddSingleton<KafkaConsumerFactory>();
                    services.AddSingleton<KafkaRecordDeserializer>();
                    services.AddSingleton<ExecutionProcessor>();

                    // Register hosted service
                    services.AddHostedService<KafkaConsumerManager>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                });
    }
}