# VerifyW3CCredential Action Documentation

The `VerifyW3CCredential` action enables workflows to verify W3C Verifiable Credentials by checking their cryptographic signatures, expiration dates, and revocation status. This action performs comprehensive validation to ensure credentials are authentic, current, and valid.

## Overview

Verifying W3C Verifiable Credentials is an essential function of the credential workflow platform, allowing for:

- Confirming the authenticity of credentials through signature verification
- Checking that credentials haven't expired
- Validating that credentials haven't been revoked
- Supporting trust chains and schema validation (future functionality)

## Configuration Options

### Verification Checks

The VerifyW3CCredential action includes several verification options that can be enabled or disabled:

1. **Verify Signature** (Enabled by default)
   - Validates that the credential was cryptographically signed by the issuer
   - Verifies the integrity of the credential data
   - Confirms the authenticity of the issuer

2. **Verify Expiry** (Enabled by default)
   - Checks if the credential has an expiration date
   - Validates that the credential hasn't expired
   - Uses the `expirationDate` field in the credential

3. **Verify Revocation Status** (Disabled by default)
   - Checks if the credential has been revoked by the issuer
   - Uses the credential status information to check a revocation registry
   - Requires the credential to include a `credentialStatus` property

4. **Verify Schema** (Disabled by default)
   - Validates that the credential structure conforms to its schema
   - Ensures all required fields are present and correctly formatted
   - *Note: This feature is currently disabled in the UI*

5. **Verify Trust Registry** (Disabled by default)
   - Checks if the issuer is in a trusted registry
   - Confirms the issuer is authorized to issue this type of credential
   - *Note: This feature is currently disabled in the UI*

### Credential Source

The action needs to know where to get the credential for verification. You can configure this using:

1. **Source Selection**:
   - **Static Value**: Directly enter a W3C Verifiable Credential JWT
   - **From Trigger**: Use a credential supplied by the workflow trigger
   - **From Previous Action**: Use a credential produced by a previous action in the workflow

2. **Value Configuration**:
   - For static values: Enter the raw JWT credential
   - For trigger inputs: Select the parameter that contains the credential
   - For action outcomes: Select the action that produced the credential

## How It Works

When a workflow with a VerifyW3CCredential action is executed:

1. The platform resolves the credential from the configured source
2. It parses the credential to extract the relevant fields
3. If signature verification is enabled:
   - The issuer's DID is resolved to obtain their public key
   - The signature is cryptographically verified using the issuer's public key
4. If expiry verification is enabled:
   - The credential's expiration date is checked against the current date/time
5. If revocation checking is enabled:
   - The credential's status is checked using its status method
6. The verification results are stored in the workflow context as a `CredentialVerificationResult`
7. The workflow continues based on the verification outcome

## Verification Result

The action produces a `CredentialVerificationResult` that includes:

- `IsValid`: Overall status (true if all checks pass)
- `SignatureValid`: Whether the signature is valid
- `IsExpired`: Whether the credential has expired
- `IsRevoked`: Whether the credential has been revoked
- `InTrustRegistry`: Whether the issuer is in the trust registry
- `ErrorMessage`: Any error that occurred during verification

These results can be used by subsequent workflow actions to make decisions.

## Technical Details

The VerifyW3CCredential action:

1. Uses the `CredentialParser` service to parse the credential JWT
2. For signature verification, resolves the issuer's DID document and verifies using the appropriate public key
3. For expiry checking, examines the credential's expiration date
4. For revocation checking, makes requests to the credential's status endpoint

## Integration with Other Components

This action integrates with:

- **Parameter Resolution System**: To obtain credentials from various sources
- **DID Resolution**: To resolve issuer DIDs for signature verification
- **Revocation Services**: To check credential revocation status

## Use Cases

The VerifyW3CCredential action is ideal for:

1. **Access Control**: Verify credentials before granting access to resources
2. **Onboarding Processes**: Validate user-provided credentials during registration
3. **Credential Exchange**: Verify credentials received from external systems
4. **Trust Establishment**: Ensure credentials are from trusted sources
5. **Compliance Checks**: Validate that credentials meet regulatory requirements

## Example Configuration

A typical configuration might include:

1. Enabling signature verification and expiry checking
2. Setting the credential source to a trigger parameter named "credential"
3. Using the verification result in subsequent workflow actions to make decisions
4. Configuring error handling for invalid credentials