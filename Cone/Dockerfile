FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Cone.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM build AS publish
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet publish "Cone.csproj" -c Release -o /app/publish  \
   && dotnet tool install --global dotnet-ef --version 9.0.8 \
   && dotnet ef migrations bundle -o /app/publish/bundle \
   && chmod 755 /app/publish/bundle

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=publish /app/publish/bundle ./bundle
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cone.dll"]