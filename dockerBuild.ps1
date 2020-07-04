dotnet publish -c Release
dotnet MedReminder/bin/Release/netcoreapp3.1/publish/MedReminder.dll
docker build -t counter-image -f Dockerfile .