# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and project files for the UI
COPY SecretGameBackend.sln ./
COPY SecretGameBackend.UI/*.csproj ./SecretGameBackend.UI/
COPY SecretGameBackend.Shared/*.csproj ./SecretGameBackend.Shared/

# Restore dependencies for the UI project
RUN dotnet restore SecretGameBackend.UI/SecretGameBackend.UI.csproj

# Copy the entire repository into the container
COPY . .

# Build and publish the UI project
RUN dotnet publish SecretGameBackend.UI/SecretGameBackend.UI.csproj -c Release -o /app/publish

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SecretGameBackend.UI.dll"]
