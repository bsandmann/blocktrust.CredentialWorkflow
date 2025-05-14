# JWT Token Generation Action Documentation

The `JWT Token Generation` action enables workflows to create JSON Web Tokens (JWTs) using the RSA keys associated with your tenant. This action allows for configuring standard JWT fields and custom claims, supporting both static values and dynamic data from workflow triggers and previous actions.

## Overview

JSON Web Tokens (JWTs) are an industry standard for securely transmitting information between parties as a compact, self-contained token. In the Blocktrust Credential Workflow platform, the JWT Token Generation Action enables workflows to:

- Create cryptographically signed tokens for authentication and authorization
- Transform verified credential data into standardized JWT format
- Generate access tokens for downstream API interactions
- Encode claims data with expiration policies
- Build tokens compatible with OpenID Connect and OAuth 2.0 flows

This action leverages your tenant's RSA key pair to sign tokens, ensuring they can be verified by third parties who have access to your public key.

## Configuration Options

### Issuer (iss)

The issuer claim identifies the entity that issued the JWT:

- **Automatically configured** based on your host URL and tenant ID
- Example: `https://example.com/5f8d7e6c-9b4a-4f2d-8e1d-3c5b2f9a7e8d`
- Cannot be modified directly as it must match your tenant's identity

This ensures that all tokens created by your tenant have a consistent and verifiable issuer claim.

### Audience (aud)

The audience claim identifies the recipients or systems that the JWT is intended for:

1. **Source**: 
   - **Static Value**: Manually enter a specific audience identifier
   - **From Trigger**: Use an audience value provided in the trigger parameters

2. **Value**: 
   - For static sources: The specific audience string (e.g., `https://api.example.com`)
   - For trigger sources: Select the parameter name that contains the audience

The audience is required and helps prevent tokens issued for one system from being used with another.

### Subject (sub)

The subject claim identifies the principal entity (typically a user) that the token represents:

1. **Source**: 
   - **Static Value**: Manually enter a specific subject identifier
   - **From Trigger**: Use a subject provided in the trigger parameters
   - **From Previous Action**: Use a subject value from a previous action's outcome

2. **Value**: 
   - For static sources: The specific subject string (e.g., a user ID or DID)
   - For trigger sources: Select the parameter that contains the subject
   - For action outcomes: Select the action that produced the subject

When using a previous credential verification action, the system will attempt to extract the subject from the credential's subject ID.

### Expiration (exp)

The expiration claim defines when the token becomes invalid:

1. **Source**:
   - **Static Value**: Manually enter a number of seconds
   - **From Trigger**: Use an expiration value provided in the trigger parameters

2. **Value**:
   - For static sources: Number of seconds from current time (e.g., `3600` for 1 hour)
   - For trigger sources: Select the parameter that contains the expiration in seconds

The system uses this value to calculate a precise expiration timestamp in the JWT.

### Claims

Claims are statements about the subject and additional data carried by the token. There are three ways to define claims:

1. **Define Manually**: Add custom claims with static values
   - **Claim Key**: The name of the claim (e.g., `role`, `email`, `permissions`)
   - **Value**: A fixed value entered directly

2. **From Trigger**: Map claims from trigger parameters
   - **Claim Key**: The name of the claim to create in the token
   - **Value**: Select the trigger parameter to use for each claim

3. **From Previous Action**: Use all claims from a credential in a previous action
   - Automatically imports all credential subject properties as claims
   - Especially useful when translating a verified credential into a JWT
   - Select the specific action (typically a credential verification or issuance action)

## How It Works

When a workflow with a JWT Token Generation action is executed:

1. The platform resolves the issuer, audience, subject, and expiration values
2. It retrieves the tenant's RSA private key for signing
3. If using claims from a previous action, it extracts the credential claims from that action's outcome
4. Otherwise, it processes each manually defined claim or trigger-derived claim
5. The system generates a JWT with the standard registered claims (iss, sub, aud, exp, iat, jti)
6. The token is signed using the tenant's RSA private key with the RS256 algorithm
7. The signed JWT is returned in the workflow outcome
8. The token can be accessed by subsequent actions in the workflow

