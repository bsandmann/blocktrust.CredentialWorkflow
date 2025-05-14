---
title: HTTP Action
layout: default
parent: Actions
nav_order: 2
---

# HTTP Action Documentation

The `HTTP Action` component enables workflows to make HTTP requests to external services, APIs, and endpoints. This action allows for creating outbound HTTP connections with configurable methods, headers, and body parameters, enabling integration with third-party systems.

## Overview

HTTP integration is a fundamental capability for any workflow system. In the Blocktrust Credential Workflow platform, the HTTP Action enables workflows to:

- Fetch data from external APIs
- Submit data to third-party services
- Trigger external processes
- Validate information through external sources
- Interact with webhook endpoints

This component serves as a bridge between your workflow platform and external web services, allowing for seamless integration with the broader web ecosystem.

## Configuration Options

### HTTP Method

The HTTP method to use for the request:

- **GET**: Retrieve data from the endpoint
- **POST**: Submit data to the endpoint
- **PUT**: Update existing resources at the endpoint
- **DELETE**: Remove resources at the endpoint
- **PATCH**: Partially update resources at the endpoint

The method selection impacts how the request is structured:
- GET and DELETE typically don't include a request body
- POST, PUT, and PATCH typically include a request body

### Endpoint

The URL or API endpoint to send the request to:

1. **Source**: 
   - **Static Value**: Manually enter a specific URL
   - **From Trigger**: Use a URL provided in the trigger parameters
   - **From Previous Action**: Use a URL produced by a previous action in the workflow

2. **Value**: 
   - For static sources: The specific URL to use (e.g., `https://api.example.com/data`)
   - For trigger sources: Select the parameter name that contains the URL
   - For action outcomes: Select the action that produced the URL

The endpoint should be a valid URL with the appropriate protocol (HTTP or HTTPS).

### Headers

HTTP headers to include with the request:

1. **Header Name**: The name of the HTTP header (e.g., `Content-Type`, `Authorization`)
2. **Value Source**:
   - **Static**: A fixed value entered directly
   - **Trigger Input**: Value from a trigger parameter
   - **Action Outcome**: Value from a previous action result
3. **Value**: The content of the header

Common headers you might configure include:
- `Content-Type`: Defines the format of the request body (e.g., `application/json`)
- `Authorization`: Provides authentication credentials
- `Accept`: Specifies the expected response format
- `User-Agent`: Identifies the client making the request

### Request Body (for POST, PUT, PATCH methods)

When using POST, PUT, or PATCH methods, you can configure the request body:

1. **Field Name**: The key in the JSON object to be sent
2. **Value Source**:
   - **Static**: A fixed value entered directly
   - **Trigger Input**: Value from a trigger parameter
   - **Action Outcome**: Value from a previous action result
3. **Value**: The actual content for the field

The request body is constructed as a JSON object with the fields you define.

## How It Works

When a workflow with an HTTP action is executed:

1. The platform resolves the endpoint URL and all parameters
2. It constructs the HTTP request with the specified method, headers, and body
3. The request is sent to the specified endpoint
4. The response is received and processed
5. The response data is stored in the workflow context for potential use by subsequent actions
6. The workflow continues based on the HTTP response

## Technical Details

The HTTP action:

1. Supports all standard HTTP methods (GET, POST, PUT, DELETE, PATCH)
2. Automatically serializes request body fields into JSON for content types that require it
3. Handles URL encoding and query parameter formatting for GET requests
4. Processes HTTP status codes and response headers
5. Supports both synchronous and asynchronous HTTP requests

## Integration with Other Components

This action integrates with:

- **Parameter Resolution System**: To obtain dynamic values from various sources
- **Workflow Context**: Reads input parameters and stores operation results
- **Error Handling**: Provides detailed error information for failed requests

## Use Cases

The HTTP action is ideal for:

1. **Data Retrieval**: Fetch information from external APIs
   - Example: Retrieve user data from a CRM system

2. **External Notifications**: Send notifications to external systems
   - Example: Post credential issuance events to a monitoring service

3. **Validation**: Verify information through third-party services
   - Example: Validate an address through a postal service API

4. **Integration**: Connect workflows to external platforms
   - Example: Submit credential data to a partner system

5. **Webhooks**: Trigger events in external systems
   - Example: Notify an external system when a credential is issued

## Example Configurations

### Basic GET Request

Retrieve data from an API:

1. HTTP Method: GET
2. Endpoint: `https://api.example.com/users/123`
3. Headers:
   - `Accept`: `application/json`
   - `Authorization`: `Bearer {your-api-token}`

### POST Request with JSON Payload

Submit data to an API:

1. HTTP Method: POST
2. Endpoint: `https://api.example.com/credentials/verify`
3. Headers:
   - `Content-Type`: `application/json`
   - `Authorization`: `Bearer {your-api-token}`
4. Body Fields:
   - `credentialId`: Value from a trigger parameter
   - `issuerDid`: Value from a previous action
   - `checkRevocation`: `true` (static)

### PUT Request Updating a Resource

Update an existing resource:

1. HTTP Method: PUT
2. Endpoint: `https://api.example.com/users/123/preferences`
3. Headers:
   - `Content-Type`: `application/json`
4. Body Fields:
   - `notificationEnabled`: `true`
   - `language`: Value from a trigger parameter

## Requirements and Dependencies

For the HTTP action to work properly:

1. The workflow platform must have outbound network access to reach the specified endpoints
2. For HTTPS endpoints, the platform must trust the endpoint's SSL certificates
3. Proper authentication credentials must be provided if the endpoint requires authentication

## Troubleshooting

Common issues and solutions:

1. **Connection Failures**: Ensure the endpoint is reachable and correctly spelled
2. **Authentication Errors**: Verify that the proper authentication headers are included
3. **Request Format Issues**: Check that the body format matches what the endpoint expects
4. **SSL/TLS Errors**: Ensure the endpoint has a valid, trusted SSL certificate
5. **Timeout Errors**: For slow endpoints, check if longer timeouts need to be configured

## Security Considerations

When using the HTTP action:

1. Avoid sending sensitive information like access tokens as query parameters
2. Use HTTPS endpoints rather than HTTP when possible
3. Consider using environment variables or secrets storage for authentication tokens
4. Be aware of data retention policies for external services
5. Validate and sanitize any data received from external sources before using it in subsequent actions