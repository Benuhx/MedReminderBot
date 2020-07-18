using System.IO;
using System.Reflection;

namespace MedReminder.Tests {
    public interface ISqlSkriptService {
        string GetSqlSkript(string name);
    }

    public class SqlSkriptService : ISqlSkriptService {
        public string GetSqlSkript(string name) {
            if (!name.EndsWith(".sql")) name += ".sql";
            var curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (!Directory.Exists(Path.Combine(curDir, "Sql_Schema"))) {
                var parentDir = Directory.GetParent(curDir);
                if (parentDir == null) {
                    throw new FileNotFoundException($"Es wurde kein {name} gefunden. Aktuelles Verzeichnis: {curDir}");
                }
                curDir = parentDir.FullName;
            }

            var dateiPfad = Path.Combine(curDir, "Sql_Schema", name);
            if (!File.Exists(dateiPfad)) {
                throw new FileNotFoundException($"Das SQL-Skript {dateiPfad} existiert nicht");
            }
            return File.ReadAllText(dateiPfad);
        }
    }
}