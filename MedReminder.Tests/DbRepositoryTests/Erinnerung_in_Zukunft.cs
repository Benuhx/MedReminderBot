using System;
using System.Collections.Generic;
using System.Linq;
using MedReminder.Repository;
using MedReminder.Entities;
using MedReminder.Services;
using Moq;
using Xunit;

namespace MedReminder.Tests.DbRepositoryTests {
    public class Erinnerung_in_Zukunft : TestWithDbRepositoryAndEmptyDatabase {
        private readonly List<Erinnerung> _erinnerungen;

        public Erinnerung_in_Zukunft() {
            var dts = new Mock<DateTimeService>();
            dts.Setup(x => x.Now).Throws(new Exception("Nur UTC ist erlaubt"));
            dts.Setup(x => x.UtcNow).Returns(new DateTime(2020, 06, 01, 12, 00, 00));

            DbRepository = new Repository.DbRepository(Config, dts.Object);
            LeereDatenbankUndErstelleTabellen();

            DbRepository.SpeichereChatZustand(new Entities.ChatZustand()
            {
                ChatId = 1
            });
            DbRepository.AddBenutzer(new Entities.Benutzer()
            {
                Id = 1,
                TelegramChatId = 1,
                Name = "Timm"
            });
            DbRepository.SpeichereErinnerung(new Entities.Erinnerung()
            {
                Id = 1,
                BenutzerId = 1,
                UhrzeitUtc = new DateTime(2000, 1, 1, 13, 0, 0)
            });

            _erinnerungen = DbRepository.GetFaelligeErinnerungen();
        }

        [Fact]
        public void findet_keine_faelligen_Erinnerungen() {
            Assert.Empty(_erinnerungen);
        }
    }
}