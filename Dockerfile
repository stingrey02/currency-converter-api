FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CurrencyConverterAPI.csproj", "./"]
RUN dotnet restore "CurrencyConverterAPI.csproj"

COPY . .
RUN dotnet publish "CurrencyConverterAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet CurrencyConverterAPI.dll"]
