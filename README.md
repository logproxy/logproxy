## LogProxy

### Build

#### Simple Build using cake

Running the following should build, test, publish, and finally build a docker image.

```bash
dotnet tool restore
dotnet cake
```

#### Manual Build

If the simple build does not work out, please follow these steps.

```bash

dotnet restore LogProxy.sln
dotnet build -c Release LogProxy.sln
dotnet test LogProxy.sln
dotnet publish -c Release -o ./artifacts LogProxy/LogProxy.csproj
cd ./artifacts
docker build -t logproxy/logproxy .
```

### Run LogProxy

Following either of the buid steps yields an artifact directory, which contains the build artifacts. Also there is a docker image.

#### Run LogProxy locally

todo -> set api key; set authorized user; both ideally in appsettings

```bash
cd ./artifacts
dotnet LogProxy.dll
```

#### Run LogProxy docker image

todo -> set api key; set authorized user; through env

```bash
docker run -it --rm -p 8080:8080 logproxy/logproxy
```
