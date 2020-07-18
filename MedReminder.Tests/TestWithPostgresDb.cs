using Microsoft.EntityFrameworkCore;
using MedReminder.Repository;
using MedReminder.DTO;
using System.Data;
using System;
using MedReminder.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MedReminder.Tests {
    public abstract class TestWithPostgresDb {
        protected readonly Config Config;
        protected readonly SqlSkriptService sqlSkriptService;

        protected TestWithPostgresDb() {
            var configService = new YamlConfigService(new NullLogger<YamlConfigService>(), "config_unit_tests.yaml");
            Config = configService.ReadConfig();
            if(!Config.PostgresConnectionString.Contains("Database=med_reminder_unit_tests")) throw new Exception("ConnectionStr muss für die TestDb sein!");
            sqlSkriptService = new SqlSkriptService();
        }
    }
}