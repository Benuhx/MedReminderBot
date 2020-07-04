
using System;
using System.Collections.Generic;
using System.Linq;
using MedReminder.DTO;
using MedReminder.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedReminder.Repository {
    public class DbRepository {
        private readonly Config _config;

        public DbRepository(Config config) {
            _config = config;
        }

        public bool ChatIdExistiert(long chatId) {
            using var d = Gdc();
            return d.Benutzer.Any(x => x.TelegramChatId == chatId);
        }

        public void AddBenutzer(Benutzer b) {
            using var d = Gdc();
            d.Benutzer.Add(b);
            d.SaveChanges();
        }

        public Benutzer GetBenutzerAusChatId(long chatId) {
            using var d = Gdc();
            return d.Benutzer.SingleOrDefault(x => x.TelegramChatId == chatId);
        }

        public long GetChatIdFromBenutzer(Benutzer b) {
            using var d = Gdc();
            return d.Benutzer.SingleOrDefault(x => x.Id == b.Id).TelegramChatId;
        }

        public void SpeichereErinerung(Erinnerung e) {
            using var d = Gdc();
            d.Erinnerung.Add(e);
            d.SaveChanges();
        }

        public ChatZustand GetChatZustand(long chatId) {
            using var d = Gdc();
            return d.ChatZustand.SingleOrDefault(x => x.ChatId == chatId);
        }

        public void SpeichereChatZustand(ChatZustand zustand) {
            using var d = Gdc();
            var aktuellerZustand = d.ChatZustand.FirstOrDefault(x => x.ChatId == zustand.ChatId);
            if (aktuellerZustand == null) {
                d.ChatZustand.Add(zustand);
                d.SaveChanges();
                return;
            }
            d.Update(aktuellerZustand);
            aktuellerZustand.Zustand = zustand.Zustand;
            d.SaveChanges();
        }

        public List<Erinnerung> GetFaelligeErinnerungen() {
            using var d = Gdc();
            var curDate = DateTime.UtcNow.Date;
            var curTime = DateTime.UtcNow;
            curTime = new DateTime(2000, 1, 1, curTime.Hour, curTime.Minute, 0);

            var faelligeErinnerungenNochNichtGesendet = d.Erinnerung
                .Where(x => x.GueltigAbDatim >= curDate && x.UhrzeitUtc <= curTime)
                .Where(x => !x.ErinnerungGesendet.Any(x => x.GesendetUm.Date == curDate))
                .Include(x => x.Benutzer);

            return faelligeErinnerungenNochNichtGesendet.ToList();
        }

        public void SpeichereErinnerungGesendet(ErinnerungGesendet e) {
            using var d = Gdc();
            d.ErinnerungGesendet.Add(e);
            d.SaveChanges();
        }



        private MedReminderDbContext Gdc() {
            return new MedReminderDbContext(_config);
        }
    }
}