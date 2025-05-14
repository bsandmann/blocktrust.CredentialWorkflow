---
title: Workflow Overview
layout: default
nav_order: 2
---

# Workflow Overview Page Documentation

The Workflow Overview page is the central hub for managing all your credential workflows in the Blocktrust Credential Workflow platform. This page provides a comprehensive view of your workflows, their current states, and recent execution outcomes, while also offering tools for workflow management, creation, and execution.

## Accessing the Workflow Overview

The Workflow Overview page is accessible at:
```
/workflows
```

This page requires authentication and is typically the main dashboard for managing your credential workflows.

## Workflow Management Features

### Viewing Workflows

The Workflow Overview displays all workflows associated with your tenant in a tabular format with the following information:

1. **Name**: The descriptive name of the workflow
2. **State**: The current operational state of the workflow (Active or Inactive)
3. **Last Run Outcome**: The result of the most recent workflow execution
4. **Last Run Ended**: The timestamp when the last execution completed

Each workflow entry is clickable, allowing you to navigate to the workflow designer for detailed editing.

### Creating Workflows

There are three ways to create new workflows:

1. **Create New**: Click the "Create New" button to create a blank workflow
2. **Import**: Import an existing workflow from a JSON file
3. **Create from Template**: Use pre-defined templates to create new workflows with common patterns

### Running and Stopping Workflows

Each workflow can be activated or deactivated directly from the overview page:

1. **Run**: Click the "Run" button to activate a workflow
   - The system automatically detects the trigger type and sets the appropriate state
   - For HTTP triggers, this makes the endpoint accessible
   - For recurring triggers, this starts the scheduled execution
   - For form triggers, this activates the form for data collection
   - For wallet interaction triggers, this enables communication with wallet applications
   - For manual triggers, this enables the "Execute" button

2. **Stop**: Click the "Stop" button to deactivate a running workflow
   - This prevents new executions from being triggered
   - Ongoing executions will continue until completion

### Manual Execution

Workflows with manual triggers can be directly executed from the overview page:

1. The "Execute" button appears in the "Execution" column for workflows with manual triggers
2. The button is only enabled when the workflow is in the "ActiveWithManualTrigger" state
3. Clicking "Execute" immediately creates a new workflow outcome and enqueues it for processing
4. A toast notification confirms successful triggering

This feature is ideal for testing workflows and for operational tasks that need to be run on demand rather than on a schedule.

### Workflow Actions

Additional actions available for each workflow:

1. **Delete**: Permanently remove a workflow (requires confirmation)
2. **Export**: Download the workflow as a JSON file for backup or sharing
3. **View Logs**: Click on the outcome status to navigate to the logs page for the workflow

## Templates Section

The Templates section displays available workflow templates for quick workflow creation:

### Template Information

Each template includes:
1. **Name**: The descriptive name of the template
2. **Description**: A brief explanation of the template's purpose and functionality

### Creating from Templates

To create a workflow from a template:

1. Click the "Create from Template" button for your chosen template
2. The system automatically:
   - Loads the template structure
   - Processes any placeholders with tenant-specific information
   - Creates a new workflow with the processed template
   - Redirects you to the workflow designer to review and customize the workflow

Templates provide starting points for common credential workflows, saving time and ensuring best practices are followed.

## Workflow States

Workflows can exist in various states, indicated by color-coded badges:

1. **Inactive** (Gray): The workflow is not running and will not respond to triggers
2. **Active States** (Green): The workflow is running and will respond to triggers:
   - **ActiveWithExternalTrigger**: Listening for HTTP requests
   - **ActiveWithRecurrentTrigger**: Running on a schedule
   - **ActiveWithFormTrigger**: Accepting form submissions
   - **ActiveWithWalletInteractionTrigger**: Listening for wallet communications
   - **ActiveWithManualTrigger**: Ready for manual execution

## Workflow Outcome States

Workflow execution outcomes are indicated by color-coded badges:

1. **Success** (Green): The workflow executed successfully
2. **FailedWithErrors** (Red): The workflow encountered errors during execution
3. **NotStarted** (Yellow): The workflow outcome has been created but execution has not begun
4. **Running** (Blue): The workflow is currently executing

Clicking on the outcome badge navigates to the detailed logs view for that workflow.

## Logs Page

The Logs page provides detailed information about workflow executions, allowing you to debug and monitor your workflows. This page is accessible in two ways:

1. Directly via URL: `/logs/{workflowId}`
2. By clicking on an outcome status badge in the Workflow Overview page

### Log Page Features

#### Workflow Selection

The top of the Logs page includes a dropdown menu that allows you to switch between different workflows. When you select a workflow, the page displays all execution outcomes for that workflow.

#### Execution History

The main section displays a table of all execution attempts for the selected workflow:

1. **State**: The outcome state (Success, FailedWithErrors, NotStarted, Running) with color coding
2. **Started**: When the execution began (sortable column)
3. **Ended**: When the execution completed (sortable column)

#### Detailed Execution Information

Clicking on any execution row expands it to reveal detailed information:

1. **Action Outcomes**: Results of each individual action in the workflow
   - Action ID and Outcome ID for traceability
   - Detailed JSON output from each action
   - Copy buttons for easy sharing or analysis

2. **Execution Context**: Information about how the workflow was triggered
   - HTTP Method used (for HTTP triggers)
   - Query Parameters received (with values and copy buttons)
   - Request Body (formatted for readability)

3. **Error Information**: When a workflow fails, this section displays:
   - Detailed error messages
   - Stack traces when available
   - Formatted for easy debugging

#### Interactive Features

The Logs page includes several interactive features:

1. **Sorting**: Click column headers to sort by start or end time
2. **Copy to Clipboard**: Each section includes copy buttons for sharing data
3. **JSON Formatting**: All JSON data is automatically formatted for readability
4. **Refresh**: A refresh button to update the log data

### Using the Logs Page for Debugging

The Logs page is essential for debugging workflow issues:

1. **Trigger Validation**: Check if the execution context contains the expected parameters
2. **Action Execution**: Review each action outcome to identify where issues occurred
3. **Error Analysis**: Examine detailed error messages to understand failures
4. **Data Flow**: Track how data passes from one action to another through the workflow

## Importing Workflows

To import a workflow from JSON:

1. Click the "Import" button to open the import modal
2. Either:
   - Paste the JSON directly into the textarea
   - Select a JSON file using the file picker
3. Click "Import" to validate and create the workflow
4. The system will validate the JSON against the workflow schema
5. If valid, a new workflow is created and added to your workflow list

## Technical Details

The Workflow Overview page:

1. Automatically loads all workflows associated with your tenant
2. Displays real-time workflow states and outcomes
3. Uses color-coded indicators for quick status assessment
4. Validates imported workflows against a JSON schema
5. Implements confirmation dialogues for destructive actions
6. Provides toast notifications for action confirmations
7. Supports workflow execution directly from the overview

## Best Practices

For effective workflow management:

1. **Naming**: Use descriptive names for your workflows that reflect their purpose
2. **Templates**: Leverage templates for common scenarios to ensure consistency
3. **Testing**: Use manual execution for testing before activating scheduled workflows
4. **Organization**: Deactivate workflows that aren't currently needed to maintain clarity
5. **Backup**: Regularly export important workflows for backup purposes
6. **Monitoring**: Regularly check the logs page to ensure workflows are executing correctly
7. **Debugging**: When issues occur, use the detailed information in the logs page to identify and fix problems

The Workflow Overview page provides a complete management interface for your credential workflows, from creation and configuration to execution and monitoring.