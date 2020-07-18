Remove-Item "./Docker/filesLinux64" -Recurse -ErrorAction Ignore
Remove-Item "./Docker/filesLinuxArm" -Recurse -ErrorAction Ignore

dotnet publish MedReminder/MedReminder.csproj -c Release -o "./Docker/filesLinux64" -r=linux-x64 --nologo --no-self-contained
dotnet publish MedReminder/MedReminder.csproj -c Release -o "./Docker/filesLinuxArm" -r=linux-arm --nologo --no-self-contained

cd Docker
docker build -f Dockerfile-x64 -t benuhx/med-reminder:latest .
docker build -f Dockerfile-arm -t benuhx/med-reminder:latest-arm .
# docker buildx build -f Dockerfile-arm --platform linux/arm/v7 -t benuhx/med-reminder:latest-arm .
cd ..

docker push benuhx/med-reminder:latest
docker push benuhx/med-reminder:latest-arm

docker image prune -f