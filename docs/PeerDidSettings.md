# PeerDID Management Documentation

PeerDIDs are decentralized identifiers used for secure DIDComm messaging between the Blocktrust Credential Workflow platform and external wallet applications. The PeerDID management interface allows you to create, view, and delete PeerDIDs for your tenant.

## Overview

PeerDIDs in the Blocktrust Credential Workflow platform are essential components for:

- Establishing secure DIDComm v2 communication channels
- Authenticating the workflow platform to external wallets
- Enabling encrypted message exchange
- Supporting wallet interaction triggers in workflows

## Accessing PeerDID Management

You can access the PeerDID management interface at:
```
/Account/Manage/PeerDids
```

This page is tenant-specific, meaning each tenant can only see and manage their own PeerDIDs.

## Creating a New PeerDID

Creating a new PeerDID is a straightforward process:

1. Navigate to the PeerDID management page
2. Enter a descriptive name for the PeerDID in the input field
3. Click the "Add PeerDID" button

The system will automatically:
- Generate a new PeerDID with the appropriate cryptographic keys
- Configure the service endpoint to your current host
- Store the PeerDID and associated keys securely
- Display the newly created PeerDID in the list

### Naming Your PeerDIDs

Choose descriptive names that help you identify the purpose of each PeerDID. For example:
- "Production Wallet Connection"
- "Test Environment DIDComm"
- "Mobile App Integration"

## Managing Existing PeerDIDs

The interface displays all existing PeerDIDs for your tenant with the following information:

- PeerDID name
- Truncated PeerDID value (for security and readability)
- Creation date and time

Each PeerDID entry provides two actions:
- **Copy DID**: Copies the full PeerDID value to your clipboard for use in other applications
- **Delete**: Removes the PeerDID from your tenant

## Using PeerDIDs with Wallet Interaction Triggers

PeerDIDs created in this interface can be used in the WalletInteractionTrigger component for workflows:

1. Create a workflow with a WalletInteractionTrigger
2. Copy a PeerDID from the management interface
3. Paste it into the PeerDID field of the trigger configuration

This establishes the DIDComm identity that the workflow will use when communicating with external wallets.

Refer to the [WalletInteractionTrigger documentation](WalletInteractionTrigger.md) for more detailed information on configuring wallet interaction workflows.

## Technical Details

### PeerDID Format

PeerDIDs follow the format specified in the Peer DID Method specification:
```
did:peer:1[code][encoded-value]
```

For example:
```
did:peer:1zQmZMygzYqNwU6Uhmewx5Xepf2VLp5S4HLSwwgf2aiKZuwa
```

### Service Endpoint

Each PeerDID is automatically configured with a service endpoint matching your current host URL. This allows external applications to send DIDComm messages to your workflow platform.

### Security Considerations

- PeerDIDs contain cryptographic material and should be treated as sensitive information
- The system securely stores the private keys associated with each PeerDID
- When a PeerDID is deleted, all associated cryptographic material is removed
- PeerDIDs are tenant-specific and properly isolated between different organizations