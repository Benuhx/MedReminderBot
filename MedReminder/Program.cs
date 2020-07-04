using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MedReminder.DTO;
using MedReminder.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace MedReminder {
    public class Program {
        public static async Task Main(string[] args) {
            using (var container = ConfigureDependencyInjectionAndCreateContainer()) {
                var configReader = container.GetInstance<YamlConfigService>();
                if (!configReader.ConfigFileExists()) {
                    configReader.WriteDefaultConfig();
                    return;
                }
                var config = await configReader.ReadConfig();
                container.Inject<Config>(config);

                var mainWorker = container.GetInstance<IMainWorker>();
                await mainWorker.Run();
                if (Debugger.IsAttached) {
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadKey();
                }
            }
        }

        private static Container ConfigureDependencyInjectionAndCreateContainer() {
            var services = new ServiceCollection();

            services.AddLogging(configure => configure
                    .AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            var container = new Container();
            container.Configure(config => {
                config.Scan(x => {
                    x.AssemblyContainingType<Program>();
                    x.WithDefaultConventions();
                });

                config.Populate(services);
            });

            return container;
        }
    }
}