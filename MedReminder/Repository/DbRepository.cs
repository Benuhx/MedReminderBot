
using System;
using System.Collections.Generic;
using System.Linq;
using MedReminder.DTO;
using MedReminder.Entities;
using MedReminder.Services;
using Microsoft.EntityFrameworkCore;

namespace MedReminder.Repository {
    public class DbRepository {
        private readonly Config _config;
        private readonly DateTimeService _dateTimeService;

        public DbRepository(Config config, DateTimeService dateTimeService) {
            _config = config;
            _dateTimeService = dateTimeService;
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
            var curDate = _dateTimeService.UtcNow.Date;
            var curTime = _dateTimeService.UtcNow;
            curTime = new DateTime(2000, 1, 1, curTime.Hour, curTime.Minute, 0);

            var faelligeErinnerungenNochNichtGesendet = d.Erinnerung
                .Where(x => x.GueltigAbDatim <= curDate && x.UhrzeitUtc <= curTime)
                .Where(x => !x.ErinnerungGesendet.Any(x => x.GesendetUm.Date == curDate && !x.IstZusaetzlicheErinnerung))
                .Include(x => x.Benutzer)
                .ToList();

            return faelligeErinnerungenNochNichtGesendet;
        }

        public List<Erinnerung> GetZusaetzlicheErinnerungen() {
            using var d = Gdc();
            var curDate = _dateTimeService.UtcNow.Date;
            var curTime = _dateTimeService.UtcNow;
            curTime = new DateTime(2000, 1, 1, curTime.Hour, curTime.Minute, 0);

            var faelligeErinnerungenNochNichtGesendet = d.Erinnerung
                .Where(x => x.ZusaetzlicheErinnerung != null)
                .Where(x => x.GueltigAbDatim <= curDate && x.ZusaetzlicheErinnerung <= curTime)
                .Where(x => !x.ErinnerungGesendet.Any(x => x.GesendetUm.Date == curDate && x.IstZusaetzlicheErinnerung))
                .Include(x => x.Benutzer)
                .ToList();

            return faelligeErinnerungenNochNichtGesendet;
        }

        public List<Erinnerung> GetUeberfaelligeErinnerungen() {
            using var d = Gdc();
            var curDate = _dateTimeService.UtcNow.Date;
            var curTime = _dateTimeService.UtcNow;

            var ueberfaelligeErinnerungen =
            d.ChatZustand
            .Where(x => x.Zustand == (int) ZustandChat.WarteAufBestaetigungDerErinnerung)
            .SelectMany(x => x.Benutzer.Erinnerung)
            .Where(X => X.ZusaetzlicheErinnerung == null)
            .Where(x => curTime >= x.ErinnerungGesendet.Where(y => y.ErinnerungId == x.Id).Max(y => y.GesendetUm).AddHours(1))
            .Include(x => x.Benutzer)
            .ToList();

            return ueberfaelligeErinnerungen;
        }

        public Erinnerung GetErinnerungFuerChatId(long chatId) {
            using var d = Gdc();
            var erinnerung = d.Erinnerung.SingleOrDefault(x => x.Benutzer.TelegramChatId == chatId);
            return erinnerung;
        }

        public void SpeichereErinnerungGesendet(ErinnerungGesendet e) {
            using var d = Gdc();
            d.ErinnerungGesendet.Add(e);
            d.SaveChanges();
        }

        public void LoescheErinnerungGesendet(int erinnerungId, bool istZusaetzlicheErinnerung) {
            using var d = Gdc();
            d.Database.ExecuteSqlInterpolated($"Delete from erinnerung_gesendet eg where eg.erinnerung_id = {erinnerungId} and eg.ist_zusaetzliche_erinnerung = {istZusaetzlicheErinnerung}");
        }

        public void LoescheZusatzlicheErinnerung(Erinnerung e) {
            using var d = Gdc();
            d.Erinnerung.Update(e);
            e.ZusaetzlicheErinnerung = null;
            d.SaveChanges();
        }

        public void ResetFuerChatId(long chatId) {
            using var d = Gdc();
            var benutzer = d.Benutzer.SingleOrDefault(x => x.TelegramChatId == chatId);
            if (benutzer != null) {
                //erinnerung_gesendet ist cascade delete
                d.Database.ExecuteSqlInterpolated($"delete from erinnerung e where e.benutzer_id = {benutzer.Id};");
                d.Database.ExecuteSqlInterpolated($"delete from benutzer b where b.id = {benutzer.Id};");
            }
            d.Database.ExecuteSqlInterpolated($"delete from chat_zustand where chat_id = {chatId};");
        }

        public void ExecuteSqlScript(string sql) {
            using var d = Gdc();
            using var t = d.Database.BeginTransaction();
            try {
                d.Database.ExecuteSqlRaw(sql);
                t.Commit();
            } catch (Exception e) {
                t.Rollback();
                throw e;
            }
        }

        private MedReminderDbContext Gdc() {
            return new MedReminderDbContext(_config);
        }
    }
}