# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Expose the port the application runs on
# EXPOSE 8080
# EXPOSE 8000
# ENV ASPNETCORE_URLS=https://+;http://+
# ENV ASPNETCORE_HTTPS_PORTS=8001
# ENV ASPNETCORE_HTTP_PORTS=8000

ARG BUILD_CONFIGURATION=Release

# Copy the project files and restore dependencies
COPY . ./
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .


# Run the application
ENTRYPOINT ["dotnet", "NetAPI.dll"]