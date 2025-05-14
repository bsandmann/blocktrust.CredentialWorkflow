---
title: Custom Validation Action
layout: default
parent: Actions
nav_order: 6
---

# CustomValidation Action Documentation

The `CustomValidation` action enables workflows to perform flexible, JavaScript-based validation on any JSON data. Unlike the more structured W3C credential validation, this component allows for complex validation logic with custom expressions, making it suitable for validating various data formats and implementing business-specific validation requirements.

## Overview

Custom validation is essential for many workflows that need to verify data beyond predefined patterns. This action allows you to:

- Apply flexible validation rules to any JSON data structure
- Create dynamic validation expressions using JavaScript
- Define custom error messages for each validation rule
- Control workflow execution based on validation results

## Configuration Options

### Data Source

The action needs data to validate. You can specify the source of this data using:

1. **Source Selection**:
   - **From Trigger**: Use data provided by the workflow trigger
   - **From Previous Action**: Use data produced by a previous action in the workflow

2. **Value Configuration**:
   - For trigger inputs: Select the parameter that contains the data
   - For action outcomes: Select the action that produced the data

### Validation Rules

The core of this action is the ability to define custom validation rules using JavaScript expressions. Each rule consists of:

1. **Rule Name**: A descriptive name for the validation rule
2. **Expression**: A JavaScript expression that evaluates to true or false
3. **Error Message**: The message to display when the validation fails

You can add multiple validation rules, and all must pass for the data to be considered valid.

#### JavaScript Expressions

The validation expressions use JavaScript syntax and have access to a special `data` object that represents the input data. For example:

- `data.age >= 18`: Validates that the age field is at least 18
- `data.email.includes('@')`: Checks that an email contains the @ symbol
- `data.items.length > 0`: Ensures an array has at least one item
- `data.total === data.items.reduce((sum, item) => sum + item.price, 0)`: Verifies that the total matches the sum of item prices

### Error Handling

The action provides options for handling validation failures:

1. **Action on Failure**:
   - **Stop Workflow**: The workflow execution stops if validation fails
   - **Continue with Next Action**: The workflow continues to the next action even if validation fails
   - **Skip to Specific Action**: The workflow jumps to a specified action if validation fails

2. **Skip to Action** (when "Skip" is selected):
   - Allows selecting which action in the workflow to jump to after a validation failure

## How It Works

When a workflow with a CustomValidation action is executed:

1. The platform resolves the data from the configured source
2. The data is parsed from JSON if necessary
3. Each validation rule is applied in sequence:
   - The JavaScript expression is evaluated using the Jint JavaScript engine
   - The `data` variable in the expression is bound to the input data
   - The expression must evaluate to a boolean result (true/false)
   - If the expression evaluates to false, the rule is considered failed
4. If all validation rules pass, the action succeeds
5. If any validation rule fails:
   - The error is recorded
   - The workflow proceeds according to the configured failure action

## Technical Implementation

The validation process operates by:

1. Parsing the JSON data into a dynamic object structure
2. Using the Jint JavaScript engine to evaluate expressions
3. Providing a sandbox environment where expressions can safely access the data
4. Capturing and reporting any evaluation errors

## Expression Examples

### Basic Field Validation

| Expression | Description |
|------------|-------------|
| `data.id !== undefined` | Ensures the ID field exists |
| `typeof data.name === 'string'` | Verifies name is a string type |
| `data.email && data.email.includes('@')` | Checks for a valid email format |

### Numeric Validation

| Expression | Description |
|------------|-------------|
| `data.age >= 18 && data.age <= 65` | Age must be between 18 and 65 |
| `data.amount > 0` | Amount must be positive |
| `data.score >= 0 && data.score <= 100` | Score must be between 0 and 100 |

### Array and Object Validation

| Expression | Description |
|------------|-------------|
| `data.items && data.items.length > 0` | Items array must not be empty |
| `data.addresses.some(a => a.type === 'billing')` | Must have a billing address |
| `data.skills && data.skills.includes('JavaScript')` | Must have JavaScript skill |

### Complex Validation

| Expression | Description |
|------------|-------------|
| `data.password.length >= 8 && /[A-Z]/.test(data.password) && /[0-9]/.test(data.password)` | Password complexity check |
| `data.startDate < data.endDate` | End date must be after start date |
| `data.items.every(item => item.quantity > 0)` | All item quantities must be positive |

## Use Cases

The CustomValidation action is ideal for:

1. **Business Rules**: Implement complex business validation logic
2. **Form Validation**: Validate user-submitted form data before processing
3. **Data Transformation Validation**: Ensure transformed data meets requirements
4. **API Response Validation**: Validate responses from external services
5. **Cross-Field Validation**: Check relationships between multiple fields

## Integration with Other Components

This action works well with:

- **Form Triggers**: Validate data collected from form submissions
- **HTTP Triggers**: Validate data received from API calls
- **JSON Transformation Actions**: Validate data after transformation
- **Error Handling Actions**: Process validation errors appropriately

## Best Practices

1. **Name Rules Clearly**: Give each rule a descriptive name that identifies its purpose
2. **Keep Expressions Simple**: Break complex validation into multiple smaller rules
3. **Provide Helpful Error Messages**: Include specific details about why validation failed
4. **Use Defensive Programming**: Check for null/undefined values before accessing properties
5. **Test Thoroughly**: Validate your expressions with both valid and invalid data

## Limitations

- JavaScript expressions have access only to the provided data and basic JavaScript functionality
- No access to external services or APIs within expressions
- Complex regular expressions may need careful escaping
- Performance may be affected by very complex expressions or large data structures