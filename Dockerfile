# -------------------------------
# Build step
# -------------------------------
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out src/NModbus.DeviceSimulator/NModbus.DeviceSimulator.csproj

# -------------------------------
# Runtime image
# -------------------------------
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "NModbus.DeviceSimulator.dll"]