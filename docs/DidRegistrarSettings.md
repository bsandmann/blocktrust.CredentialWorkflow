---
title: DID Registrar Settings
layout: default
parent: Settings
nav_order: 1
---

# DID Registrar Settings Documentation

The DID Registrar settings allow you to configure how your tenant interacts with a Prism DID registrar node when creating, updating, or deleting decentralized identifiers (DIDs). These settings are essential for any workflows that involve DID operations.

## Overview

A DID Registrar is responsible for registering and managing decentralized identifiers on a blockchain network. In the Blocktrust Credential Workflow platform, the DID Registrar settings enable you to:

- Connect to an OpenPrismNode (OPN) instance
- Specify which wallet within the OPN to use for DID operations
- Enable automated DID creation and management within workflows

## Accessing DID Registrar Settings

You can access the DID Registrar settings at:
```
/Account/Manage/DidRegistrar
```

This page is tenant-specific, meaning each tenant can configure their own DID Registrar settings.

## Configuration Options

The DID Registrar settings page requires the following information:

1. **OPN Registrar URL**: The URL of the OpenPrismNode (OPN) instance that will handle DID registrations
   - Example: `https://opn.mainnet.blocktrust.dev`
   - This should be the base URL of a running OPN instance

2. **Wallet ID**: The identifier of the wallet within the OPN that will be used for DID operations
   - This corresponds to a specific wallet created in the OPN instance
   - The wallet must have sufficient funds to pay for DID operations on the blockchain

## Setting Up OpenPrismNode (OPN)

To use the DID Registrar functionality, you need access to an OpenPrismNode instance. You can:

1. Use an existing OPN instance if you have access to one
2. Deploy your own OPN instance following the documentation

For detailed instructions on how to set up, run, and configure an OpenPrismNode, please refer to the official OpenPrismNode documentation:

[OpenPrismNode Documentation](https://bsandmann.github.io/OpenPrismNode/)

This documentation provides step-by-step instructions for:
- Installing and running OpenPrismNode
- Creating and managing wallets
- Configuring wallet settings
- Obtaining wallet IDs
- Monitoring DID operations

## Using DID Registrar with Workflows

Once you've configured your DID Registrar settings, you can use the following workflow actions:

1. **CreateDID Action**: Automatically generates and registers a new DID on the blockchain
2. **UpdateDID Action**: Updates an existing DID document
3. **DeactivateDID Action**: Deactivates a DID that is no longer needed

These actions will use the configured OPN instance and wallet to perform the necessary blockchain operations.

## Technical Details

### How DID Registration Works

1. When a workflow needs to create a DID, it contacts the specified OPN Registrar URL
2. The OPN instance uses the provided Wallet ID to identify which wallet to use for the operation
3. The wallet must have sufficient funds to pay for the blockchain transaction
4. The DID is registered on the blockchain and becomes globally resolvable
5. The private keys associated with the DID are stored securely in the Blocktrust platform

### Security Considerations

- The OPN Registrar URL should be secured with HTTPS
- The wallet ID is sensitive information that identifies which wallet will be charged for operations
- Consider using separate wallets for different environments (production, testing, etc.)
- Regularly monitor wallet balances to ensure sufficient funds for DID operations