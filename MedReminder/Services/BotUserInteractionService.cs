using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedReminder.DTO;
using MedReminder.Entities;
using MedReminder.Repository;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Args;

namespace MedReminder.Services {
    public interface IBotUserInteractionService {

    }

    public class BotUserInteractionService : IBotUserInteractionService {
        private readonly ITelegramApi _telegramApi;
        private readonly DbRepository _dbRepository;
        private readonly Config _config;
        private readonly Dictionary<long, ChatZustand> _chatZustand;
        private readonly ILogger<BotUserInteractionService> _logger;

        public BotUserInteractionService(ITelegramApi telegramApi, DbRepository dbRepository, Config config, ILogger<BotUserInteractionService> logger) {
            _telegramApi = telegramApi;
            _telegramApi.NeueNachricht = VerarbeiteNeueNachricht;
            _dbRepository = dbRepository;
            _config = config;
            _telegramApi.SetTelegramBotToken(config.TelegramToken);
            _chatZustand = new Dictionary<long, ChatZustand>();
            _logger = logger;
        }

        private async void VerarbeiteNeueNachrichtWrapper(MessageEventArgs e) {
            try {
                VerarbeiteNeueNachricht(e);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString());
            }
        }

        private async void VerarbeiteNeueNachricht(MessageEventArgs e) {
            var chatId = e.Message.Chat.Id;
            var nachrichtText = e.Message.Text;

            if (nachrichtText.Contains("/start")) return;

            var chatZustand = GetChatZustand(chatId);

            switch (chatZustand) {
                case ChatZustand.NichtBekannt:
                    await NeuenBenutzerRegistrieren(chatId);
                    break;
                case ChatZustand.WarteAufName:
                    await SpeichereBenutzer(chatId, nachrichtText);
                    break;
                default:
                    await _telegramApi.SendeNachricht("Ich habe leider keine passende Antwort für dich ☹", chatId, true);
                    break;
            }
        }

        private async Task NeuenBenutzerRegistrieren(long chatId) {
            await _telegramApi.SendeNachricht($"Wie heißt du ✌?", chatId, true);
            _chatZustand[chatId] = ChatZustand.WarteAufName;
        }

        private async Task SpeichereBenutzer(long chatId, string nachrichtText) {
            var b = new Benutzer
            {
                Name = nachrichtText,
                TelegramChatId = chatId
            };
            _dbRepository.AddBenutzer(b);
            await _telegramApi.SendeNachricht($"Willkommen, {b.Name} 😎", chatId, true);
            await _telegramApi.SendeNachricht($"Um wie viel Uhr soll ich dich erinnern?", chatId, true);
            _chatZustand[chatId] = ChatZustand.WarteAufUhrzeit;
        }

        private ChatZustand GetChatZustand(long chatId) {
            if (!_chatZustand.ContainsKey(chatId)) {
                _chatZustand.Add(chatId, ChatZustand.NichtBekannt);
            }

            return _chatZustand[chatId];
        }

        private void SetzteChatZustand(long chatId, ChatZustand chatZustand) {
            _chatZustand[chatId] = chatZustand;
        }
    }

    public enum ChatZustand {
        NichtBekannt,
        WarteAufName,
        WarteAufUhrzeit
    }
}