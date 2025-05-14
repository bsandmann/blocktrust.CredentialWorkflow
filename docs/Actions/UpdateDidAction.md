---
title: Update DID Action
layout: default
parent: Actions
nav_order: 9
---

# UpdateDID Action Documentation

The UpdateDID action enables workflows to modify existing decentralized identifiers (DIDs) on the PRISM blockchain. This action allows for adding, removing, or replacing verification methods and service endpoints of a DID that was previously created.

## Overview

Updating a DID is an important operation for maintaining decentralized identities by:

- Adding new verification methods (cryptographic keys)
- Removing compromised or obsolete keys
- Modifying service endpoints
- Keeping the DID document current as requirements change

## Configuration Options

### DID Registrar Settings

Like the [CreateDID action](CreateDidAction), the UpdateDID action can use DID registrar settings from one of two sources:

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

### DID and Master Key Secret

To update a DID, you must provide:

1. **DID**: The decentralized identifier to update
   - Must be in format `did:prism:...`
   - Can be a static value or obtained from the trigger/previous action

2. **Master Key Secret**: The base64-encoded private key associated with the DID's master key
   - Required for authentication to prove ownership of the DID
   - Can be obtained from the CreateDID action output or from an external source
   - Must be valid base64-encoded data

Both these values can be configured to use:
- **Static values**: Directly entered values
- **Trigger parameters**: Values passed from the workflow trigger
- **Action outcomes**: Values from previous actions in the workflow

### Update Operations

The UpdateDID action supports three types of operations that can be performed on a DID:

#### 1. Add Operation

Adds a new verification method (key) to the DID document, configurable with:

- **Key ID**: A unique identifier for this key (e.g., "key-1")
  - Must contain only ASCII letters, numbers, and dashes
  - Must be unique within the DID document

- **Purpose**: The intended use of this key
  - **authentication**: For authenticating as the DID subject
  - **keyAgreement**: For establishing encrypted communication
  - **assertionMethod**: For issuing verifiable credentials
  - **capabilityInvocation**: For authorization capabilities
  - **capabilityDelegation**: For delegating capabilities

- **Curve**: The cryptographic curve to use
  - **secp256k1**: Standard Bitcoin curve (default for most purposes)
  - **Ed25519**: Edwards curve for signatures
  - **X25519**: Montgomery curve (automatically selected for KeyAgreement)

#### 2. Remove Operation

Removes an existing verification method from the DID document:

- **Key ID**: The identifier of the key to remove
  - Must match an existing verification method in the DID document
  - Must contain only ASCII letters, numbers, and dashes

#### 3. Set Operation

Replaces all service endpoints in the DID document with a new set. For each service:

- **Service ID**: A unique identifier for this service (e.g., "service-1")
  - Must contain only ASCII letters, numbers, and dashes
  - Must be unique within the DID document

- **Type**: The type of service
  - **Predefined types**:
    - LinkedDomain
    - DIDCommMessaging
    - CredentialRegistry
    - OID4VCI
    - OID4VP
  - **Custom types**: You can define your own service type

- **Endpoint**: The URL where the service is located
  - Must be a valid URL or URI

## Action Outcome

When executed, the UpdateDID action:

1. Connects to the specified DID registrar
2. Authenticates using the master key secret
3. Performs all update operations in sequence
4. Submits the DID update transaction to the blockchain
5. Returns the updated DID document for use in subsequent workflow actions

## Technical Notes

- DID updates are on-chain transactions that require a funded wallet in the OpenPrismNode
- Each update transaction may take time as it requires blockchain confirmation
- The action automatically handles key generation for added verification methods
- Multiple operations can be included in a single update transaction
- Update operations are applied in the order they are defined
- Failed operations will cause the entire update to fail

## Differences from CreateDID

While the [CreateDID action](CreateDidAction) initializes a new DID document, the UpdateDID action modifies an existing DID document by:

1. Requiring a reference to an existing DID
2. Requiring the master key secret for authentication
3. Supporting targeted operations (add, remove, set) rather than complete document definition
4. Allowing incremental changes to the DID document

For information on creating new DIDs, see the [CreateDID action documentation](CreateDidAction).