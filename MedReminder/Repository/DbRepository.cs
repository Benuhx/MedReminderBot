
using System.Linq;
using MedReminder.DTO;
using MedReminder.Entities;

namespace MedReminder.Repository {
    public interface IDbRepository {
        void AddBenutzer(Benutzer b);
        bool ChatIdExistiert(long chatId);
    }

    public class DbRepository : IDbRepository {
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
            if(aktuellerZustand == null) {
                d.ChatZustand.Add(zustand);
                d.SaveChanges();
                return;
            }
            d.Update(aktuellerZustand);
            aktuellerZustand.Zustand = zustand.Zustand;
            d.SaveChanges();
        }

        private MedReminderDbContext Gdc() {
            return new MedReminderDbContext(_config);
        }
    }
}