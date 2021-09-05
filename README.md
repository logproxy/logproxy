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

Following either of the build steps yields an artifact directory, which contains the build artifacts. Also there is a docker image.

#### Run LogProxy locally

Please verify that the AirTable apikey is set and correct the `appsettings.json`. Alternatively, environment variables can be used (see below). In the same fashion the user/password for the basic authentication can be set.

```bash
cd ./artifacts
dotnet LogProxy.dll
```

#### Run LogProxy docker image

When using the docker image, pass in the apikey through an env variable as shown below. In the same fashion, other configuration entries can be set. This includes the bind point (including port) and user/password

```bash
docker run -it --rm --env "AirTableConfig__ApiKey=__set_me__" -p 8080:8080 logproxy/logproxy
```

### Example

Given that logproxy is running. Example get request

```bash
# get
curl --basic -u user:password http://localhost:8080/messages | jq
# post two titleAndText elements
curl -v --basic -u user:password -H "Content-Type: application/json" -X POST --data '[{"title":"newtitle1","text":"newtext1"},{"title":"newtitle2","text":"newtext2"}]' http://localhost:8080/messages
```

### Some Remarks

* currently, the basic auth only checks for one user, which can be configured through the settings. but this can easily be changed to account for a real world setting
* currently, the post does not accept a single title/text struct/element. to me this was more in line with the examples in the description