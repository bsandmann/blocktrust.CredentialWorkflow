---
title: Getting Started Guide
layout: default
nav_order: 2
---

# Getting Started Guide

When first working with the Credential Workflow Platform you have two options of running the platform:
- Either using a hosted instance of the platform, which is available at [https://workflow.blocktrust.dev](https://workflow.blocktrust.dev/)
- Or running the platform locally on your own machine / or host it yourself on a server.

The choice of which option to use depends on your needs and trust-requirements. Using the hosted platform naturally implies that nearly all information is accessible to the provider. When usign the platform for Http Request and Credential Validation is might be less critical as when usign the platform for creating DIDs and iussing credentials as the credentials would be exposed to the provider.

## Using a hosted instance

You can access the hosted instance of the Credential Workflow Platform at [https://workflow.blocktrust.dev](https://workflow.blocktrust.dev/). 
This instance is pre-configured and ready to use. You can create an account and start building workflows immediately. 
The platform has itself no restrictions of the network ie. you can use it with DIDs and credentials on the **preprod** or **mainnet** network.
However this being hosted, we would advice the platform only for testing and development purposes on the preprod network.
For a more production ready solution we would recommend to run the platform on your own server.

## Installation and Setup locally / on your own server 

The platform can be run as a docker compose file, which includes a postgres database and the platform itself.
You can find the docker compose file in the root directory of the repository. 
The image can be found here [https://github.com/users/bsandmann/packages/container/package/workflowplatform](https://github.com/users/bsandmann/packages/container/package/workflowplatform)

```yaml
services:
  credential-workflow-web:
    image: ghcr.io/bsandmann/workflowplatform:latest
    container_name: credential-workflow-web
    ports:
      - "8080:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://0.0.0.0:8080

      AppSettings__PrismBaseUrl: https://opn.mainnet.blocktrust.dev
      AppSettings__PrismDefaultLedger: mainnet
      AppSettings__PrismBaseUrlFallback: https://opn.preprod.blocktrust.dev
      AppSettings__PrismDefaultLedgerFallback: preprod

      ConnectionStrings__DefaultConnection: Host=postgres;Username=postgres;Password=postgres;Database=workflowdatabase

      EmailSettings__SendGridKey: <YOUR_SENDGRID_KEY>
      EmailSettings__SendGridFromEmail: <YOUR_FROM_EMAIL>
      EmailSettings__DefaultFromName: Credential Workflow Platform
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - wp

  postgres:
    image: postgres:15
    container_name: credential-workflow-postgres
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: workflowdatabase
    volumes:
      - postgres_data:/var/lib/postgresql/data
    expose:
      - "5432"
    networks:
      - wp
    healthcheck:
      # Simpler pg_isready, assumes it's run against the local server
      test: ["CMD-SHELL", "pg_isready -U postgres -d workflowdatabase"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:

networks:
  wp:
    driver: bridge

```

## Building the application

When you want to build the application locally, you can also do so:

   - Install the .net 9 SDK
   - Clone the repository from [https://github.com/bsandmann/blocktrust.CredentialWorkflow](https://github.com/bsandmann/blocktrust.CredentialWorkflow) which is the main repo of the platform. 
   - Additionally you need to clone the following repositories:
        - The Blocktrust Mediator project: [https://github.com/bsandmann/blocktrust.Mediator](https://github.com/bsandmann/blocktrust.Mediator)
        - The Blocktrust DIDComm project: [https://github.com/bsandmann/blocktrust.DIDComm](https://github.com/bsandmann/blocktrust.DIDComm)
        - The Blocktrust Core project: [https://github.com/bsandmann/blocktrust.Core](https://github.com/bsandmann/blocktrust.Core)
        - The Blocktrust PeerDID project: [https://github.com/bsandmann/blocktrust.PeerDID](https://github.com/bsandmann/blocktrust.PeerDID)
        - The Blocktrust Credentials project: [https://github.com/bsandmann/blocktrust.VerifiableCredential](https://github.com/bsandmann/blocktrust.VerifiableCredential)
        - The DidPrismResolverClient: [https://github.com/bsandmann/DidPrismResolverClient](https://github.com/bsandmann/DidPrismResolverClient)
   - You should for one now be able to build you own version of the platform. Either by:
        - Building the image by running the Dockerfile (found inside the the root directory of the CredentialWorkflow.Web project) at the root of the all the cloned projects, or
        - by running`dotnet build` in the root directory of the project of the CredentialWorkflow.Web project.
   - Configure the connection string in `appsettings.json`
   - Run database migrations
   - Start the application

## Creating Your First Workflow

1. **Access the Workflow Designer**
   - Navigate to the Workflows page
   - Click "Create New" to open the workflow designer

2. **Select a Trigger**
   - Choose how the workflow will be initiated:
     - HTTP trigger for API-based activation
     - Recurring trigger for scheduled execution
     - Manual trigger for user-initiated runs
     - Form trigger for data collection
     - Wallet interaction trigger for credential exchange

3. **Add Actions**
   - Build a sequence of actions that will execute when the trigger fires
   - Common first workflows include:
     - Issuing a simple credential
     - Validating an incoming credential
     - Sending a notification email

4. **Configure Parameters**
   - Set up data flow between trigger inputs and action parameters
   - Use parameter substitution with {% raw %}`{{paramName}}`{% endraw %} syntax

5. **Activate and Test**
   - Save the workflow
   - Activate it from the Workflow Overview page
   - Test with sample data

For a more indepth explanation see the [User Guides](UserGuides/index.md) section.:
- [Creating and Updating DIDs](UserGuides/CreatingAndUpdatingDids.md)
- [Issuing Credentials](UserGuides/IssuingCredentials.md)
- [Verifying Credentials](UserGuides/VerifyingCredentials.md)

For additional support, contact the Blocktrust team or consult the GitHub repository for known issues and solutions.