# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Proje dosyasını kopyala ve bağımlılıkları yükle
COPY *.csproj .
RUN dotnet restore

# Tüm dosyaları kopyala ve build et
COPY . .
RUN dotnet publish -c Release -o out

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5052
ENTRYPOINT ["dotnet", "TaskManagerAPI.dll"]