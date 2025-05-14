---
title: Create DID Action
layout: default
parent: Actions
nav_order: 4
---

# CreateDID Action Documentation

The CreateDID action enables workflows to create new decentralized identifiers (DIDs) on the PRISM blockchain. This action allows for configuring the DID's verification methods and service endpoints, with options to use either tenant-wide or custom DID registrar settings.

## Overview

Creating a DID is an important operation for establishing a decentralized identity that can be used for:

- Issuing verifiable credentials
- Establishing secure communication channels
- Proving identity in decentralized systems
- Managing cryptographic keys in a standards-compliant way

## Configuration Options

### DID Registrar Settings

The CreateDID action can use DID registrar settings from one of two sources:

1. **Tenant Settings** (Recommended)
   - Use the centrally configured DID registrar settings from your tenant
   - Simplifies management and ensures consistency across workflows
   - Requires proper [DID Registrar Settings](../Settings/DidRegistrarSettings) configuration

2. **Custom Settings**
   - Configure registrar settings specific to this workflow action
   - Allows for using different registrars for different workflows
   - Requires specifying:
     - **OPN Registrar URL**: The URL of the OpenPrismNode registrar
     - **Wallet ID**: The wallet identifier within the OPN

For information on setting up tenant-wide DID registrar settings, see the [DID Registrar Settings](../Settings/DidRegistrarSettings) documentation.

### Verification Methods

Verification methods define the cryptographic keys associated with the DID and their purposes. Each DID must have at least one verification method, and you can add multiple methods with different purposes.

For each verification method, configure:

1. **Key ID**: A unique identifier for this key (e.g., "key-1")
   - Must contain only ASCII letters, numbers, and dashes
   - Must be unique within the DID document

2. **Purpose**: The intended use of this key
   - **Authentication**: For authenticating as the DID subject
   - **KeyAgreement**: For establishing encrypted communication
   - **AssertionMethod**: For issuing verifiable credentials
   - **CapabilityInvocation**: For authorization capabilities
   - **CapabilityDelegation**: For delegating capabilities

3. **Curve**: The cryptographic curve to use
   - **secp256k1**: Standard Bitcoin curve (default for most purposes)
   - **Ed25519**: Edwards curve for signatures
   - **X25519**: Montgomery curve (automatically selected for KeyAgreement)

> **Note**: When "KeyAgreement" is selected as the purpose, the curve is automatically set to "X25519" as this is the appropriate curve for key agreement operations.

### Service Endpoints

Services are optional endpoints associated with a DID that define how to interact with the identity. You can add multiple services, each with:

1. **Service ID**: A unique identifier for this service (e.g., "service-1")
   - Must contain only ASCII letters, numbers, and dashes
   - Must be unique within the DID document

2. **Type**: The type of service
   - **Predefined types**:
     - LinkedDomain
     - DIDCommMessaging
     - CredentialRegistry
     - OID4VCI
     - OID4VP
   - **Custom types**: You can define your own service type by toggling to custom value

3. **Endpoint**: The URL where the service is located
   - Must be a valid URL or URI

## Dynamic Parameters

Each field in the CreateDID action can be configured to use dynamic values from:

- **Static values**: Directly entered values
- **Trigger parameters**: Values passed from the workflow trigger
- **Workflow context**: Values from previous actions in the workflow

This enables creating DIDs with properties determined at runtime.

## Action Outcome

When executed, the CreateDID action:

1. Connects to the specified DID registrar
2. Creates a new DID with the configured verification methods and services
3. Submits the DID creation transaction to the blockchain
4. Returns the newly created DID for use in subsequent workflow actions

The created DID is stored in the workflow context and can be used by other actions, such as:
- Storing the DID in the tenant's issuing keys
- Creating a verifiable credential with the new DID as the issuer
- Establishing DIDComm connections

## Technical Notes

- DID creation is an on-chain transaction that requires a funded wallet in the OpenPrismNode
- The creation process may take time as it requires blockchain confirmation
- The action automatically handles key generation for each verification method
- Created DIDs follow the PRISM DID method specification (did:prism:...)