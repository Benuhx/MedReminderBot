using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MedReminder.Repository;
using Microsoft.Extensions.Logging;

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
            var istDebug = Debugger.IsAttached;

            while (true) {
                var faelligeErinnerungen = _dbRepository.GetFaelligeErinnerungen();
                var tasks = faelligeErinnerungen.Select(x => _botUserInteractionService.SendeErinnerung(x, ErinnerungsTyp.ErsteErinnerung)).ToArray();
                Task.WaitAll(tasks);

                var zusaetzlicheErinnerungen = _dbRepository.GetZusaetzlicheErinnerungen();
                tasks = zusaetzlicheErinnerungen.Select(x => _botUserInteractionService.SendeErinnerung(x, ErinnerungsTyp.ZusaetzlicheErinnerung)).ToArray();
                Task.WaitAll(tasks);

                var ueberfaelligeErinnerungen = _dbRepository.GetUeberfaelligeErinnerungen();
                tasks = ueberfaelligeErinnerungen.Select(x => _botUserInteractionService.SendeErinnerung(x, ErinnerungsTyp.UeberfaelligeErinnerung)).ToArray();
                Task.WaitAll(tasks);

                _logger.LogInformation($"Läuft um {DateTime.Now}{Environment.NewLine}{faelligeErinnerungen.Count} Erinnerungen verschickt{Environment.NewLine}{zusaetzlicheErinnerungen.Count} zusaetzliche Erinnerungen verschickt{Environment.NewLine}{ueberfaelligeErinnerungen.Count} überfällige Erinnerungen verschickt");

                if(istDebug) {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                } else {
                    await Task.Delay(TimeSpan.FromSeconds(60));
                }

            }
        }
    }
}