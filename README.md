# User Management API

This is a simple User Management API built with ASP.NET Core. It demonstrates CRUD (Create, Read, Update, Delete) operations for managing users, including validation and middleware for logging, error handling, and authentication.

## Features

- **CRUD Endpoints**: Manage users with GET, POST, PUT, and DELETE endpoints.
- **Middleware**: Includes custom middleware for logging, error handling, and authentication.
- **Validation**: Ensures only valid user data is processed.
- **Logging**: Uses Serilog to log requests, responses, and errors to an external JSON file.

## Technologies Used

- ASP.NET Core
- Serilog
- Swagger (for API documentation)
- Copilot for debugging the code

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/) or any other IDE
- [Git](https://git-scm.com/)

### Installation

1. Clone the repository:
   ```shell
   git clone https://github.com/yourusername/your-repo-name.git
   cd your-repo-name
   ```

## API Endpoints

- **GET /users:** Retrieve all users.

- **GET /users/{id:int}:** Retrieve a specific user by ID.

- **POST /users:** Add a new user. Validates user input.

- **PUT /users/{id:int}:** Update an existing user. Validates user input.

- **DELETE /users/{id:int}:** Delete a user by ID.

## Middleware

- **ErrorHandlingMiddleware:** Handles exceptions and returns a consistent error response.

- **AuthenticationMiddleware:** Validates the authentication token.

- **LoggingMiddleware:** Logs incoming requests and outgoing responses using Serilog.

## Logging

- Logs are saved to a JSON file in the ```logs``` directory, with a rolling interval of one day.
