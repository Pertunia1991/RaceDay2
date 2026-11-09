
# RaceDay – Race Event Management System

## 1. Project Overview

RaceDay is a web-based race event management system developed as part of the PROG6212 Programming 2B Practical Project.

The system provides a platform for managing sporting events and supporting the interaction between event organisers and participants.

Part 3 extends the system developed in Parts 1 and 2 by introducing an ASP.NET Core MVC web application that communicates with the RESTful API developed in Part 2.

The application uses a role-based approach to provide functionality for Organisers and Participants.

## 2. Technologies Used

The project uses the following technologies:

- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- SQL Server
- HTML5
- CSS3
- Bootstrap
- JavaScript
- Docker
- GitHub and GitHub Actions
- Azure Blob Storage (for image storage, where configured)

## 3. System Architecture

RaceDay follows a layered architecture.

The MVC application acts as the front-end interface through which users interact with the system.

The MVC application communicates with the RESTful API, which handles application operations and database communication.

The system architecture can be summarised as follows:

User
  |
  v
RaceDay MVC Web Application
  |
  v
RaceDay RESTful API
  |
  v
SQL Server Database

Image uploads are handled through the API and Azure Blob Storage integration.

The MVC application does not directly access the database.

## 4. User Roles

### 4.1 Organiser

The Organiser area is intended to support race event administration, including:

- Access to the Organiser dashboard.
- Event management.
- Creating, updating and deleting events.
- Managing event categories.
- Viewing participant enrolments.
- Capturing race results.

### 4.2 Participant

The Participant area supports users who wish to participate in sporting events.

Participant functionality includes:

- Viewing available events.
- Viewing event information.
- Entering events.
- Viewing personal event enrolments.
- Viewing race results.
- Accessing and updating profile information.

## 5. Project Structure

The solution contains the API and MVC application projects.

Main project components include:

- **RaceDay2.API** – RESTful API responsible for application operations and data access.
- **RaceDay.MVC** – ASP.NET Core MVC web application providing the user interface.
- **Models** – Application data and view models.
- **Controllers** – Handle application requests and navigation.
- **Views** – Razor views used to display the application pages.
- **Services** – Handle communication between the MVC application and API.

## 6. Prerequisites

The following software is required to run the application:

- Visual Studio 2022.
- .NET 8 SDK.
- SQL Server.
- Docker Desktop.
- Git.

Docker Desktop must be running before executing Docker commands.

## 7. Database Configuration

The API requires a configured SQL Server database.

Before running the application:

1. Ensure SQL Server is installed and running.
2. Configure the database connection string in the API application's configuration.
3. Ensure the database is created and the required tables are available.
4. Do not commit database passwords or other sensitive connection details to GitHub.

The MVC application communicates with the API rather than accessing SQL Server directly.

## 8. Running the API

The API must be running before accessing the MVC application.

From PowerShell, execute:

```powershell
dotnet run --project "C:\Users\Student\source\repos\RaceDay2\RaceDay2.API.csproj" --urls "http://0.0.0.0:5000"
```

The API will be available at:

`http://localhost:5000`

Keep the API terminal window open while using the MVC application.

## 9. Running the MVC Application Locally

Open the RaceDay MVC project in Visual Studio.

Set RaceDay.MVC as the startup project and run the application.

The MVC application communicates with the API using the configured API base URL.

Ensure the API is running before testing MVC functionality.

## 10. Authentication and Session Management

The application includes registration and login functionality.

The MVC application communicates with the API for authentication and uses session information to manage the logged-in user's details and role.

The intended role-based navigation provides separate areas for Organisers and Participants.

Users can log out to end their application session.

## 11. Organiser Functionality

The Organiser interface provides event administration functionality, including event management, category management, participant enrolments and results administration.

## 12. Participant Functionality

The Participant interface provides access to event browsing, event information, event enrolment, personal event records, results and profile information.

## 13. Azure Blob Storage

Azure Blob Storage is used for image storage through the API integration.

The intended upload process is:

1. The user selects an image through the MVC interface.
2. The MVC application sends the image to the API.
3. The API handles the upload to Azure Blob Storage.
4. The resulting image reference is stored with the relevant application record.
5. The image can subsequently be displayed in the application.

Azure storage credentials must be configured securely and must not be committed to the public repository.

## 14. Docker Deployment

The MVC application is containerised using Docker.

The API runs on the Windows host machine, while the MVC application runs inside the Docker container.

### 14.1 Build the Docker Image

Open PowerShell and navigate to the MVC project directory:

```powershell
cd "C:\Users\Student\source\repos\RaceDay.MVC"
```

Build the image:

```powershell
docker build -f Dockerfile -t raceday-mvc:part3 .
```

### 14.2 Run the API

Start the API on the host machine using the command in Section 8.

The API must remain running.

### 14.3 Run the MVC Container

Execute:

```powershell
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc:part3
```

The `host.docker.internal` address allows the MVC container to communicate with the API running on the Windows host.

### 14.4 Access the Application

Open a browser and navigate to:

`http://localhost:8080`

### 14.5 Verify the Container

To confirm that the MVC container is running:

```powershell
docker ps
```

To view the container logs:

```powershell
docker logs raceday-mvc
```

To stop the container:

```powershell
docker stop raceday-mvc
```

To remove the container:

```powershell
docker rm raceday-mvc
```

## 15. Continuous Integration and GitHub Actions

GitHub is used for source-code management and version control.

GitHub Actions is used to automate the build and validation process.

The workflow is intended to restore dependencies, build the solution and execute the available automated tests.

The latest successful GitHub Actions workflow execution is included as submission evidence below.

## 16. GitHub Actions Evidence

![GitHub Actions Successful Build](docs/images/github-actions-success.png)

*Replace this image with a screenshot of your actual successful GitHub Actions workflow.*

## 17. Docker Evidence

Docker is used to package and run the RaceDay MVC application in a container.

The Docker image is named:

`raceday-mvc:part3`

The running container is named:

`raceday-mvc`

The application is accessed through:

`http://localhost:8080`

## 18. Video Demonstration

The demonstration video presents the RaceDay application, its functionality and the deployment process.

Video link:

[Insert your demonstration video link here]

## 19. GitHub Repository

GitHub repository:

[Insert your GitHub repository link here]

## 20. Conclusion

RaceDay Part 3 extends the existing system by providing an MVC web application that communicates with the Part 2 RESTful API.

The project brings together web application development, API integration, role-based functionality, database communication, image storage integration, version control and Docker deployment.

The completed system demonstrates the application of software development concepts covered in PROG6212 Programming 2B.
