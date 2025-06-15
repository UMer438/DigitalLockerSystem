# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all files and restore dependencies
COPY . .
RUN dotnet restore "DigitalLockerSystem.csproj"

# Publish the app
RUN dotnet publish "DigitalLockerSystem.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .

# Copy configuration and database (if exists locally)
COPY appsettings.json .
COPY DigitalLockerSystem.db .

# Expose the port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "DigitalLockerSystem.dll", "--urls", "http://0.0.0.0:80"]
