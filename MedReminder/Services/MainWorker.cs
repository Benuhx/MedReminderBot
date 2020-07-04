using System;
using System.Threading.Tasks;
using MedReminder.DTO;
using MedReminder.Entities;
using MedReminder.Repository;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Args;

namespace MedReminder.Services {
    public interface IMainWorker {
        Task Run();
    }

    public class MainWorker : IMainWorker {
        private readonly ILogger<MainWorker> _logger;
        private readonly IBotUserInteractionService _botUserInteractionService;

        public MainWorker(ILogger<MainWorker> logger, IBotUserInteractionService botUserInteractionService) {
            _botUserInteractionService = botUserInteractionService;
            _logger = logger;
        }

        public async Task Run() {
            _logger.LogInformation($"Ist gestart um {DateTime.Now}");
            while (true) {
                await Task.Delay(TimeSpan.FromSeconds(30));
                _logger.LogInformation($"Läuft um {DateTime.Now}");
            }
        }
    }
}