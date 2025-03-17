# DotNet API Boilerplate

## Overview

This project is a boilerplate for building .NET API applications with various features such as authentication, rate limiting, CORS, and logging using Serilog.

[![.NET](https://github.com/FullstackCodingGuy/DotNet-API-Boilerplate/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/FullstackCodingGuy/DotNet-API-Boilerplate/actions/workflows/dotnet-build.yml)

## Features

- [ ] [Vertical Slicing Architecture](https://github.com/FullstackCodingGuy/Developer-Fundamentals/wiki/Architecture-%E2%80%90-Vertical-Slicing-Architecture)
- [x] Swagger
- [x] Minimal API
- [x] Authentication using JWT Bearer tokens
- [ ] Authorization
- [x] Rate limiting to prevent API abuse
- [x] CORS policies for secure cross-origin requests
- [x] Response caching and compression
- [x] Logging with Serilog
- [x] Health check endpoint
- [x] [Middlewares](https://github.com/FullstackCodingGuy/dotnetapi-boilerplate/tree/main/src/Middlewares)
- [ ] Entity Framework
- [ ] Serilog
- [ ] FluentValidation
- [ ] Vault Integration
- [ ] MQ Integration
- [ ] Application Resiliency
- [ ] Performance
  - [ ] Response Compression
  - [ ] Response Caching 
  - [ ] Metrics
- [ ] Deployment
  - [ ] Docker
  - [ ] Podman
  - [ ] CloudFormation
- [ ] CI
  - [ ] Github Action

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Keycloak](https://www.keycloak.org/) for authentication

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/FullstackCodingGuy/netapi-boilerplate.git
    cd netapi-boilerplate
    ```

2. Install the required NuGet packages:
    ```sh
    dotnet restore
    ```

3. Update the `appsettings.json` and `appsettings.Development.json` files with your configuration.

### Running the Application

1. Build and run the application:
    ```sh
    dotnet run

    or

    dotnet run --launch-profile "https"

    ```

1.1 Setting cert

```
dotnet dev-certs https -ep ${HOME}/.aspnet/https/dotnetapi-boilerplate.pfx -p mypassword234
dotnet dev-certs https --trust

```

Running with Docker

```
// To build the image
docker-compose build

// To run the container
docker-compose up

// To kill container
docker-compose down

```

2. The application will be available at `http://localhost:8000` and `https://localhost:8001` (or the configured URL).

### Health Check Endpoint

The application includes a health check endpoint to verify that the API is running. You can access it at:


```
GET /health

This will return a simple "Healthy" message.
```

### Logging with Serilog

Serilog is configured to log to the console and a file with daily rotation. You can customize the logging settings in the `serilog.json` file.

Example configuration in [Program.cs](http://_vscodecontentref_/1):

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()   // Log to console
    .WriteTo.File("logs/api-log.txt", rollingInterval: RollingInterval.Day) // Log to a file (daily rotation)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();
```


Additional Configuration
- Authentication: Configure the JWT Bearer options in Program.cs to match your Keycloak settings.
- CORS: Update the CORS policies in Program.cs to match your frontend URLs.
- Rate Limiting: Adjust the rate limiting settings in Program.cs as needed.
