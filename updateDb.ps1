# Benoetigt Install-Module powershell-yaml
Import-Module powershell-yaml
$configFile = (Get-ChildItem -Filter config.yaml -Recurse -ErrorAction SilentlyContinue -Force | Select-Object -First 1).FullName
Write-Output "DB ConnectingStr aus Datei $configFile"

$content = [IO.File]::ReadAllText($configFile)
$yaml = ConvertFrom-Yaml $content
$conStr = $yaml.PostgresConnectionString

dotnet ef dbcontext scaffold --project MedReminder --context MedReminderDbContext --context-dir Repository --output-dir Entities --data-annotations --force $conStr Npgsql.EntityFrameworkCore.PostgreSQL
Write-output "OK"