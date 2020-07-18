using System;
using System.Collections.Generic;
using System.Linq;
using MedReminder.Entities;
using MedReminder.Services;
using Moq;
using Xunit;

namespace MedReminder.Tests.DbRepositoryTests {
    public class Erinnerung_Fuer_Naechsten_Tag : TestWithDbRepositoryAndEmptyDatabase {
        private readonly List<Erinnerung> _erinnerungen;
        private readonly List<Erinnerung> _ueberfaellig;
        private readonly List<Erinnerung> _zusaetzlich;

        public Erinnerung_Fuer_Naechsten_Tag() {
            var dts = new Mock<DateTimeService>();
            dts.Setup(x => x.Now).Throws(new Exception("Nur UTC ist erlaubt"));
            dts.Setup(x => x.UtcNow).Returns(new DateTime(2020, 06, 01, 12, 00, 00));

            DbRepository = new Repository.DbRepository(Config, dts.Object);
            LeereDatenbankUndErstelleTabellen();

            DbRepository.SpeichereChatZustand(new Entities.ChatZustand()
            {
                ChatId = 1,
                Zustand = (int) ZustandChat.WarteAufBestaetigungDerErinnerung
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
                UhrzeitUtc = new DateTime(2000, 1, 1, 09, 00, 0),
                GueltigAbDatim = dts.Object.UtcNow.Date.AddDays(1)
            });

            _erinnerungen = DbRepository.GetFaelligeErinnerungen();
            _ueberfaellig = DbRepository.GetUeberfaelligeErinnerungen();
            _zusaetzlich = DbRepository.GetZusaetzlicheErinnerungen();
        }

        [Fact]
        public void findet_keine_faellige_Erinnerungen() {
            Assert.Empty(_erinnerungen);
        }

        [Fact]
        public void findet_keine_uerberfaellige() {
            Assert.Empty(_ueberfaellig);
        }

        [Fact]
        public void findet_keine_zusaetzliche() {
            Assert.Empty(_zusaetzlich);
        }
    }
}