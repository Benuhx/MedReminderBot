using System;
using System.Linq;
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
        private readonly DbRepository _dbRepository;

        public MainWorker(ILogger<MainWorker> logger, IBotUserInteractionService botUserInteractionService, DbRepository dbRepository) {
            _botUserInteractionService = botUserInteractionService;
            _logger = logger;
            _dbRepository = dbRepository;
        }

        public async Task Run() {
            _logger.LogInformation($"Ist gestart um {DateTime.Now}");

            while (true) {
                var faelligeErinnerungen = _dbRepository.GetFaelligeErinnerungen();

                var tasks = faelligeErinnerungen.Select(x => _botUserInteractionService.SendeErinnerung(x, false)).ToArray();
                Task.WaitAll(tasks);

                var zusaetzlicheErinnerungen = _dbRepository.GetZusaetzlicheErinnerungen();
                tasks = zusaetzlicheErinnerungen.Select(x => _botUserInteractionService.SendeErinnerung(x, true)).ToArray();
                Task.WaitAll(tasks);

                _logger.LogInformation($"Läuft um {DateTime.Now}{Environment.NewLine}{faelligeErinnerungen.Count} Erinnerungen verschickt{Environment.NewLine}{zusaetzlicheErinnerungen.Count} zusaetzliche Erinnerungen verschickt");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}