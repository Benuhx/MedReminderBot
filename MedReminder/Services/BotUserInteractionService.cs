using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ILogger<BotUserInteractionService> _logger;

        public BotUserInteractionService(ITelegramApi telegramApi, DbRepository dbRepository, Config config, ILogger<BotUserInteractionService> logger) {
            _telegramApi = telegramApi;
            _telegramApi.NeueNachricht = VerarbeiteNeueNachrichtWrapper;
            _dbRepository = dbRepository;
            _telegramApi.SetTelegramBotToken(config.TelegramToken);
            _logger = logger;
        }

        private async void VerarbeiteNeueNachrichtWrapper(MessageEventArgs e) {
            try {
                await VerarbeiteNeueNachricht(e);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString());
            }
        }

        private async Task VerarbeiteNeueNachricht(MessageEventArgs e) {
            var chatId = e.Message.Chat.Id;
            var benutzer = _dbRepository.GetBenutzerAusChatId(chatId);
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
                case ChatZustand.WarteAufUhrzeit:
                    await SpeichereUhrzeit(benutzer, chatId, nachrichtText);
                    break;
                default:
                    await _telegramApi.SendeNachricht("Ich habe leider keine passende Antwort für dich ☹", chatId, true);
                    break;
            }
        }

        private async Task NeuenBenutzerRegistrieren(long chatId) {
            await _telegramApi.SendeNachricht($"Wie heißt du ✌?", chatId, true);
            SpeichereChatZustand(chatId, ChatZustand.WarteAufName);
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
            SpeichereChatZustand(chatId, ChatZustand.WarteAufUhrzeit);
        }

        private async Task SpeichereUhrzeit(Benutzer benutzer, long chatId, string nachrichtText) {
            nachrichtText = nachrichtText.Replace(":", "");
            if (nachrichtText.Length != 4) {
                await _telegramApi.SendeNachricht("Die Uhrzeit muss 4 Zahlen im 24h Format enthalten (z.B. 21:00 oder 2100 für 21 Uhr", chatId, true);
                return;
            }

            var dateTime = DateTime.ParseExact(nachrichtText, "HHmm", CultureInfo.InvariantCulture);
            var e = new Erinnerung
            {
                BenutzerId = benutzer.Id,
                UhrzeitUtc = dateTime
            };
            _dbRepository.SpeichereErinerung(e);

            await _telegramApi.SendeNachricht($"Ich erinnere dich um {dateTime:HH:mm} Uhr 💪", chatId, true);
            SpeichereChatZustand(chatId, ChatZustand.Fertig);
        }

        private ChatZustand GetChatZustand(long chatId) {
            var zustand = _dbRepository.GetChatZustand(chatId);
            if(zustand == null) return ChatZustand.NichtBekannt;
            return (ChatZustand)zustand.Zustand;
        }

        private void SpeichereChatZustand(long chatId, ChatZustand chatZustand) {
            var zustand = new Entities.ChatZustand
            {
                ChatId = chatId,
                Zustand = (int)chatZustand
            };
            _dbRepository.SpeichereChatZustand(zustand);
        }
    }

    public enum ChatZustand {
        NichtBekannt,
        WarteAufName,
        WarteAufUhrzeit,
        Fertig
    }
}