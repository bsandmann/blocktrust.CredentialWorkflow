---
title: Configuration 
layout: default
parent: Settings
nav_order: 5
---

# Configuration Settings

This document provides details about the main configuration settings for the Blocktrust Credential Workflow platform.

## App Settings

The following settings can be configured in the `appsettings.json` file or via environment variables:

### Prism DID Network Configuration

#### PrismBaseUrl

- **Purpose**: Defines the primary base URL for the Prism DID network node.
- **Default**: `https://opn.mainnet.blocktrust.dev`
- **Required**: Yes
- **Example Usage**: Used when creating or resolving DIDs on the primary network.

#### PrismDefaultLedger

- **Purpose**: Specifies the primary ledger to use with the Prism DID network.
- **Default**: `mainnet`
- **Required**: Yes
- **Example Usage**: Used to determine which ledger to anchor DIDs to when creating new DIDs.

#### PrismBaseUrlFallback

- **Purpose**: Defines a fallback base URL for the Prism DID network when the primary endpoint is unavailable or another network should be checked.
- **Default**: `https://opn.preprod.blocktrust.dev`
- **Required**: No
- **Example Usage**: Automatically used if primary network requests fail.

#### PrismDefaultLedgerFallback

- **Purpose**: Specifies the fallback ledger to use with the Prism DID network.
- **Default**: `preprod`
- **Required**: No
- **Example Usage**: Used with the fallback base URL when the primary network is unavailable.

### Email Settings

Email settings are required for sending notifications and credentials via email.

#### SendGridKey

- **Purpose**: API key for the SendGrid email service.
- **Default**: None, must be provided in production.
- **Required**: Yes if email functionality is used.
- **Example**: `SG.xxxxxxxxxxxxxxxxxxxxxxxx`

#### SendGridFromEmail

- **Purpose**: Email address used as the sender for all outgoing emails.
- **Default**: None, must be provided.
- **Required**: Yes if email functionality is used.
- **Example**: `verified-sender@yourdomain.com`

## Overriding Settings in Docker Compose

When deploying the application using Docker Compose, you can override these settings by specifying environment variables:

```yaml
version: '3.8'
services:
  credential-workflow:
    image: blocktrust/credential-workflow:latest
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres; Database=WorkflowDatabase; Username=postgres; Password=securepassword
      - AppSettings__PrismBaseUrl=https://opn.mainnet.blocktrust.dev
      - AppSettings__PrismDefaultLedger=mainnet
      - AppSettings__PrismBaseUrlFallback=https://opn.preprod.blocktrust.dev
      - AppSettings__PrismDefaultLedgerFallback=preprod
      - EmailSettings__SendGridKey=your-sendgrid-api-key
      - EmailSettings__SendGridFromEmail=verified-sender@yourdomain.com
      - EmailSettings__DefaultFromName=Your Organization Name
    ports:
      - "5000:80"
    depends_on:
      - postgres
    
  postgres:
    image: postgres:latest
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=securepassword
      - POSTGRES_DB=WorkflowDatabase
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:
```

### Environment Variable Naming Convention

- Use double underscores `__` to represent nesting in JSON.
- The .NET configuration system automatically maps these environment variables to the application settings.