using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MedReminder.Entities;
using MedReminder.DTO;

namespace MedReminder.Repository {
    public partial class MedReminderDbContext : DbContext {
        private readonly Config _config;

        public MedReminderDbContext(Config config) {
            _config = config;
        }

        public MedReminderDbContext() {
        }

        public MedReminderDbContext(DbContextOptions<MedReminderDbContext> options)
            : base(options) {
        }

        public virtual DbSet<Benutzer> Benutzer { get; set; }
        public virtual DbSet<ChatZustand> ChatZustand { get; set; }
        public virtual DbSet<Erinnerung> Erinnerung { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql(_config.PostgresConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Benutzer>(entity => {
                entity.HasIndex(e => e.TelegramChatId)
                    .HasName("user_telegram_chat_id_uindex")
                    .IsUnique();
            });

            modelBuilder.Entity<ChatZustand>(entity => {
                entity.HasIndex(e => e.ChatId)
                    .HasName("chat_zustand_telegram_chat_id_uindex")
                    .IsUnique();

                entity.HasOne(d => d.Chat)
                    .WithOne(p => p.ChatZustand)
                    .HasPrincipalKey<Benutzer>(p => p.TelegramChatId)
                    .HasForeignKey<ChatZustand>(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("chat_zustand_benutzer_telegram_chat_id_fk");
            });

            modelBuilder.Entity<Erinnerung>(entity => {
                entity.HasOne(d => d.Benutzer)
                    .WithMany(p => p.Erinnerung)
                    .HasForeignKey(d => d.BenutzerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("erinnerung_benutzer_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
