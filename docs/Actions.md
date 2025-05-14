---
title: Actions
layout: default
nav_order: 5
has_children: true
---

# Workflow Actions

Actions are the building blocks of workflows in the Credential Workflow Platform. They define the operations that will be performed when a workflow executes. This section provides a comprehensive overview of all available action types, their configuration options, and best practices for implementation.

## Action Categories

The platform offers actions across several functional categories:

1. **Credential Operations**
   - Issue W3C Credentials
   - Verify W3C Credentials
   - Validate Credential Structure

2. **Communication**
   - Send Emails
   - Make HTTP Requests
   - DIDComm Messaging

3. **Identity Management**
   - Create DIDs
   - Update DIDs
   - Deactivate DIDs

4. **Security & Authentication**
   - JWT Token Generation
   - Custom Validation

## Action Execution Flow

When a workflow runs, actions:

1. Receive input from the trigger or previous actions
2. Process the data according to their configuration
3. Produce an outcome (success or failure)
4. Store output data for use by subsequent actions
5. Pass control to the next action in the sequence

## Action Configuration

Each action has specific configuration parameters, but most share these common elements:

- **Input Parameters**: Define what data the action will use
- **Parameter Mapping**: Connect data from triggers or previous actions
- **Success Conditions**: Define what constitutes successful execution
- **Error Handling**: Specify how errors should be managed

## Action Output

Actions produce structured output that:

- Is stored in the workflow execution context
- Can be referenced by subsequent actions using parameter substitution
- Is available in workflow logs for debugging and monitoring
- May be returned in API responses for HTTP-triggered workflows

## Parameter Substitution

Actions support dynamic parameter values through substitution syntax:

- Use `{{paramName}}` to reference trigger inputs
- Use `{{actionId.outputField}}` to reference outputs from previous actions
- Complex nested values can be accessed using dot notation

For detailed information about each action type, refer to the specific documentation pages listed below.