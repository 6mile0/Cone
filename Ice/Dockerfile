FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Ice/Ice.csproj", "./"]
RUN dotnet restore

COPY Ice/. .
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

COPY Ice/appsettings.json ./appsettings.json

EXPOSE 8080
ENTRYPOINT ["dotnet", "Ice.dll"]
