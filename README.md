# MotoRentalService API

Welcome to the **MotoRentalService API** repository! This application provides an API for managing motorcycle rentals, including motorcycle registration, user management, rental cost calculations, and more.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Endpoints](#endpoints)
- [Additional Features](#additional-features)
  - [Docker](#docker)
  - [Cache](#cache)
  - [Testing](#testing)
- [Contributing](#contributing)

## Installation

To set up the project locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/username/new-repo.git
   cd MotoRentalServiceAPI
   
2. Restore the project's dependencies:
   ```bash
   dotnet restore

3. Build the solution:
   ```bash
   dotnet build

4. Run script for postgres database:
   [script.sql](./script.sql)

## Configuration

Ensure that you configure the necessary environment variables and connection strings in the [appsettings.json](./MotoRentalService.Api/appsettings.json) file or via environment variables.

## Usage

1. To run the application, use the following command:
   ```bash
   dotnet run

2. For Development with Swagger:

 - To interact with the API using Swagger during development, please follow these steps:
  
   - Generate a token using the appropriate role.

     - Admin: For administrative tasks and access.

     - User: For standard user operations.

   - Store the generated token in the "Authorize" field within the Swagger interface.

     ![swagger_token](https://github.com/user-attachments/assets/f446d1cf-0957-48f0-80e0-90d260da1bde)

  - This will include the token in the header of subsequent requests, allowing you to authenticate and interact with the API endpoints effectively.

## Endpoints

For detailed information about the available endpoints, please refer to the [requests](./MotoRentalService.Api/Requests/) examples. Alternatively, you can run the application in development mode and use Swagger UI to interact with the API and explore its endpoints.

## Additional Features

### Docker

The application is configured to run in Docker containers, facilitating deployment and consistent execution across different environments.

#### How to Use Docker

1. Ensure Docker is installed and running.

2. Build the Docker image.

3. Run the container.

### Cache

The application uses caching to improve the performance of read operations. The cache is configured in Startup.cs and can be adjusted as needed for different usage scenarios.

### Testing

The application includes unit tests to ensure code quality and functionality.

#### Running the Tests

1. To run the tests, use the following command:
   ```bash
   dotnet test 

## Contributing
If you would like to contribute to the project, please follow these steps:

Fork this repository.
Create a branch for your feature (git checkout -b feature/new-feature).
Commit your changes (git commit -am 'Add new feature').
Push to the branch (git push origin feature/new-feature).
Create a Pull Request.
