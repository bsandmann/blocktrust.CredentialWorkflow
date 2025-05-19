---
title: Issue W3C Credential
layout: default
parent: Actions
nav_order: 3
---

# Issue W3C Credential Action Documentation

The `IssueW3CCredential` action enables workflows to create and sign W3C Verifiable Credentials using cryptographic keys associated with the tenant's DIDs. This action allows for configuring the credential subject, claims, issuer, and expiration date.

## Overview

Creating W3C Verifiable Credentials is a core function of the credential workflow platform, allowing for:

- Creating cryptographically verifiable statements about a subject
- Defining credential claims with both static and dynamic values
- Signing credentials with the tenant's issuing keys
- Setting appropriate validity periods

## Configuration Options

### Subject DID

The DID of the entity that the credential is being issued to:

1. **Source**: 
   - **Static Value**: Manually enter a specific DID
   - **From Trigger**: Use a DID provided in the trigger parameters

2. **Value**: 
   - For static sources: The specific DID string to use
   - For trigger sources: The parameter name that contains the DID

### Issuer DID

The DID of the entity issuing the credential:

- Select from the tenant's available issuing keys
- Displays the key name and a truncated version of the DID for easy identification
- The private key associated with this DID will be used to sign the credential

### Valid Until

The expiration date of the credential:

- Optional field - if not specified, the credential will not have an expiration date
- Must be today or a future date
- The credential will expire at the end of the specified day (23:59:59)

### Claims

Claims are the actual data fields included in the credential:

1. **Claim Key**: The property name that will appear in the credential
   - Must be unique within the credential
   - Typically describes the type of information (e.g., "name", "birthDate", "licenseNumber")

2. **Claim Value Source**:
   - **Static Value**: Fixed data entered directly
   - **From Trigger**: Dynamic data pulled from trigger parameters

3. **Claim Management**:
   - **Add Claim**: Add a new claim to the credential
   - **Remove Claim**: Delete an existing claim
   - **Edit Claim**: Change the key name or value

## How It Works

When a workflow with an IssueW3CCredential action is executed:

1. The platform resolves all parameter values (subject DID, claims, etc.)
2. It creates an unsigned W3C Verifiable Credential with the specified data
3. The system retrieves the private key for the selected issuing key
4. The credential is signed using the private key
5. The signed credential is stored in the workflow outcome
6. The credential can be accessed by subsequent actions in the workflow

## Example Configuration

A typical configuration might include:

1. Subject DID from an HTTP trigger parameter
2. Issuer DID selected from the tenant's issuing keys
3. Expiration date set to one year from issuance
4. Claims including:
   - `name`: Static value or from trigger
   - `email`: From trigger parameter
   - `membershipLevel`: Static value