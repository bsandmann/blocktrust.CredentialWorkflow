# Development Report Milestone 1

## Project Structure and Architecture
The project structure was designed to be modular and scalable, following the best practices for ASP.NET Core applications. The solution is currently divided into projects, each responsible for a specific part of the application.
The Web project contains the main application logic, including controllers, views, and services. The Core project is responsible for handling database operations and defining the data models.

### Registration, Authentication, and User Management
The project started with implementing basic user registration and authentication features. We used the ASP.NET Core Identity framework to handle user management, including registration, login, and password reset. 
This decision was made to ensure a secure and reliable user management system that can be easily extended in the future e.g. for external authentication providers or role-based access control.
The user management system was adapted to be used with the PostgreSQL database, which was chosen as the primary database for the project according to the specifications from the proposal.

### Tenant Management
The project includes a basic tenant management system that allows users to be part of tenants. Each tenant can have its own set of users and settings, making it possible to create isolated environments for different organizations or projects.
The tenant is also responsible for managing the connection to the different Identus cloud agents, which will be used for issuing and verifying credentials at a later stage of the project.

### Workflow
The project includes a basic workflow system that allows users to create and manage workflows. A workflow consists of a set of triggers and actions that define the logic of the workflow. Triggers listen for specific events or inputs, such as HTTP requests or DIDComm messages, while actions process and validate inputs e.g. including DID resolution, credential verification, and issuing or revoking credentials.
A user can create and manage those workflows through the web interface, making it possible to automate various processes based on SSI credentials. A workflows also be written manually in JSON format, allowing for more complex and custom workflows. Those workflows can be shared and imported. A dedicated JSON schema was created to ensure the validity of the workflows.

### Designer
The project includes a simple visual designer that allows users to create and manage workflows. While the current implementation supports only linear workflows, the designer can be extended to support more complex workflows in the future e.g. through the use of conditional branches or maybe even loops.

### Technical decisions
Besides .Net Core and PostgreSQL, the project uses Entity Framework Core as the ORM. It is heavily based on the Command-Pattern using the MediatR library. The project also uses the FluentResults to handle the results of the commands. 
For the frontend, the project is using Blazor Server (interactive rendermode) for the workflow management and server-rendered Razor Pages for the user management. The project also uses the Tailwind CSS framework for styling.

## Next Steps
In the first milestone we mainly focused on setup up a structure and a user interface which can accomodate the implementation of the next features ie. the implementation of the code to run through an actual example workflow in code. 

