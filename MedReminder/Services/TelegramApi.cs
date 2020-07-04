using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace MedReminder.Services {
    public interface ITelegramApi {
        Action<MessageEventArgs> NeueNachricht { get; set; }
        void SetTelegramBotToken(string telegramBotToken);
        Task SendeNachricht(string txt, long chatId, bool enableNotification);
    }

    public class TelegramApi : ITelegramApi {
        private TelegramBotClient _botClient;
        public Action<MessageEventArgs> NeueNachricht { get; set; }

        public TelegramApi() {

        }

        public void SetTelegramBotToken(string telegramBotToken) {
            _botClient = new TelegramBotClient(telegramBotToken);
            _botClient.OnMessage += BotClientOnOnMessage;
            _botClient.StartReceiving();
        }

        public async Task SendeNachricht(string txt, long chatId, bool enableNotification) {
            if (_botClient == null) throw new Exception("Telegram Bot not initialized");
            await _botClient.SendTextMessageAsync(chatId, txt, disableNotification: !enableNotification);
        }

        private void BotClientOnOnMessage(object sender, MessageEventArgs e) {
            NeueNachricht?.Invoke(e);
        }
    }
}