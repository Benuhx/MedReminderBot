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
        Task SendeErinnerung(Erinnerung e, bool istZusaetzlicheErinnerung);
    }

    public class BotUserInteractionService : IBotUserInteractionService {
        private readonly ITelegramApi _telegramApi;
        private readonly DbRepository _dbRepository;
        private readonly ILogger<BotUserInteractionService> _logger;
        private readonly List<string> _smilies;
        private readonly Random _random;

        public BotUserInteractionService(ITelegramApi telegramApi, DbRepository dbRepository, Config config, ILogger<BotUserInteractionService> logger) {
            _telegramApi = telegramApi;
            _telegramApi.NeueNachricht = VerarbeiteNeueNachrichtWrapper;
            _dbRepository = dbRepository;
            _telegramApi.SetTelegramBotToken(config.TelegramToken);
            _logger = logger;
            _smilies = new List<string> { ":)", "😀", "😛", "😉", "😺" };
            _random = new Random();
        }

        public async Task SendeErinnerung(Erinnerung e, bool istZusaetzlicheErinnerung) {
            var smiley = GetRandomSmiley();
            var chatId = _dbRepository.GetChatIdFromBenutzer(e.Benutzer);
            SpeichereChatZustand(chatId, ZustandChat.WarteAufBestaetigungDerErinnerung);
            await _telegramApi.SendeNachricht($"Hey {e.Benutzer.Name} 😎{Environment.NewLine}Denke an deine Tablette {smiley}{Environment.NewLine}{Environment.NewLine}1️⃣ Antworte mit 'Ok', wenn du deine Tablette genommen hast.{Environment.NewLine}2️⃣ Du kannst mir auch mit einer Uhrzeit antworten, wenn du später erinnert werden möchtest.{Environment.NewLine}3️⃣Wenn du mir nicht antwortest, erinnere ich dich in einer Stunde nochmal {GetRandomSmiley()}", chatId);

            var eg = new ErinnerungGesendet
            {
                ErinnerungId = e.Id,
                GesendetUm = DateTime.UtcNow,
                IstZusaetzlicheErinnerung = istZusaetzlicheErinnerung
            };
            _dbRepository.SpeichereErinnerungGesendet(eg);
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

            if (nachrichtText.ToLower().Contains("reset")) {
                await ResetStart(chatId);
                return;
            }

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
                case ZustandChat.WarteAufBestaetigungDerErinnerung:
                    await AntwortAufErinnerung(benutzer, chatId, nachrichtText);
                    break;
                case ZustandChat.ResetStart:
                    await AntwortAufReset(chatId, nachrichtText);
                    break;
                default:
                    await _telegramApi.SendeNachricht($"Ich habe leider keine passende Antwort für dich ☹{Environment.NewLine}1️⃣ Wenn du die Erinnerungen deaktivieren möchtest, kann du mir eine Nachricht mit 'reset' schreiben", chatId); break;
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
            var uhrzeitErinnerung = ParseUhrzeitAusNachricht(nachrichtText);
            if (uhrzeitErinnerung == null) {
                await _telegramApi.SendeNachricht($"Deine Eingabe ist keine valide Uhrzeit 🤨. Probiere es mit z.B. mit 21:00 für 21 Uhr", chatId);
                return;
            }

            var e = new Erinnerung
            {
                BenutzerId = benutzer.Id,
                UhrzeitUtc = uhrzeitErinnerung.Item1,
                GueltigAbDatim = DateTime.UtcNow.Date
            };
            _dbRepository.SpeichereErinnerung(e);

            SpeichereChatZustand(chatId, ZustandChat.Fertig);
            await _telegramApi.SendeNachricht($"Ich erinnere dich um {uhrzeitErinnerung.Item2:HH:mm} Uhr 💪", chatId);
        }

        private async Task AntwortAufErinnerung(Benutzer benutzer, long chatId, string nachrichtText) {
            Erinnerung erinnerung;
            if (nachrichtText.ToLower().Contains("ok")) {
                SpeichereChatZustand(chatId, ZustandChat.Fertig);

                erinnerung = _dbRepository.GetErinnerung(benutzer);
                erinnerung.ZusaetzlicheErinnerung = null;
                _dbRepository.SpeichereErinnerung(erinnerung);

                await _telegramApi.SendeNachricht($"Super 😎 Ich habe vermerkt, dass du eine Tablette genommen hast {GetRandomSmiley()}", chatId);
                return;
            }

            var uhrzeitErinnerung = ParseUhrzeitAusNachricht(nachrichtText);
            if (uhrzeitErinnerung == null) {
                await _telegramApi.SendeNachricht($"Deine Eingabe ist keine valide Uhrzeit 🤨. Probiere es mit z.B. mit 21:00 für 21 Uhr", chatId);
                return;
            }

            erinnerung = _dbRepository.GetErinnerung(benutzer);
            erinnerung.ZusaetzlicheErinnerung = uhrzeitErinnerung.Item1;
            _dbRepository.SpeichereErinnerung(erinnerung);
            _dbRepository.LoescheErinnerungGesendet(erinnerung.Id, true);
            await _telegramApi.SendeNachricht($"Ich habe die spätere Erinnerung um {uhrzeitErinnerung.Item2:HH:mm} Uhr gespeichert {GetRandomSmiley()}", chatId);
        }

        private async Task ResetStart(long chatId) {
            SpeichereChatZustand(chatId, ZustandChat.ResetStart);
            await _telegramApi.SendeNachricht("Wenn mich zurücksetzen möchtest, werden die Erinnerungen deakitivert. Möchtest du fortfahren (ja / nein)?", chatId);
        }

        private async Task AntwortAufReset(long chatId, string nachrichtText) {
            nachrichtText = nachrichtText.ToLower();
            if (nachrichtText == "ja" || nachrichtText == "j") {
                _dbRepository.ResetFuerChatId(chatId);
                await _telegramApi.SendeNachricht($"Reset erfolgreich 😢{Environment.NewLine}Du kannst mir erneut schreiben, wenn du möchtest.{Environment.NewLine}Ich habe dann aber alles über dich vergessen", chatId);
                return;
            }
            SpeichereChatZustand(chatId, ZustandChat.Fertig);
            await _telegramApi.SendeNachricht("Kein Reset durchgeführt", chatId);
        }

        private ZustandChat GetChatZustand(long chatId) {
            var zustand = _dbRepository.GetChatZustand(chatId);
            if (zustand == null) return ZustandChat.NichtBekannt;
            return (ZustandChat)zustand.Zustand;
        }

        private void SpeichereChatZustand(long chatId, ZustandChat chatZustand) {
            var zustand = new ChatZustand
            {
                ChatId = chatId,
                Zustand = (int)chatZustand
            };
            _dbRepository.SpeichereChatZustand(zustand);
        }

        private string GetRandomSmiley() {
            var r = _random.Next(0, _smilies.Count);
            return _smilies[r];
        }

        private Tuple<DateTime, DateTime> ParseUhrzeitAusNachricht(string nachrichtText) {
            nachrichtText = nachrichtText.Replace(":", "");

            if (nachrichtText.Length == 2 && int.TryParse(nachrichtText, out int number) && number >= 0 && number <= 24) {
                nachrichtText = $"{nachrichtText}00";
            }

            var parseErfolgreich = DateTime.TryParseExact(nachrichtText, "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime uhrzeitErinnerung);
            if (!parseErfolgreich) {
                return null;
            }

            var uhrzeitErinnerungUtc = TimeZoneInfo.ConvertTimeToUtc(uhrzeitErinnerung);
            uhrzeitErinnerungUtc = new DateTime(2000, 1, 1, uhrzeitErinnerungUtc.Hour, uhrzeitErinnerungUtc.Minute, 0);
            uhrzeitErinnerungUtc = DateTime.SpecifyKind(uhrzeitErinnerungUtc, DateTimeKind.Utc);

            uhrzeitErinnerung = new DateTime(2000, 1, 1, uhrzeitErinnerung.Hour, uhrzeitErinnerung.Minute, 0);
            uhrzeitErinnerung = DateTime.SpecifyKind(uhrzeitErinnerung, DateTimeKind.Utc);

            return new Tuple<DateTime, DateTime>(uhrzeitErinnerungUtc, uhrzeitErinnerung);
        }
    }
}