# RecurringTimerTrigger Component Documentation

The `RecurringTimerTrigger` component enables workflows to be executed automatically on a schedule defined by a CRON expression. This allows for periodic, time-based workflow execution without manual intervention.

## Features

- **CRON Expression Input**: Configures the schedule using standard CRON syntax
- **Human-readable Description**: Displays the schedule in plain English
- **Schedule Preview**: Shows the next 5 execution times based on the current CRON expression
- **Validation**: Validates CRON expressions and provides error feedback

## CRON Expression Format

The component uses the standard CRON format with 5 fields:

```
┌───────────── minute (0 - 59)
│ ┌───────────── hour (0 - 23)
│ │ ┌───────────── day of the month (1 - 31)
│ │ │ ┌───────────── month (1 - 12)
│ │ │ │ ┌───────────── day of the week (0 - 6) (Sunday to Saturday)
│ │ │ │ │
│ │ │ │ │
* * * * *
```

## Common CRON Expressions

Here are some common CRON expressions for reference:

| CRON Expression | Description | Example |
|-----------------|-------------|---------|
| `*/15 * * * *` | Every 15 minutes | 00:00, 00:15, 00:30, 00:45, etc. |
| `0 * * * *` | Every hour | 00:00, 01:00, 02:00, etc. |
| `0 0 * * *` | Every day at midnight | 00:00 each day |
| `0 12 * * *` | Every day at noon | 12:00 each day |
| `0 0 * * 1` | Every Monday at midnight | 00:00 on Mondays |
| `0 0 1 * *` | First day of each month at midnight | 00:00 on the 1st of each month |
| `0 9-17 * * 1-5` | Every hour from 9 AM to 5 PM, Monday through Friday | 9:00, 10:00, ..., 17:00 on weekdays |
| `0 0 1 1 *` | Once a year on January 1st at midnight | 00:00 on January 1st |

## Technical Implementation

The RecurringTimerTrigger component:

1. Uses the `NCrontab` library to validate and process CRON expressions
2. Uses the `CronExpressionDescriptor` library to generate human-readable descriptions
3. Calculates the next 5 occurrences for preview
4. Integrates with a background service (`RecurringWorkflowBackgroundService`) that:
   - Runs every 30 seconds to check for workflows due to execute
   - Parses the CRON expression of each active recurring workflow
   - Determines if any workflows should be triggered based on the current time
   - Creates workflow outcomes and enqueues them for execution

## Integration with Workflow Execution

When a workflow with a RecurringTimerTrigger is set to active:

1. The workflow is marked as `ActiveWithRecurrentTrigger` in the database
2. The background service periodically checks all workflows with this state
3. If a workflow's scheduled time occurs within the check interval, it is triggered
4. A workflow outcome is created and enqueued for processing

## Example Use Cases

The RecurringTimerTrigger is ideal for:

1. **Daily Maintenance**: Schedule credential housekeeping tasks to run daily at a specific time
   - Example: `0 2 * * *` (runs at 2 AM every day)

2. **Periodic Health Checks**: Run verification checks at regular intervals
   - Example: `0 */4 * * *` (runs every 4 hours)

3. **Weekly Reports**: Generate credential status reports on a specific day
   - Example: `0 9 * * 1` (runs at 9 AM every Monday)

4. **Monthly Cleanup**: Clear expired credentials on the first of each month
   - Example: `0 0 1 * *` (runs at midnight on the 1st of each month)

5. **Business Hours Operations**: Run processes only during working hours
   - Example: `0 9-17 * * 1-5` (runs every hour from 9 AM to 5 PM, Monday through Friday)