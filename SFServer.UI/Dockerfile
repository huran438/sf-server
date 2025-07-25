﻿# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install LibMan CLI globally
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli \
    && export PATH="$PATH:/root/.dotnet/tools"

# Ensure LibMan is available in PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy the solution and project files for the UI
COPY SFServer.sln ./
COPY SFServer.UI/*.csproj ./SFServer.UI/
COPY SFServer.Shared/*.csproj ./SFServer.Shared/

# Restore NuGet dependencies
RUN dotnet restore SFServer.UI/SFServer.UI.csproj

# Copy the entire repository into the container
COPY . .

# Run LibMan restore
RUN cd SFServer.UI && libman restore

# Build and publish the UI project
RUN dotnet publish SFServer.UI/SFServer.UI.csproj -c Release -o /app/publish

# Final image: use the ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SFServer.UI.dll"]
