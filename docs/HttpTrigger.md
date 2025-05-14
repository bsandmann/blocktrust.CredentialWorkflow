---
title: HTTP Trigger
layout: default
parent: Triggers
nav_order: 1
---

# HttpRequestTrigger Component Documentation

The `HttpRequestTrigger` component enables workflows to be triggered via HTTP endpoints. It provides a user interface for configuring HTTP-based triggers with customizable parameters.

## Trigger Types

### Custom HTTP Request

The main HTTP trigger type that allows for complete customization of the HTTP endpoint configuration.

### HTTP Request for Credential Issuance

A specialized template of the Custom HTTP Request preconfigured with parameters relevant for credential issuance workflows. This trigger type uses the same underlying mechanism as the Custom HTTP Request but comes with predefined parameters specific to credential issuance operations.

### HTTP Request for Credential Verification

A specialized template of the Custom HTTP Request preconfigured with parameters relevant for credential verification workflows. Like the issuance template, it uses the same underlying mechanism but with verification-specific predefined parameters.

## Features

- **HTTP Endpoint Generation**: Automatically creates a unique URL endpoint for each workflow
- **HTTP Method Selection**: Supports GET, POST, PUT, and DELETE methods
- **Parameter Management**: Define, edit, and validate custom parameters
- **Schema Generation**: Automatically generates JSON schema for API documentation
- **cURL Example**: Provides ready-to-use cURL commands for testing

## Usage Instructions

### Endpoint URL

The component displays a unique URL endpoint for the workflow in the format:
```
{base-url}/api/workflow/{workflow-id}
```

This URL can be copied to the clipboard using the "Copy URL" button.

### HTTP Method Configuration

Select the appropriate HTTP method (GET, POST, PUT, DELETE) for your trigger. This determines:
- How parameters are passed (query parameters for GET, request body for others)
- Required headers and format for client requests

### Parameter Configuration

Parameters define the data required to trigger the workflow:

1. **Adding Parameters**: Click "Add Parameter" to define new parameters
2. **Parameter Properties**:
   - **Name**: Must start with a letter and contain only alphanumeric characters
   - **Type**: Currently only supports string type (expandable in future)
   - **Description**: Optional field to document parameter purpose

3. **Parameter Validation**:
   - Names must be unique within a trigger
   - Empty names are not allowed
   - Names must match the regex pattern `^[a-zA-Z][a-zA-Z0-9]*$`

### JSON Schema

The component generates a JSON schema documenting the required parameters. This can be copied to clipboard for API documentation purposes.

### cURL Example

A ready-to-use cURL command is automatically generated based on the current configuration:
- For GET requests: Parameters are formatted as query parameters
- For POST/PUT/DELETE: Parameters are formatted as a JSON body

## Technical Implementation

The component:
1. Manages parameter state in a local collection
2. Validates parameter names and uniqueness
3. Updates the parent `TriggerInputHttpRequest` model when changes occur
4. Provides helper functions for clipboard operations
5. Handles parameter editing, deletion, and validation

## Integration with Workflow Execution

When the endpoint is called:
1. The `WorkflowController` validates the request method and parameters
2. Parameter values are extracted from query parameters or request body
3. Parameters are validated against the defined schema (type checking, required fields)
4. If validation passes, the workflow is triggered with the provided parameters