
using System.Linq;
using MedReminder.DTO;
using MedReminder.Entities;

namespace MedReminder.Repository
{
    public interface IDbRepository
    {
        void AddBenutzer(Benutzer b);
        bool ChatIdExistiert(long chatId);
    }

    public class DbRepository : IDbRepository
    {
        private readonly Config _config;

        public DbRepository(Config config)
        {
            _config = config;
        }

        public bool ChatIdExistiert(long chatId)
        {
            using (var d = Gdc())
            {
                return d.Benutzer.Any(x => x.TelegramChatId == chatId);
            }
        }

        public void AddBenutzer(Benutzer b)
        {
            using (var d = Gdc())
            {
                d.Benutzer.Add(b);
                d.SaveChanges();
            }
        }

        private MedReminderDbContext Gdc() {
            return new MedReminderDbContext(_config);
        }
    }
}