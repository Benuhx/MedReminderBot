using System;
using System.Collections.Generic;
using System.Linq;
using MedReminder.Entities;
using MedReminder.Services;
using MedReminder.Tests;
using Moq;
using Xunit;

[assembly: CollectionBehavior(MaxParallelThreads = 1)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]

namespace MedReminder.DbRepositoryTests {
    public class Erinnerung_1_Minute_in_Vergangenheit : TestWithDbRepositoryAndEmptyDatabase {
        private readonly List<Erinnerung> _erinnerungen;
        private readonly List<Erinnerung> _ueberfaellig;
        private readonly List<Erinnerung> _zusaetzlich;

        public Erinnerung_1_Minute_in_Vergangenheit() {
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
                UhrzeitUtc = new DateTime(2000, 1, 1, 11, 59, 0)
            });

            _erinnerungen = DbRepository.GetFaelligeErinnerungen();
            _ueberfaellig = DbRepository.GetUeberfaelligeErinnerungen();
            _zusaetzlich = DbRepository.GetZusaetzlicheErinnerungen();
        }

        [Fact]
        public void findet_eine_faellige_Erinnerungen() {
            Assert.Single(_erinnerungen);
        }

        [Fact]
        public void findet_den_Namen_Timm() {
            Assert.Equal("Timm", _erinnerungen.First().Benutzer.Name);
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
