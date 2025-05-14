---
title: Validate W3C Credential Action
layout: default
parent: Actions
nav_order: 7
---

# ValidateW3CCredential Action Documentation

The `ValidateW3CCredential` action enables workflows to perform detailed validation of W3C Verifiable Credentials against custom validation rules. This component goes beyond the basic verification checks (signature, expiry, revocation) to allow for credential content validation, ensuring that credentials contain the expected data in the correct format.

## Overview

Validating credential content is critical for many use cases, as it allows systems to:

- Ensure required fields are present in the credential
- Validate that field values meet specific format requirements
- Check numerical values against acceptable ranges
- Confirm values match expected constants or allowed lists
- Implement custom validation logic for complex requirements

## Configuration Options

### Credential Source

The action needs to know which credential to validate. You can specify this using:

1. **Source Selection**:
   - **Static Value**: Directly enter a W3C Verifiable Credential JWT or JSON
   - **From Trigger**: Use a credential supplied by the workflow trigger
   - **From Previous Action**: Use a credential produced by a previous action in the workflow

2. **Value Configuration**:
   - For static values: Enter the raw credential string
   - For trigger inputs: Select the parameter that contains the credential
   - For action outcomes: Select the action that produced the credential

### Validation Rules

The heart of this action is the ability to define custom validation rules. Multiple rules can be added, and all must pass for the credential to be considered valid. Each rule consists of:

1. **Rule Type**: The type of validation to perform
2. **Configuration**: A specific configuration string that defines what to check

The following rule types are supported:

#### Required Field

Ensures that a specified field exists in the credential.

- **Configuration Format**: `fieldPath`
- **Example**: `credentialSubject.id`
- **Description**: Checks that the specified JSON path exists in the credential

#### Format Check

Validates that a field conforms to a specific format.

- **Configuration Format**: `fieldPath:formatType`
- **Example**: `credentialSubject.email:EMAIL`
- **Supported Formats**:
  - `ISO8601`: Validates date/time strings
  - `EMAIL`: Validates email addresses
  - `URL`: Validates properly formatted URLs
  - `DID`: Validates decentralized identifiers (must start with "did:")

#### Value Range

Ensures a numeric field falls within a specified range.

- **Configuration Format**: `fieldPath:min-max`
- **Example**: `credentialSubject.age:18-65`
- **Description**: Checks that the numeric value is greater than or equal to `min` and less than or equal to `max`

#### Exact Value

Validates that a field has a specific exact value.

- **Configuration Format**: `fieldPath:expectedValue`
- **Example**: `credentialSubject.type:Student`
- **Description**: Ensures the field matches the expected value exactly

#### Value From Array

Checks that a field's value is one of the specified allowed values.

- **Configuration Format**: `fieldPath:value1,value2,value3`
- **Example**: `credentialSubject.role:Student,Teacher,Admin`
- **Description**: Verifies that the field value matches one of the comma-separated allowed values

#### Custom Rule

Allows for custom validation logic (note: implementation may be limited).

- **Configuration Format**: Custom expression
- **Description**: Enables complex validation logic beyond the standard rule types

### Error Handling

The action provides several options for handling validation failures:

1. **Action on Failure**:
   - **Stop Workflow**: The workflow execution stops if validation fails
   - **Continue with Next Action**: The workflow continues to the next action even if validation fails
   - **Skip to Specific Action**: The workflow jumps to a specified action if validation fails

2. **Skip to Action** (when "Skip" is selected):
   - Allows selecting which action in the workflow to jump to after a validation failure

3. **Error Message Template**:
   - Customizable error message template for validation failures
   - Can include `{{field}}` placeholders that will be replaced with actual field names

## How It Works

When a workflow with a ValidateW3CCredential action is executed:

1. The platform resolves the credential from the configured source
2. The credential is parsed to extract its JSON structure
3. Each validation rule is applied in sequence:
   - The credential's JSON is navigated to find the specified fields
   - The appropriate validation logic is applied based on the rule type
   - Any validation failures are recorded
4. If all validation rules pass, the action succeeds
5. If any validation rule fails:
   - The error is recorded
   - The workflow proceeds according to the configured failure action

## Technical Implementation

The validation process works with both JWT-encoded credentials and JSON-encoded credentials:

1. For JWT credentials, the payload is extracted and parsed
2. JSON paths in validation rules are intelligently mapped, handling both direct paths and paths within the `vc` property (common in JWT credentials)
3. Array indexing is supported in paths (e.g., `credentialSubject.achievements[0].name`)
4. Type conversion is performed as needed to compare values correctly

## Validation Rule Examples

### Required Field Examples

| Configuration | Description |
|---------------|-------------|
| `id` | Checks that the credential has an ID field |
| `credentialSubject.id` | Ensures the subject has an ID |
| `credentialSubject.name` | Verifies the subject has a name |

### Format Examples

| Configuration | Description |
|---------------|-------------|
| `credentialSubject.email:EMAIL` | Verifies email format |
| `issuanceDate:ISO8601` | Checks issuance date is proper ISO format |
| `credentialSubject.website:URL` | Validates website is a proper URL |
| `issuer:DID` | Ensures issuer is a valid DID |

### Range Examples

| Configuration | Description |
|---------------|-------------|
| `credentialSubject.age:18-65` | Age must be between 18 and 65 |
| `credentialSubject.score:-10-100` | Score must be between -10 and 100 |

### Value Examples

| Configuration | Description |
|---------------|-------------|
| `type:VerifiableCredential` | The credential type must be exactly "VerifiableCredential" |
| `credentialSubject.verified:true` | The subject's verified status must be true |

### Value Array Examples

| Configuration | Description |
|---------------|-------------|
| `credentialSubject.role:Student,Teacher,Admin` | Role must be one of the listed values |
| `credentialSubject.status:Active,Pending` | Status must be either Active or Pending |

## Use Cases

The ValidateW3CCredential action is ideal for:

1. **User Onboarding**: Validate required fields in identity credentials
2. **Access Control**: Ensure credentials contain appropriate entitlements
3. **Regulatory Compliance**: Verify credentials meet specific regulatory requirements
4. **Data Quality**: Ensure credentials from third parties match expected formats
5. **Business Rules**: Enforce business-specific validation rules on credentials

## Integration with Other Components

This action works well with:

- **VerifyW3CCredential**: Use verification first to check cryptographic integrity, then validate content
- **CustomValidation**: For more complex validation needs beyond credential structure
- **Workflow Branching**: Use different paths based on validation results