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
        public virtual DbSet<ErinnerungGesendet> ErinnerungGesendet { get; set; }

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

                entity.HasOne(d => d.TelegramChat)
                    .WithOne(p => p.Benutzer)
                    .HasPrincipalKey<ChatZustand>(p => p.ChatId)
                    .HasForeignKey<Benutzer>(d => d.TelegramChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("benutzer_chat_zustand_chat_id_fk");
            });

            modelBuilder.Entity<ChatZustand>(entity => {
                entity.HasIndex(e => e.ChatId)
                    .HasName("chat_zustand_telegram_chat_id_uindex")
                    .IsUnique();
            });

            modelBuilder.Entity<Erinnerung>(entity => {
                entity.HasOne(d => d.Benutzer)
                    .WithMany(p => p.Erinnerung)
                    .HasForeignKey(d => d.BenutzerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("erinnerung_benutzer_id_fk");
            });

            modelBuilder.Entity<ErinnerungGesendet>(entity => {
                entity.Property(e => e.GesendetUm).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Erinnerung)
                    .WithMany(p => p.ErinnerungGesendet)
                    .HasForeignKey(d => d.ErinnerungId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("erinnerung_gesendet_erinnerung_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
