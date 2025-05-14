# ManualTrigger Component Documentation

The `ManualTrigger` component provides a simple way to create workflows that can be triggered manually by users from the Workflow overview page. Unlike other trigger types that respond to external events or time-based schedules, manual triggers are explicitly executed by a user when needed.

## Features

- **No Configuration Required**: The manual trigger does not contain any settings to configure
- **Simple Implementation**: Displays an explanatory message instead of configuration options
- **User-Initiated Execution**: Can be triggered directly from the Workflow listing page

## Usage

The Manual Trigger is ideal for workflows that:

1. Need to be run on-demand rather than on a schedule
2. Require direct user oversight before execution
3. Are used for maintenance tasks or occasional processes
4. Serve as templates that may be executed periodically but not automatically

## Manual Execution

Workflows with a manual trigger can be executed from the Workflow overview page (`/workflows`) by following these steps:

1. Navigate to the Workflow page
2. Locate the workflow with a manual trigger in the list
3. Click the "Execute" button in the "Execution" column of the workflow table
4. The workflow will be initiated immediately and its execution can be monitored from the logs

## Technical Implementation

The ManualTrigger component:

1. Uses `TriggerInputOnDemand` as its underlying data model
2. Displays a simple informational message to users in the workflow designer
3. Does not require any configuration parameters
4. Integrates with the workflow execution system to allow manual triggering

## Integration with Workflow Execution

When a workflow with a ManualTrigger is set to active:

1. The workflow is marked as `ActiveWithManualTrigger` in the database
2. An "Execute" button is displayed for this workflow in the workflow listing
3. When the button is clicked, the `ExecuteManualWorkflow` method is called
4. A workflow outcome is created and enqueued for processing

## Use Cases

The ManualTrigger is particularly useful for:

1. **Administrative Tasks**: Operations that should only be performed under direct supervision
2. **Credential Management**: Manual issuing or verification operations that aren't part of an automated flow
3. **Testing/Debugging**: Running workflows manually to verify their functionality
4. **Emergency Operations**: Critical processes that should never run automatically
5. **Ad-hoc Reporting**: Generating reports or statistics on-demand rather than on a schedule