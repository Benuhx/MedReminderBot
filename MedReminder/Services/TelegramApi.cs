using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace MedReminder.Services {
    public interface ITelegramApi {
        void SetTelegramBotToken(string telegramBotToken);
        Task SendeNachricht(string txt, long chatId, bool enableNotification);
    }

    public class TelegramApi : ITelegramApi {
        private readonly ITelegramPersistenceService _telegramPersistenceService;
        private TelegramBotClient _botClient;

        public TelegramApi(ITelegramPersistenceService telegramPersistenceService) {
            _telegramPersistenceService = telegramPersistenceService;
        }

        public void SetTelegramBotToken(string telegramBotToken) {
            _botClient = new TelegramBotClient(telegramBotToken);
            _botClient.OnMessage += BotClientOnOnMessage;
            _botClient.StartReceiving();
        }

        public async Task SendeNachricht(string txt, long chatId, bool enableNotification) {
            if (_botClient == null) throw new Exception($"Telegram Bot not initialized");
            await _botClient.SendTextMessageAsync(chatId, txt, disableNotification: !enableNotification);
        }

        private async void BotClientOnOnMessage(object sender, MessageEventArgs e) {
            var chatId = e.Message.Chat.Id;
            await SendeNachricht("Hey ho :)", chatId, true);
        }
    }
}