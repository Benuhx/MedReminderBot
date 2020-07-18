using MedReminder.Repository;

namespace MedReminder.Tests {
    public class TestWithDbRepositoryAndEmptyDatabase : TestWithPostgresDb {
        protected Repository.DbRepository DbRepository;
        public TestWithDbRepositoryAndEmptyDatabase() : base() {
        }

        public void LeereDatenbankUndErstelleTabellen() {
            var deleteSql = sqlSkriptService.GetSqlSkript("Delete_Db");
            var createSchemaSQl = sqlSkriptService.GetSqlSkript("01_initial");
            DbRepository.ExecuteSqlScript(deleteSql);
            DbRepository.ExecuteSqlScript(createSchemaSQl);
        }
    }
}