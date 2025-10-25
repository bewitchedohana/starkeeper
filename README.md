# Starkeeper

Starkeeper is a modern bookmarking platform built with microservices architecture. It enables users to efficiently manage and organize their bookmarks through a clean and intuitive interface. The platform features robust user authentication and a well-structured API design.

Key Features:
- Secure user authentication and authorization
- Organized bookmark management
- Clean and intuitive web interface built with Angular
- RESTful API architecture
- Containerized development environment

## Project Overview

The project consists of several microservices:

- **Identity Service**: Handles user authentication and authorization
- **Bookmarks Web App**: Frontend application for managing bookmarks

## Technology Stack

- Backend:
  - .NET 9.0
  - PostgreSQL
  - Docker
- Frontend:
  - Angular (Web Application)
  - TypeScript
  - Angular Material UI
  - RxJS

## Development Setup

### Prerequisites

- Docker Desktop
- Visual Studio Code
- Dev Containers extension for VS Code
- .NET 9.0 SDK

### Getting Started with Dev Containers

This project uses Docker Compose for development environments. The `compose.dev.yaml` file is configured to provide a consistent development experience across different machines.

1. Clone the repository with submodules:
```bash
git clone --recursive https://github.com/yourusername/starkeeper.git
cd starkeeper
git submodule update --init
```

2. Set up secrets:
> **Note**: The `secrets` submodule contains development configuration and sensitive data. You need proper access rights to this repository.
```bash
# Create symbolic link for Docker Compose
ln -s secrets/.env ./.env
```

3. Start the development environment:
```bash
docker compose -f compose.dev.yaml up -d --build
```

This will start:
- Identity Service (http://localhost:5000)
- PostgreSQL Database
- Other required services

3. Open the project in VS Code with Dev Containers:
   - Open VS Code
   - Install the "Dev Containers" extension if you haven't already
   - Press F1 and select "Dev Containers: Open Folder in Container"
   - Select the project folder

### Development Workflow

The development environment is configured for hot reload:
- Make changes to the code
- The services will automatically rebuild and restart
- Access the services through their respective ports:
  - Identity Service: http://localhost:5000
  - Database: localhost:5432

### Environment Variables

The following environment variables are configured in the development environment:

```yaml
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:80
```

### Database Connections

The Identity Service is configured to connect to PostgreSQL with these default settings:
- Host: identity-database
- Port: 5432
- Database: identity_service
- Username: postgres
- Password: starkeeper

### Debugging

1. The development container includes debugging tools
2. VS Code launch configurations are provided
3. Set breakpoints in your code and start debugging

### Useful Commands

```bash
# Rebuild services
docker compose -f compose.dev.yaml up -d --build

# View logs
docker compose -f compose.dev.yaml logs -f

# Stop services
docker compose -f compose.dev.yaml down

# Access a container's shell
docker exec -it starkeeper_identity_service /bin/bash
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request

## Architecture

The project follows a microservices architecture:

```
Starkeeper/
├── Identity Service (Authentication & Authorization)
│   ├── Domain
│   ├── Application
│   ├── Infrastructure
│   └── Presentation
└── Bookmarks Web App (Frontend)
```