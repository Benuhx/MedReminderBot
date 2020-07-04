using System;
using System.Threading.Tasks;
using MedReminder.DTO;
using MedReminder.Entities;
using MedReminder.Repository;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Args;

namespace MedReminder.Services
{
    public interface IMainWorker
    {
        Task Run();
    }

    public class MainWorker : IMainWorker
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly IDbRepository _dbRepository;
        private readonly IBotUserInteractionService _botUserInteractionService;
        private readonly Config _config;

        public MainWorker(Config config, ILogger<MainWorker> logger, IDbRepository dbRepository, IBotUserInteractionService botUserInteractionService)
        {
            _botUserInteractionService = botUserInteractionService;
            _config = config;
            _logger = logger;
            _dbRepository = dbRepository;
        }

        public async Task Run()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}