# Blocktrust.CredentialWorkflow Development Guidelines

## Project Structure
- **Blocktrust.CredentialWorkflow.Core**: Contains business logic, database operations, and workflow execution engine
- **Blocktrust.CredentialWorkflow.Web**: Contains UI components, controllers, and presentation layer
- Other projects are supporting libraries that typically don't need to be modified

## Application Concept
This application creates IFTTT-like workflows for credential management:
- Each workflow belongs to a tenant and consists of one trigger and multiple sequential actions
- Workflows can be executed via triggers, timers, or manually
- ProcessFlow class defines workflow structure and is serialized to disk for persistence
- Core is organized around Command pattern using MediatR for business operations

## Build Commands
- Build solution: `dotnet build blocktrust.CredentialWorkflow.sln`
- Run web project: `dotnet run --project Blocktrust.CredentialWorkflow.Web/Blocktrust.CredentialWorkflow.Web.csproj`
- Run all tests: `dotnet test`
- Run specific test: `dotnet test --filter "FullyQualifiedName=Namespace.TestClassName.TestMethodName"`
- Run specific test class: `dotnet test --filter "FullyQualifiedName~TestClassName"`

## Code Style Guidelines
- Use C# naming conventions: PascalCase for classes/methods, camelCase for variables
- All handler classes should follow Command pattern with Request/Response objects
- Use async/await for asynchronous operations with proper cancellation token propagation
- Use FluentResults for operation results with success/failure patterns
- Write xUnit tests with Arrange-Act-Assert pattern
- Prefer explicit type declarations over `var` for better readability
- Follow Entity Framework Core conventions for data access
- Organize code by feature folders rather than by technical concerns