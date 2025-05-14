---
title: Getting Started Guide
layout: default
nav_order: 3
---

# Getting Started Guide

Welcome to the Blocktrust Credential Workflow Platform getting started guide. This document will walk you through the essential steps to begin using the platform, from initial setup to creating your first workflow.

## Prerequisites

Before you begin, ensure you have the following:

- A Blocktrust account with appropriate access permissions
- Basic understanding of verifiable credentials and decentralized identity concepts
- Familiarity with workflow automation principles

## Installation and Setup

Follow these steps to set up the Credential Workflow Platform:

1. **System Requirements**
   - .NET 8.0 or later
   - PostgreSQL database
   - Web server for hosting (optional for local development)

2. **Installation**
   - Clone the repository
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
   - Use parameter substitution with `{{paramName}}` syntax

5. **Activate and Test**
   - Save the workflow
   - Activate it from the Workflow Overview page
   - Test with sample data

## Example Workflow: Credential Issuance

Here's a simple example of a workflow that issues a credential when triggered:

1. **Trigger**: HTTP trigger accepting POST requests with applicant information
2. **Actions**:
   - Validate the incoming data using custom validation rules
   - Create a W3C Verifiable Credential with the applicant information
   - Sign the credential using your organization's DID
   - Return the credential in the HTTP response

## Next Steps

After creating your first workflow, explore these advanced features:

- Using templates for common workflow patterns
- Implementing multi-step credential exchanges
- Setting up recurring credential verification
- Integrating with external systems via HTTP actions
- Customizing credential templates

Refer to the specific documentation sections for detailed guidance on each feature.

## Troubleshooting

If you encounter issues during setup or workflow execution:

- Check the application logs for detailed error messages
- Ensure your database connection is properly configured
- Verify that all required services are accessible
- Review the workflow logs for specific execution failures

For additional support, contact the Blocktrust team or consult the GitHub repository for known issues and solutions.