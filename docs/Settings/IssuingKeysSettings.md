---
title: Issuing Keys Settings
layout: default
parent: Settings
nav_order: 2
---

# Issuing Keys Management Documentation

Issuing keys are cryptographic key pairs associated with Decentralized Identifiers (DIDs) that allow your tenant to issue verifiable credentials. The Issuing Keys management interface allows you to create, view, and delete issuing keys for your tenant.

## Overview

Each tenant in the Blocktrust Credential Workflow platform can have multiple issuing keys. These keys are used to:

- Sign verifiable credentials when issuing them
- Associate issued credentials with a specific DID
- Provide cryptographic proof of the credential issuer's identity

## Accessing Issuing Keys Management

You can access the Issuing Keys management interface at:
```
/Account/Manage/IssuingKeys
```

This page is tenant-specific, meaning each tenant can only see and manage their own issuing keys.

## Creating a New Issuing Key

To create a new issuing key, you need to provide the following information:

1. **Key Name**: A descriptive name for the key (e.g., "Production Key 2025" or "Test Key")
2. **DID**: A Decentralized Identifier in the format `did:prism:[64 hex characters][:optional suffix]`
3. **Key Type**: Currently, only `secp256k1` is supported
4. **Public Key**: The base64url-encoded public key (33 bytes when decoded)
5. **Private Key**: The base64url-encoded private key (32 bytes when decoded)

### Requirements

When adding a new issuing key, ensure that:

- The DID is in the correct format: `did:prism:[64 hex characters]`
- Both public and private keys are properly formatted in base64url encoding
- The public key is 33 bytes when decoded
- The private key is 32 bytes when decoded

### Validation

The form performs several validations:
- DID format is checked against a regular expression pattern
- Keys are validated to ensure they are proper base64url format
- The decoded key lengths are verified to match expected sizes

## Managing Existing Keys

The interface displays all existing issuing keys for your tenant with the following information:

- Key name
- Truncated DID (for security reasons)
- Key type
- Truncated public and private keys
- Creation date and time

Each key has a delete option that allows you to remove it from your tenant.

## Security Considerations

- Private keys are sensitive information and should be handled securely
- The system stores private keys to be able to sign credentials
- You should keep backup copies of your keys in a secure location
- Consider rotating keys periodically for enhanced security

## Alternative Key Creation

While you can manually add issuing keys through this interface, you can also generate them automatically using the CreateDID workflow action. This action will:

1. Generate a proper DID and key pair
2. Register the DID with the specified registrar
3. Store the resulting key for use in credential issuance

Refer to the Workflow Actions documentation for more information on using the CreateDID action.

## Technical Details

### Key Format

- **DID Format**: `did:prism:[64 hex characters][:optional suffix]`
- **Key Type**: The cryptographic algorithm used (currently only secp256k1)
- **Public Key**: Base64url-encoded 33-byte key
- **Private Key**: Base64url-encoded 32-byte key

### Tenant Association

All issuing keys are associated with a specific tenant and are only accessible to users within that tenant. This ensures proper isolation between different organizations using the platform.