# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyChatBackend.csproj", "./"]
RUN dotnet restore "./MyChatBackend.csproj"

# Copy and publish app
COPY . .
RUN dotnet publish "./MyChatBackend.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyChatBackend.dll"]
