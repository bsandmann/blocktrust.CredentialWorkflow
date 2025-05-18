---
title: Wallet Interaction Trigger
layout: default
parent: Triggers
nav_order: 5
---

# WalletInteractionTrigger Documentation

The `WalletInteractionTrigger` enables workflows to be triggered by DIDComm v2 messages from digital wallets or other DIDComm-capable applications. This component allows for secure, decentralized communication between the workflow platform and external systems using the DIDComm protocol.

## Features

- **DIDComm v2 Protocol Support**: Enables secure, encrypted communications using decentralized identifiers
- **Peer DID Generation**: Automatically generates a Peer DID for the workflow
- **Message Type Configuration**: Supports different types of DIDComm message interactions
- **Out-of-Band Invitation**: Provides QR codes and invitation URLs for establishing connections
- **Parameter Configuration**: Define custom parameters for basic message types

## Message Types

The WalletInteractionTrigger supports two primary message types:

### 1. Basic Message

A simple text-based communication format that can include custom parameters:

- Allows defining custom fields that should be included in the message
- Provides a schema and example message format
- Useful for simple data exchange or notifications

### 2. Credential Presentation

A specialized interaction for requesting and receiving verifiable credentials:

- Enables credential verification workflows
- Automatically configures presentation request endpoints
- Provides cURL examples for initiating presentation requests

## DIDComm Endpoints

The component generates several important endpoints for DIDComm interactions:

### 1. DIDComm Endpoint URL

The main endpoint where DIDComm messages should be sent:
```
{base-url}/api/workflow/{workflow-id}/didcomm
```

### 2. Presentation Request Endpoint (for Credential Presentation type only)

An HTTP endpoint for initiating presentation requests:
```
{base-url}/api/workflow/{workflow-id}/didcomm/presentation-request
```

### 3. Out-of-Band Invitation Endpoint

An endpoint that provides an out-of-band invitation with QR code:
```
{base-url}/api/workflow/{workflow-id}/didcomm/oob
```

## Peer DID Configuration

Each WalletInteractionTrigger requires a Peer DID for secure communication:

1. **Automatic Generation**: The component automatically generates a new Peer DID when initialized
2. **Manual Generation**: Users can generate a new Peer DID at any time using the "Generate New PeerDID" button
3. **Persistent Storage**: The generated Peer DID is stored with the workflow configuration

For more detailed information on creating and managing PeerDIDs that can be used with wallet interaction triggers, please refer to the [PeerDID Management documentation](../Settings/PeerDidSettings).

## Out-of-Band Invitation

The out-of-band invitation feature facilitates the initial connection between the workflow and external wallets:

1. **Accessible at**: `{base-url}/api/workflow/{workflow-id}/didcomm/oob`
2. **Features**:
   - HTML page with formatted invitation details
   - QR code for scanning with mobile wallets
   - Clickable invitation URL for desktop applications
   - Follows DIDComm OOB v2 protocol specification

## Technical Implementation

The WalletInteractionTrigger integrates with several components:

1. **DIDCommController**: Handles incoming DIDComm messages and processes them through the workflow
2. **PeerDID Management**: Creates and manages PeerDIDs with appropriate key types
3. **Message Processing**: Unpacks encrypted DIDComm messages and triggers workflow execution

The controller supports various DIDComm protocol features:
- Return routing for responses
- Forward messaging to mediators
- Processing of encrypted messages
- Generation of presentation requests
