---
title: Manual Trigger
layout: default
parent: Triggers
nav_order: 3
---

# Manual-Trigger Documentation

The `ManualTrigger` component provides a simple way to create workflows that can be triggered manually by users from the Workflow overview page. Unlike other trigger types that respond to external events or time-based schedules, manual triggers are explicitly executed by a user when needed.

## Features

- **No Configuration Required**: The manual trigger does not contain any settings to configure
- **User-Initiated Execution**: Can be triggered directly from the Workflow listing page
- **No Input Parameters**: The the Manual Trigger does not accept any input parameters, the Form-Tirgger cannot be used as parameter-source in other Actions.

## Usage

The Manual Trigger is ideal for workflows that:

1. Need to be run on-demand rather than on a schedule
2. Require direct user oversight before execution
3. Are used for maintenance tasks or occasional processes
