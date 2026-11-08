# RaceDay API

## 1. Project Overview

RaceDay API is an ASP.NET Core Web API developed to support the management of running and walking events. The system provides functionality for user authentication, event management, event types, categories, participant enrolments and event results.

The API uses a SQL Server database and Entity Framework Core to manage data. Swagger UI is used to document and test the available API endpoints.

## 2. Technologies Used

- C#
- ASP.NET Core Web API
- .NET 8
- Microsoft SQL Server Express
- Entity Framework Core
- Swagger / OpenAPI
- JSON Web Tokens (JWT)
- ASP.NET Core Identity password hashing
- Git and GitHub
- link for Youtube https://youtu.be/oy0R65voBdM?si=1ZxrmZdB1h4-VRJ3  

## 3. Main Features

The API includes the following functional areas:

- **User Authentication:** User registration and login, with password hashing.
- **User Management:** User-related functionality.
- **Event Management:** Retrieve and manage event information.
- **Event Types:** Manage and retrieve event type information.
- **Categories:** Manage event categories.
- **Enrolments:** Support participant enrolment functionality.
- **Results:** Manage and retrieve event results.
- **API Documentation:** Swagger UI allows endpoints to be explored and tested.

## 4. User Roles

The intended system roles are:

- **Organiser:** Responsible for managing events and related information.
- **Participant:** Able to view events and use participant-related functionality, such as enrolment and viewing results.

Access to restricted operations must be controlled through role-based authorisation.

## 5. Prerequisites

Before running the project, ensure that you have installed:

- Visual Studio 2022 or a compatible IDE.
- .NET 8 SDK.
- Microsoft SQL Server Express.
- SQL Server Management Studio (SSMS), if required for database management.
- Git, if cloning the repository from GitHub.

## 6. Database Configuration

The project uses SQL Server Express with a database named `RaceDayDB`.

The connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "RaceDayConnection": "Server=(local)\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ensure that SQL Server Express is running and that the database exists before starting the API.

The application also uses JWT configuration for token generation and validation. Keep development keys out of public repositories in production environments.

## 7. How to Run the Project

1. Clone the repository or open the existing project in Visual Studio.
2. Open the solution or `RaceDay2.API.csproj` file.
3. Confirm that the database connection string in `appsettings.json` matches your SQL Server configuration.
4. Ensure the `RaceDayDB` database and required tables are available.
5. Restore the NuGet packages.
6. Build the solution and resolve any build errors.
7. Run the project using Visual Studio.
8. Open Swagger UI in your browser:

   `https://localhost:7066/swagger/index.html`

The port may differ depending on your local launch settings.

## 8. Testing the API

The API can be tested through Swagger UI.

1. Open the relevant endpoint.
2. Select **Try it out**.
3. Enter the required request data.
4. Select **Execute**.
5. Review the HTTP response code and response body.

Testing should cover successful requests, invalid login credentials, missing or invalid data, and access restrictions for different user roles.

Automated unit tests should also be run where provided.

## 9. Authentication and Security

Passwords are hashed before being stored in the database. The login endpoint verifies the supplied password against the stored password hash.

The API is configured to generate and validate JWT authentication tokens. Protected endpoints must use appropriate authorisation attributes to enforce access restrictions.

The assignment requirements for session management and role-based access should be verified against the implemented authentication mechanism.

## 10. Repository and Continuous Integration

The source code is maintained in GitHub. Version control is used to track development changes.

Where configured, GitHub Actions can be used to build the application and execute automated tests when changes are pushed to the repository.

The latest workflow result and test results should be checked before submission.

## 11. Conclusion

The RaceDay API provides a backend foundation for managing events, users, categories, enrolments and results. It uses ASP.NET Core Web API, SQL Server and Entity Framework Core, with Swagger supporting API testing and documentation.

Further verification of authentication, role-based authorisation, automated testing and deployment requirements should be completed according to the project specification.
