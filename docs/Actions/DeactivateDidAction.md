---
title: Deactivate DID Action
layout: default
parent: Actions
nav_order: 10
---

# DeactivateDID Action Documentation

The DeactivateDID action enables workflows to deactivate (permanently disable) decentralized identifiers (DIDs) on the PRISM blockchain. This is the final stage in a DID's lifecycle, indicating that the identifier should no longer be considered valid for any operations.
For a details walkthrough of the process, see the [Create and Update DID Userguide](../UserGuides/CreatingAndUpdatingDids.md).

## Overview

Deactivating a DID is a critical security operation that might be necessary when:

- A DID's private key has been compromised
- The DID is no longer needed and should be retired
- The owner wants to publicly signal that the DID should no longer be trusted

Once deactivated, a DID cannot be reactivated and any credentials issued by it should be considered invalid.

## Configuration Options

### DID Registrar Settings

Like other DID actions, the DeactivateDID action can use DID registrar settings from one of two sources:

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

To deactivate a DID, you must provide:

1. **DID**: The decentralized identifier to deactivate
   - Must be in format `did:prism:...`
   - Can be a static value or obtained from the trigger/previous action

2. **Master Key Secret** (Optional): The base64-encoded private key associated with the DID's master key
   - Required for authentication to prove ownership of the DID
   - Can be obtained from the CreateDID action output or from an external source
   - Must be valid base64-encoded data

Both these values can be configured to use:
- **Static values**: Directly entered values
- **Trigger parameters**: Values passed from the workflow trigger
- **Action outcomes**: Values from previous actions in the workflow

## Action Outcome

When executed, the DeactivateDID action:

1. Connects to the specified DID registrar
2. Creates and submits a deactivation transaction to the blockchain
3. Returns the status of the deactivation operation

## Technical Notes

- DID deactivation is a permanent, irreversible operation
- Deactivation is an on-chain transaction that requires a funded wallet in the OpenPrismNode
- The transaction may take time to be confirmed on the blockchain
- The DID document will still be resolvable, but will be marked as deactivated
- Deactivation does not delete the DID from the blockchain, but signals it should not be used
- Verifiers should check for deactivation status when validating credentials issued by a DID
