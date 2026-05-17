FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ZenBudget.Api.csproj", "./"]
RUN dotnet restore "./ZenBudget.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "./ZenBudget.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes and binds to port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ZenBudget.Api.dll"]
