using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Args;

namespace MedReminder.Services
{
    public interface IMainWorker {
        Task Run();
    }

    public class MainWorker : IMainWorker {
        private readonly IYamlConfigService _yamlConfigService;
        private readonly ILogger<MainWorker> _logger;
        private readonly ITelegramApi _telegramApi;

        public MainWorker(IYamlConfigService yamlConfigService, ILogger<MainWorker> _logger, ITelegramApi telegramApi) {
            _yamlConfigService = yamlConfigService;
            this._logger = _logger;
            _telegramApi = telegramApi;
        }

        public async Task Run() {
            if (!_yamlConfigService.ConfigFileExists()) {
                _yamlConfigService.WriteDefaultConfig();
                _logger.LogWarning("Config file not found, created default. Please modify file and restart app");
                return;
            }

            var config = await _yamlConfigService.ReadConfig();
            _telegramApi.SetTelegramBotToken(config.TelegramToken);
            _telegramApi.NeueNachricht = NeueNachrichtEmpfangen;
            while (true) {
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        private async void NeueNachrichtEmpfangen(MessageEventArgs e) {
            await _telegramApi.SendeNachricht("Was geht?", e.Message.Chat.Id, true);
        }
    }
}