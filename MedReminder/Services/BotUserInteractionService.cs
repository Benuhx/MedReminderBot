using System;
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
            var nachrichtText = e.Message.Text.Trim();

            if (nachrichtText.Contains("/start")) return;

            var chatZustand = GetChatZustand(chatId);

            switch (chatZustand) {
                case ZustandChat.NichtBekannt:
                    await NeuenBenutzerRegistrieren(chatId);
                    break;
                case ZustandChat.WarteAufName:
                    await SpeichereBenutzer(chatId, nachrichtText);
                    break;
                case ZustandChat.WarteAufUhrzeit:
                    await SpeichereUhrzeit(benutzer, chatId, nachrichtText);
                    break;
                default:
                    await _telegramApi.SendeNachricht("Ich habe leider keine passende Antwort für dich ☹", chatId);
                    break;
            }
        }

        private async Task NeuenBenutzerRegistrieren(long chatId) {
            SpeichereChatZustand(chatId, ZustandChat.WarteAufName);
            await _telegramApi.SendeNachricht($"Wie heißt du ✌?", chatId);
        }

        private async Task SpeichereBenutzer(long chatId, string nachrichtText) {
            var b = new Benutzer
            {
                Name = nachrichtText,
                TelegramChatId = chatId
            };
            _dbRepository.AddBenutzer(b);

            SpeichereChatZustand(chatId, ZustandChat.WarteAufUhrzeit);
            await _telegramApi.SendeNachricht($"Willkommen, {b.Name} 😎{Environment.NewLine}Um wie viel Uhr soll ich dich erinnern?", chatId);
        }

        private async Task SpeichereUhrzeit(Benutzer benutzer, long chatId, string nachrichtText) {
            nachrichtText = nachrichtText.Replace(":", "");

            if(nachrichtText.Length == 2 && int.TryParse(nachrichtText, out int number) && number >= 0 && number <=24 ) {
                nachrichtText = $"{nachrichtText}00";
            }

            var parseErfolgreich = DateTime.TryParseExact(nachrichtText, "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime uhrzeitErinnerung);
            if(!parseErfolgreich) {
                await _telegramApi.SendeNachricht($"Deine Eingabe ist keine valide Uhrzeit 🤨. Probiere es mit z.B. mit 21:00 für 21 Uhr", chatId);
                return;
            }

            uhrzeitErinnerung = new DateTime(2000, 1, 1, uhrzeitErinnerung.Hour, uhrzeitErinnerung.Minute, 0);
            uhrzeitErinnerung = DateTime.SpecifyKind(uhrzeitErinnerung, DateTimeKind.Local);

            var e = new Erinnerung
            {
                BenutzerId = benutzer.Id,
                UhrzeitUtc = uhrzeitErinnerung.ToUniversalTime()
            };
            _dbRepository.SpeichereErinerung(e);

            SpeichereChatZustand(chatId, ZustandChat.Fertig);
            await _telegramApi.SendeNachricht($"Ich erinnere dich um {uhrzeitErinnerung:HH:mm} Uhr 💪", chatId);
        }

        private ZustandChat GetChatZustand(long chatId) {
            var zustand = _dbRepository.GetChatZustand(chatId);
            if(zustand == null) return ZustandChat.NichtBekannt;
            return (ZustandChat)zustand.Zustand;
        }

        private void SpeichereChatZustand(long chatId, ZustandChat chatZustand) {
            var zustand = new Entities.ChatZustand
            {
                ChatId = chatId,
                Zustand = (int)chatZustand
            };
            _dbRepository.SpeichereChatZustand(zustand);
        }
    }
}