# DIDComm Action Documentation

The `DIDComm Action` component enables workflows to communicate with external wallets and applications using the DIDComm v2 protocol. This action allows for sending trust pings, messages, and verifiable credentials to recipients through secure, encrypted DIDComm channels.

## Overview

DIDComm (Decentralized Identifier Communication) is a protocol for secure, private, and authenticated messaging between DID-identified entities. In the Blocktrust Credential Workflow platform, the DIDComm Action enables workflows to:

- Establish secure communication channels with external wallets
- Send basic messages to recipients
- Issue verifiable credentials directly to wallets
- Verify connectivity through trust ping operations

This component serves as a bridge between your workflow platform and the broader decentralized identity ecosystem, allowing for standardized credential exchange.

## Configuration Options

### Sender Peer DID

The DID that will be used as the sender identity for the DIDComm message:

- Select from the tenant's available peer DIDs
- Displays the name and a truncated version of the DID for easy identification
- The private keys associated with this DID will be used to sign and encrypt the message

This identity represents your tenant in the communication. For information on managing PeerDIDs, see the [PeerDID Settings documentation](PeerDidSettings.md).

### Recipient Peer DID

The DID of the recipient that the message will be sent to:

1. **Source**: 
   - **Static Value**: Manually enter a specific recipient DID
   - **From Trigger**: Use a DID provided in the trigger parameters
   - **From Previous Action**: Use a DID produced by a previous action in the workflow

2. **Value**: 
   - For static sources: The specific DID string to use
   - For trigger sources: Select the parameter name that contains the DID
   - For action outcomes: Select the action that produced the DID

The recipient DID must be a valid peer DID that the system can resolve to obtain service endpoints for message delivery.

### DIDComm Type

The type of DIDComm message to send:

1. **Trust Ping**:
   - A simple connectivity test to verify that the communication channel works
   - Useful for checking if a wallet is online and accessible
   - Requires minimal configuration - just sender and recipient DIDs

2. **Message**:
   - A basic DIDComm message with customizable content fields
   - Allows for defining a structured message with multiple fields
   - Each field can have static or dynamic values

3. **Credential Issuance**:
   - Sends a verifiable credential to the recipient wallet
   - Follows the DIDComm Issue Credential v2 protocol
   - Requires a credential input from a previous action or trigger

### Message Content (for Message type)

When the **Message** type is selected, you can define the content fields:

1. **Add Field**: Creates a new message field with key and value
2. **Field Key**: The property name in the message
3. **Field Value Source**:
   - **Static**: A fixed value entered directly
   - **Trigger Input**: Value from a trigger parameter
   - **Action Outcome**: Value from a previous action result
4. **Field Value**: The actual content of the field

### Credential Source (for Credential Issuance type)

When the **Credential Issuance** type is selected, you need to specify the credential to be sent:

1. **Source Selection**:
   - **Static Value**: Directly enter a W3C Verifiable Credential JWT
   - **From Trigger**: Use a credential supplied by the workflow trigger
   - **From Previous Action**: Use a credential produced by a previous action (typically an Issue W3C Credential action)

2. **Value Configuration**:
   - For static values: Enter the raw JWT credential
   - For trigger inputs: Select the parameter that contains the credential
   - For action outcomes: Select the action that produced the credential

## How It Works

When a workflow with a DIDComm action is executed:

1. The platform resolves both sender and recipient DIDs
2. It retrieves the cryptographic keys associated with the sender DID
3. The system resolves the recipient's DID document to find service endpoints
4. Based on the DIDComm type, it creates the appropriate message:
   - For Trust Ping: A simple ping message
   - For Message: A structured message with the defined fields
   - For Credential Issuance: A credential offer message with the credential as an attachment
5. The message is encrypted using the sender's keys and the recipient's public key
6. The encrypted message is sent to the recipient's service endpoint
7. The result of the operation is stored in the workflow outcome

## Technical Details

The DIDComm action:

1. Uses the `PeerDidResolver` to resolve DIDs to their DID documents
2. Extracts service endpoints from the recipient's DID document
3. Handles mediator routing if the recipient uses a mediator
4. Packages messages according to the DIDComm v2 specification
5. Supports the Issue Credential v2 protocol for credential issuance
6. Performs encryption and signing using the tenant's keys

## Integration with Other Components

This action integrates with:

- **PeerDID Management**: Uses tenant-managed Peer DIDs from the [PeerDID Settings](PeerDidSettings.md)
- **Issue W3C Credential Action**: Can use credentials created by this action for issuance
- **Workflow Context**: Reads input parameters and stores operation results
- **Parameter Resolution**: Supports dynamic values from trigger inputs and previous actions

## Use Cases

The DIDComm action is ideal for:

1. **Wallet Integration**: Send credentials directly to user wallets
2. **Multi-Party Workflows**: Communicate with multiple participants in a credential exchange
3. **Notification Systems**: Alert users of important credential events
4. **Verification Requests**: Initiate credential verification flows with external systems
5. **Trust Establishment**: Create secure channels for subsequent credential exchange

## Example Configurations

### Trust Ping Configuration

A basic connectivity check:

1. Sender PeerDID: Select one of your tenant's Peer DIDs
2. Recipient PeerDID: Specify the recipient's DID (static or from trigger)
3. DIDComm Type: Trust Ping

### Credential Issuance Configuration

Send a credential to a wallet:

1. Sender PeerDID: Select one of your tenant's Peer DIDs
2. Recipient PeerDID: Set to the subject's DID (usually from a trigger parameter)
3. DIDComm Type: Credential Issuance
4. Credential Source: From Previous Action (select an Issue W3C Credential action)

### Custom Message Configuration

Send a structured message:

1. Sender PeerDID: Select one of your tenant's Peer DIDs
2. Recipient PeerDID: Specify the recipient's DID
3. DIDComm Type: Message
4. Message Content: Add fields like "subject", "body", and "priority"

## Requirements and Dependencies

For the DIDComm action to work properly:

1. You must have at least one Peer DID created in your tenant settings (see [PeerDID Settings](PeerDidSettings.md))
2. For credential issuance, you need a valid credential from a previous action
3. The recipient DID must be resolvable and have valid service endpoints
4. Your workflow platform must have outbound network access to reach the recipient's service endpoints

## Troubleshooting

Common issues and solutions:

1. **Message Delivery Failure**: Ensure the recipient DID is valid and has accessible service endpoints
2. **Encryption Errors**: Verify that your Peer DIDs have properly configured cryptographic keys
3. **Invalid Credential**: For credential issuance, check that the credential is a valid JWT
4. **Missing Parameters**: Ensure all required values are properly configured in the action