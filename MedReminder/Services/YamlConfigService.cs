using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MedReminder.DTO;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace MedReminder.Services {
    public interface IYamlConfigService {
        bool ConfigFileExists();
        void WriteDefaultConfig();
        Task<Config> ReadConfig();
    }

    public class YamlConfigService : IYamlConfigService {
        private readonly string _configFileName;
        private string _configFilePath;
        private readonly ILogger<YamlConfigService> _logger;

        public YamlConfigService(ILogger<YamlConfigService> logger) {
            _logger = logger;
            _configFileName = "config.yaml";
            _configFilePath = GetConfigFilePath();
        }

        public bool ConfigFileExists() {
            return File.Exists(_configFilePath);
        }

        public async Task<Config> ReadConfig() {
            var deserializer = new Deserializer();

            if (!ConfigFileExists())
                _logger.LogError($"Should read config file, but File '{_configFilePath}' does not exist");

            var fileContent = await File.ReadAllTextAsync(_configFilePath);
            var config = deserializer.Deserialize<Config>(fileContent);
            return config;
        }


        public void WriteDefaultConfig() {
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

        private string GetConfigFilePath() {
             var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
             while(!File.Exists(Path.Combine(curDir, _configFileName))) {
                 _logger.LogDebug($"Keine {_configFileName} im Ordner {curDir} gefunden. Navigiere einen Ordner höher");
                 var parentDir = Directory.GetParent(curDir);
                 if(parentDir == null) {
                     throw new FileNotFoundException($"Es wurde keine {_configFilePath} gefunden. Aktuelles Verzeichnis: {curDir}");
                 }
                 curDir = parentDir.FullName;
             }
             _logger.LogInformation($"Nutze {_configFileName} im Ordner {curDir} gefunden");
             return Path.Combine(curDir, _configFileName);
        }
    }
}