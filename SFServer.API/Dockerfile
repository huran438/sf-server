# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and project files
COPY SecretGameBackend.sln ./
COPY SecretGameBackend.API/*.csproj ./SecretGameBackend.API/
COPY SecretGameBackend.Shared/*.csproj ./SecretGameBackend.Shared/

# Restore dependencies
RUN dotnet restore SecretGameBackend.API/SecretGameBackend.API.csproj

# Copy the rest of the repository
COPY . .

# Build and publish the API project
RUN dotnet publish SecretGameBackend.API/SecretGameBackend.API.csproj -c Release -o /app/publish

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Listen on port 8080 on all network interfaces inside the container
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "SecretGameBackend.API.dll"]
