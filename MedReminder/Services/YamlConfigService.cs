using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MedReminder.DTO;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace MedReminder.Services
{
    public interface IYamlConfigService
    {
        bool ConfigFileExists();
        void WriteConfig();
        Task<Config> ReadConfig();
    }

    public class YamlConfigService : IYamlConfigService
    {
        private readonly string _configFilePath;
        private readonly ILogger<YamlConfigService> _logger;

        public YamlConfigService(ILogger<YamlConfigService> logger)
        {
            _logger = logger;
            var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _configFilePath = Path.Combine(currentDir, "config.yaml");
        }

        public bool ConfigFileExists()
        {
            return File.Exists(_configFilePath);
        }

        public async Task<Config> ReadConfig()
        {
            var deserializer = new Deserializer();

            if (!ConfigFileExists())
                _logger.LogError($"Should read config file, but File '{_configFilePath}' does not exist");

            var fileContent = await File.ReadAllTextAsync(_configFilePath);
            var config = deserializer.Deserialize<Config>(fileContent);
            return config;
        }


        public void WriteConfig()
        {
            var emptyConfig = new Config
            {
                TelegramToken = string.Empty
            };

            var serializer = new Serializer();
            var yaml = serializer.Serialize(emptyConfig);

            _logger.LogInformation($"Wrting default config to {_configFilePath}");

            if (File.Exists(_configFilePath)) File.Delete(_configFilePath);
            File.WriteAllText(_configFilePath, yaml);
        }       
    }
}
