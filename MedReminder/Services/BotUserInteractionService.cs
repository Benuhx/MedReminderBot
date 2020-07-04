using System.Collections.Generic;
using System.Threading.Tasks;
using MedReminder.DTO;
using MedReminder.Entities;
using MedReminder.Repository;
using Telegram.Bot.Args;

namespace MedReminder.Services {
    public interface IBotUserInteractionService {
        void VerarbeiteNeueNachricht(MessageEventArgs e);
    }

    public class BotUserInteractionService : IBotUserInteractionService {
        private readonly ITelegramApi _telegramApi;
        private readonly DbRepository _dbRepository;
        private readonly Config _config;
        private readonly Dictionary<long, ChatZustand> _chatZustand;

        public BotUserInteractionService(ITelegramApi telegramApi, DbRepository dbRepository, Config config) {
            _telegramApi = telegramApi;
            _telegramApi.NeueNachricht = VerarbeiteNeueNachricht;
            _dbRepository = dbRepository;
            _config = config;
            _telegramApi.SetTelegramBotToken(config.TelegramToken);
            _chatZustand = new Dictionary<long, ChatZustand>();
        }

        public async void VerarbeiteNeueNachricht(MessageEventArgs e) {
            var chatId = e.Message.Chat.Id;
            var nachrichtText = e.Message.Text;
            if (!_chatZustand.ContainsKey(chatId)) {
                _chatZustand.Add(chatId, ChatZustand.NichtBekannt);
            }
            var zustand = _chatZustand[chatId];

            switch (zustand) {
                case ChatZustand.NichtBekannt:
                    await NeuenBenutzerRegistrieren(chatId);
                    break;
                case ChatZustand.WarteAufName:
                    await SpeichereBenutzer(chatId, nachrichtText);
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

        private bool BenutzerIstBekannt(long chatId) {
            return _dbRepository.ChatIdExistiert(chatId);
        }
    }

    public enum ChatZustand {
        NichtBekannt,
        WarteAufName,
        WarteAufUhrzeit
    }
}