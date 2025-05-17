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

The setting for the issuing key pairs is tenant-specific, meaning each tenant can only see and manage their own issuing keys.

## Creating a New Issuing Key

To create a new issuing key, you need to provide the following information:

1. **Key Name**: A descriptive name for the key (e.g., "Production Key 2025" or "Test Key")
2. **DID**: A Decentralized Identifier in the format `did:prism:[64 hex characters][:optional suffix]`
3. **Key Type**: Currently, only `secp256k1` is supported
4. **Public Key**: The base64url-encoded public key (33 bytes when decoded)
5. **Private Key**: The base64url-encoded private key (32 bytes when decoded)

**Note**: If you have a uncompressed version of the public-key consisting of the x and y value, you can also provide the public key by adding both values.

### Requirements

When adding a new issuing key, ensure that:

- The DID is in the correct format: `did:prism:[64 hex characters]`
- Both public and private keys are properly formatted in base64url encoding
- The public key is 33 bytes when decoded (alternatlivy you can provide the uncompressed version of the public key in two fields)
- The private key is 32 bytes when decoded

### Validation

The form performs several validations:
- DID format is checked against a regular expression pattern
- Keys are validated to ensure they are proper base64url format
- The decoded key lengths are verified to match expected sizes

## Security Considerations

- Private keys are sensitive information and should be handled securely
- The system stores private keys to be able to sign credentials
- You should keep backup copies of your keys in a secure location
- Consider rotating keys periodically for enhanced security
- Do not use a hosted version of the OPN instance for production purposes, as this may expose your private keys to unauthorized access
