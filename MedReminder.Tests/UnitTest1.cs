using System;
using MedReminder.Repository;
using MedReminder.Services;
using Moq;
using Xunit;

namespace MedReminder.Tests {
    public class Das_DbRepository : TestWithDbRepositoryAndEmptyDatabase {
        public Das_DbRepository() {
            var dts = new Mock<DateTimeService>();
            dts.Setup(x => x.Now).Throws(new Exception("Nur UTC ist erlaubt"));
            dts.Setup(x => x.UtcNow).Returns(new DateTime(2020, 06, 01, 12, 00, 00));

            DbRepository = new DbRepository(Config, dts.Object);
            LeereDatenbankUndErstelleTabellen();

            DbRepository.SpeichereChatZustand(new Entities.ChatZustand() {
                ChatId = 1
            });
            DbRepository.AddBenutzer(new Entities.Benutzer() {
                Id = 1,
                TelegramChatId = 1,
                Name = "Timm"
            });
            DbRepository.SpeichereErinnerung(new Entities.Erinnerung() {
                BenutzerId = 1,
                UhrzeitUtc = new DateTime(2000, 1, 1, 11, 0, 0)
            });
        }

        [Fact]
        public void findet_faellige_Erinnerungen() {
            var erinnerungen = DbRepository.GetFaelligeErinnerungen();
            Assert.Single(erinnerungen);
        }
    }
}
