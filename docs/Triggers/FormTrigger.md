---
title: Form Trigger
layout: default
parent: Triggers
nav_order: 4
---

# FormTrigger Component Documentation

The `FormTrigger` component enables the creation of custom web forms that can trigger workflows when submitted. This allows for user-friendly data collection through a dedicated form page that's automatically generated based on your configuration.

## Features

- **Dynamic Form Generation**: Automatically creates web forms based on configured fields
- **Custom URL Endpoint**: Provides a unique form URL that can be shared with users
- **Form Customization**: Configurable title, description, and form fields
- **Field Validation**: Enforces required fields and proper validation
- **Multiple Field Types**: Supports text, number, date, and boolean field types

## Form Configuration

### Form Settings

1. **Form URL**: Unique URL where the form can be accessed
   - Format: `{base-url}/form/{workflow-id}`
   - Can be copied to the clipboard using the "Copy URL" button

2. **Form Title**: Optional custom title for the form page
   - If not provided, the workflow name will be used as the form title

3. **Form Description**: Optional text providing context or instructions
   - Displayed below the title on the form page

### Field Configuration

Each form can have multiple fields that collect different types of information:

1. **Field Name**: Identifier for the field (required)
   - Used to reference the field value in workflow actions

2. **Field Type**: Determines the input validation and UI presentation
   - **Text**: Standard text input (default)
   - **Number**: Numeric input only
   - **Yes/No**: Boolean input
   - **Date**: Date selection input

3. **Description**: Label text shown to the user (required)
   - Should clearly explain what information to enter

4. **Default Value**: Optional pre-filled value for the field

### Form States

The form page can display several different states:

1. **Active Form**: When the workflow is active and properly configured
2. **Inactive Form**: When the workflow exists but is set to inactive (displays a message indicating the form is unavailable)
3. **Error State**: When configuration issues prevent the form from loading
4. **Success State**: After successful submission, with option to submit another response

When a form is submitted:

1. The FormService validates all required fields are present
2. An execution context containing the form data is created
3. A workflow outcome is generated and enqueued for processing
4. The workflow actions can access form field values through parameter resolution
