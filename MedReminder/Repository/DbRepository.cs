
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

        public void SpeichereErinnerung(Erinnerung e) {
            using var d = Gdc();
            var dbErinnerung = d.Erinnerung.FirstOrDefault(x => x.Id == e.Id);
            if (dbErinnerung == null) {
                d.Erinnerung.Add(e);
                d.SaveChanges();
                return;
            }
            d.Erinnerung.Update(dbErinnerung);
            dbErinnerung.GueltigAbDatim = e.GueltigAbDatim;
            dbErinnerung.UhrzeitUtc = e.UhrzeitUtc;
            dbErinnerung.ZusaetzlicheErinnerung = e.ZusaetzlicheErinnerung;
            d.SaveChanges();
        }

        public ChatZustand GetChatZustand(long chatId) {
            using var d = Gdc();
            return d.ChatZustand.SingleOrDefault(x => x.ChatId == chatId);
        }

        public Erinnerung GetErinnerung(Benutzer b) {
            using var d = Gdc();
            return d.Erinnerung.SingleOrDefault(x => x.BenutzerId == b.Id);
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
                .Where(x => !x.ErinnerungGesendet.Any(x => x.GesendetUm.Date == curDate && !x.IstZusaetzlicheErinnerung))
                .Include(x => x.Benutzer)
                .ToList();

            return faelligeErinnerungenNochNichtGesendet;
        }

        public List<Erinnerung> GetZusaetzlicheErinnerungen() {
            using var d = Gdc();
            var curDate = DateTime.UtcNow.Date;
            var curTime = DateTime.UtcNow;
            curTime = new DateTime(2000, 1, 1, curTime.Hour, curTime.Minute, 0);

            var faelligeErinnerungenNochNichtGesendet = d.Erinnerung
                .Where(x => x.ZusaetzlicheErinnerung != null)
                .Where(x => x.GueltigAbDatim >= curDate && x.ZusaetzlicheErinnerung <= curTime)
                .Where(x => !x.ErinnerungGesendet.Any(x => x.GesendetUm.Date == curDate && x.IstZusaetzlicheErinnerung))
                .Include(x => x.Benutzer)
                .ToList();

            return faelligeErinnerungenNochNichtGesendet;
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