---
title: JWT Keys Settings
layout: default
parent: Settings
nav_order: 4
---

# JWT Keys Management Documentation

The JWT Keys management page displays the RSA key pair used by your tenant for signing and verifying JSON Web Tokens (JWTs). Unlike other settings, these keys are automatically generated during tenant creation and cannot be changed.

## Overview

JSON Web Tokens (JWTs) are a compact, URL-safe means of representing claims between two parties. In the Blocktrust Credential Workflow platform, JWTs are used for:

- Securely signing credentials issued by your tenant
- Providing cryptographic proof of the credential issuer's identity
- Enabling verification of credential authenticity by relying parties

## Accessing JWT Keys

You can access the JWT Keys management page at:
```
/Account/Manage/JwtKeys
```

This page is tenant-specific, meaning each tenant can only see their own RSA key pair.

## Key Generation

The RSA key pair for your tenant is generated automatically during tenant creation:

1. A secure 3072-bit RSA key pair is generated using the .NET cryptography libraries
2. The public and private keys are stored in XML format in the tenant database record
3. These keys are permanent and cannot be changed or rotated through the user interface

## Key Usage

The JWT Keys page displays:

1. **Public Key (PEM Format)**: The public key can be shared with external parties for JWT verification
2. **Private Key (PEM Format)**: The private key is used internally for JWT signing and must be kept secure

## JWKS Endpoint

The platform provides a JWKS (JSON Web Key Set) endpoint that external applications can use to verify JWTs issued by your tenant:

```
/{tenant-id}/.well-known/jwks.json
```

Where `{tenant-id}` is your tenant's unique identifier.

### What is a JWKS?

A JWKS (JSON Web Key Set) is a standardized format for publishing cryptographic keys used for JWT verification. The JWKS endpoint:

- Provides the public key in a standardized JSON format
- Includes key metadata like the algorithm used and key ID
- Can be automatically consumed by JWT libraries and frameworks
- Is cached for improved performance (1 hour cache duration)

### Using the JWKS Endpoint

When integrating with external systems that need to verify credentials issued by your tenant:

1. Provide them with your JWKS endpoint URL
2. The external system can fetch your public key from this endpoint
3. The external system can then use this key to verify JWTs signed by your tenant

## Security Considerations

- The private key is sensitive information and should never be shared
- The public key and JWKS endpoint are designed to be publicly accessible
- The keys are generated using secure cryptographic practices
- The system uses RSA-SHA256 (RS256) for JWT signatures by default

## Technical Details

### RSA Key Size

The platform uses 3072-bit RSA keys, providing a strong security level that balances security and performance.

### Key Format

On the JWT Keys page, the keys are displayed in PEM format for easier integration with other systems, but they are stored internally in XML format.

### JWKS Format

The JWKS endpoint provides the public key as a standard JSON Web Key Set in the following format:

```json
{
  "keys": [
    {
      "kty": "RSA",
      "kid": "tenant-{your-tenant-id}",
      "alg": "RS256",
      "n": "{base64url-encoded-modulus}",
      "e": "{base64url-encoded-exponent}"
    }
  ]
}
```