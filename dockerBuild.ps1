Remove-Item "./Docker/files" -Recurse -ErrorAction Ignore
dotnet publish --nologo -c Release -r=linux-arm -o "./Docker/files" --self-contained false
cp config.yaml.template Docker/files/config.yaml

cd Docker
docker build -t counter-image -f Dockerfile .
cd ..