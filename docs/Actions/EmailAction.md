---
title: Email Action
layout: default
parent: Actions
nav_order: 1
---

# Email Action Documentation

The `Email Action` component enables workflows to send email notifications with customizable content and dynamic parameters. This action allows for creating personalized email communications to recipients based on workflow data and trigger inputs.

## Overview

Email notifications are a critical component of many digital credential workflows. In the Blocktrust Credential Workflow platform, the Email Action enables workflows to:

- Send notifications about credential status changes
- Deliver credentials directly to recipients
- Alert administrators about workflow events
- Provide instructions to users

This component leverages SendGrid as the email delivery service, ensuring reliable delivery and tracking capabilities.

## Configuration Options

### To Email

The recipient's email address:

1. **Source**: 
   - **Static Value**: Manually enter a specific email address
   - **From Trigger**: Use an email address provided in the trigger parameters
   - **From Previous Action**: Use an email address produced by a previous action in the workflow

2. **Value**: 
   - For static sources: The specific email address to use (e.g., `recipient@example.com`)
   - For trigger sources: Select the parameter name that contains the email address
   - For action outcomes: Select the action that produced the email address

The email address must be valid and properly formatted.

### Subject

The email subject line:

- Enter the subject text directly in the input field
- Can include template parameters in the format `{{parameterName}}`
- Supports both static text and dynamic content

Example: `Your credential from {{issuerName}} is ready`

### Email Body

The main content of the email:

- Enter the body text in the textarea
- Supports HTML formatting for rich content
- Can include template parameters in the format `{{parameterName}}`
- JSON parameters are automatically formatted with proper indentation when displayed in the email

Example:
```
Dear {{recipientName}},

Your new credential has been issued and is ready for your review.

Credential details:
{{credentialDetails}}

Best regards,
The {{issuerName}} Team
```

### Parameters

Define the dynamic values that will be substituted in the subject and body:

1. **Parameter Name**: The name of the parameter used in the template (without curly braces)
2. **Value Source**:
   - **Static**: A fixed value entered directly
   - **Trigger Input**: Value from a trigger parameter
   - **Action Outcome**: Value from a previous action result
3. **Value**: The content to substitute for the parameter

Parameters allow for highly customized emails with content specific to each workflow execution.

## How It Works

When a workflow with an Email action is executed:

1. The platform resolves the recipient email address and all parameters
2. It processes the subject and body templates, replacing all `{{parameterName}}` occurrences with actual values
3. If any parameter value contains JSON, it is automatically formatted for better readability
4. The email is sent via SendGrid to the specified recipient
5. The result of the operation is stored in the workflow outcome
6. The workflow continues based on the email sending result


## Requirements and Dependencies

For the Email action to work properly:

1. SendGrid API key must be configured in the platform settings
2. Sender email address and name must be configured in the platform settings
3. The platform must have outbound network access to reach the SendGrid API
4. Recipients must have valid email addresses
. 
Overwrite the settings in the appSettings.json:
`EmailSettings__SendGridKey`
`EmailSettings__SendGridFromEmail`

## Troubleshooting

Common issues and solutions:

1. **Emails Not Sending**: Verify SendGrid API key is valid and correctly configured
2. **Parameter Substitution Issues**: Check that parameter names in templates exactly match the defined parameters
3. **Formatting Problems**: If HTML content isn't rendering correctly, verify the HTML syntax
4. **Email Deliverability**: Check for spam filtering issues and ensure the sender domain is properly configured in SendGrid
