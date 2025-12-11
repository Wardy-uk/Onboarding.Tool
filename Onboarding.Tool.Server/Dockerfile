# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
EXPOSE 8080

# Copy solution and project files
COPY ["Onboarding.Tool.Server/Onboarding.Tool.Server.csproj", "Onboarding.Tool.Server/"]
COPY ["Onboarding.Tool.Model/Onboarding.Tool.Model.csproj", "Onboarding.Tool.Model/"]

# Restore dependencies for backend projects only
RUN dotnet restore "Onboarding.Tool.Server/Onboarding.Tool.Server.csproj"

# Copy all source code (backend only)
COPY ["Onboarding.Tool.Server/", "Onboarding.Tool.Server/"]
COPY ["Onboarding.Tool.Model/", "Onboarding.Tool.Model/"]

WORKDIR /src/Onboarding.Tool.Server

# Build the backend project
RUN dotnet publish "Onboarding.Tool.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Onboarding.Tool.Server.dll"]
