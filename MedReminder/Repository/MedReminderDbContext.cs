using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MedReminder.Entities;
using MedReminder.DTO;
using Microsoft.Extensions.DependencyInjection;

namespace MedReminder.Repository
{
    public partial class MedReminderDbContext : DbContext
    {
        private readonly Config _config;

        public MedReminderDbContext()
        {

        }

        public MedReminderDbContext(Config config)
        {
            _config = config;
        }

        public MedReminderDbContext(DbContextOptions<MedReminderDbContext> options, Config config)
            : base(options)
        {
            _config = config;
        }

        public virtual DbSet<Benutzer> Benutzer { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_config.PostgresConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Benutzer>(entity =>
            {
                entity.HasIndex(e => e.TelegramChatId)
                    .HasName("user_telegram_chat_id_uindex")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("nextval('user_id_seq'::regclass)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
