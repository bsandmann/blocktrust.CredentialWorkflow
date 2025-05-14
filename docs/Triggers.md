---
title: Triggers
layout: default
nav_order: 4
has_children: true
---

# Workflow Triggers

Triggers are the starting points for workflows in the Credential Workflow Platform. They define how and when a workflow will be initiated. This section provides a detailed overview of all available trigger types, their configuration options, and best practices for implementation.

## Available Trigger Types

The platform supports the following trigger types:

1. **HTTP Trigger**: Initiate workflows via HTTP requests, ideal for API-driven integrations
2. **Recurring Trigger**: Schedule workflows to run at specific intervals
3. **Manual Trigger**: Execute workflows on-demand through the user interface
4. **Form Trigger**: Collect structured data via forms to trigger workflows
5. **Wallet Interaction Trigger**: Respond to credential requests from wallet applications

## Common Trigger Features

All triggers share certain common features:

- **Input Parameters**: Define the data that will be passed to workflow actions
- **Activation Controls**: Enable or disable the trigger
- **Execution Context**: Provide metadata about how the workflow was initiated
- **Validation Rules**: Ensure input data meets required criteria

## Trigger Selection Guidelines

When selecting a trigger for your workflow, consider:

1. **Initiation Source**: Where does the request to start the workflow come from?
2. **Data Collection**: What data is needed to start the workflow?
3. **Timing Requirements**: When should the workflow run?
4. **Integration Needs**: Which systems need to communicate with the workflow?

## How Triggers Work

When a trigger is activated, it:

1. Collects input data (from an HTTP request, form submission, etc.)
2. Validates the input against defined rules
3. Creates a workflow context containing the input data
4. Initiates the workflow execution process
5. Passes the execution context to the first action in the workflow

For detailed information about each trigger type, refer to the specific documentation pages listed below.