## Technical Details

The JWT Token Generation action:

1. Uses the tenant's 3072-bit RSA key pair stored in the database
2. Signs tokens using the RS256 algorithm (RSA Signature with SHA-256)
3. Includes standard JWT headers specifying the algorithm and key ID
4. Adds a unique token identifier (jti) to prevent token replay
5. Automatically adds the issuance timestamp (iat) and not-before time (nbf)
6. When extracting claims from previous credentials, handles different data formats properly
7. Validates all required parameters before generating the token

## Integration with Other Components

This action integrates with:

- **JWT Keys System**: Uses the tenant's RSA key pair configured at tenant creation
- **Workflow Context**: Reads input parameters and stores the resulting token
- **Parameter Resolution**: Supports dynamic values from trigger inputs and previous actions
- **JSON Web Key Set (JWKS) Endpoint**: Your tenant's public keys are available at `/{tenant-id}/.well-known/jwks.json` for token verification

## Use Cases

The JWT Token Generation action is ideal for:

1. **API Authentication**: Generate tokens for accessing protected APIs
   - Example: Create an access token with specific scopes for a third-party API

2. **Credential Translation**: Transform credentials into JWT format for systems that don't support W3C VCs
   - Example: Convert a W3C Verifiable Credential into a JWT for a legacy system

3. **Single Sign-On**: Create authentication tokens for SSO integration
   - Example: Generate a token containing user identity information for an external application

4. **Authorization**: Encode authorization information for downstream services
   - Example: Create a token with role and permission claims based on verified credentials

5. **Delegation**: Create tokens for delegated access to resources
   - Example: Generate a short-lived token for a specific operation on behalf of a user

## Example Configurations

### Basic Authentication Token

A simple token for API authentication:

1. Audience: Static Value - `https://api.example.com`
2. Subject: From Trigger - `userId` parameter
3. Expiration: Static Value - `3600` (1 hour)
4. Claims: Manually defined
   - `role`: "user"
   - `scope`: "read write"

### Credential-Based Access Token

Convert a verified credential into an access token:

1. Audience: Static Value - `https://resource-server.example.com`
2. Subject: From Previous Action - Verification action that processed the credential
3. Expiration: Static Value - `900` (15 minutes)
4. Claims: From Previous Action - Selected verification action
   - Automatically includes all credential subject properties as claims

### Dynamic API Client Token

Generate a token with parameters from an API request:

1. Audience: From Trigger - `serviceUrl` parameter
2. Subject: Static Value - `api-client`
3. Expiration: From Trigger - `tokenLifetime` parameter
4. Claims: From Trigger
   - `clientId`: `clientId` parameter
   - `scope`: `requestedScope` parameter

## JWKS Endpoint

Your tenant's public keys are available via a JWKS (JSON Web Key Set) endpoint:

```
/{tenant-id}/.well-known/jwks.json
```

This endpoint provides the RSA public key in standardized JWKS format:
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

Third-party systems can use this endpoint to retrieve your public key and verify tokens you've issued.

## Security Considerations

When using the JWT Token Generation action:

1. **Token Lifetime**: Set appropriate expiration times based on security requirements
   - Shorter lifetimes are more secure but require more frequent token renewal
   - Consider the use case when determining expiration (minutes for sensitive operations, hours for general sessions)

2. **Claim Content**: Be mindful of what data you include in claims
   - Avoid including sensitive information as token claims are only encoded, not encrypted
   - Consider data minimization principles and include only necessary claims

3. **Audience Validation**: Always specify precise audience values
   - Use specific URLs or identifiers rather than generic values
   - Receiving systems should validate that they are the intended audience

4. **Subject Handling**: Ensure subject values uniquely identify the entity
   - DIDs are excellent subject values for decentralized identity tokens
   - Consider using stable, non-reusable identifiers for subjects

5. **Key Security**: The system handles key security, but be aware:
   - Your tenant's RSA private key is used for signing all tokens
   - The key is securely stored in the database and never exposed via API