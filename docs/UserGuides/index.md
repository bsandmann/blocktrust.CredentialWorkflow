---
title: User Guides
layout: default
nav_order: 3
has_children: true
---

# User Guides

Welcome to the Blocktrust Credential Workflow user guides. These guides provide step-by-step instructions for common workflows and use cases within the platform.

## Available Guides

### [Creating and Updating DIDs](CreatingAndUpdatingDids.md)

This guide covers how to create and manage Decentralized Identifiers (DIDs) using the Blocktrust Credential Workflow platform. Learn how to:
- Set up the DID registrar settings with an OpenPrismNode (OPN) instance
- Create a new DID with verification methods and services
- Configure issuance keys for credential signing
- Update an existing DID by adding new verification methods
- Deactivate a DID when it's no longer needed

### [Issuing Credentials](IssuingCredentials.md)

Learn how to issue W3C Verifiable Credentials through a form-based workflow. This guide walks through:
- Setting up an issuing DID with proper verification methods
- Creating a form trigger to collect recipient information
- Validating form inputs before credential issuance
- Creating and signing W3C Verifiable Credentials
- Delivering credentials to recipients via email

### [Verifying Credentials](VerifyingCredentials.md)

This guide demonstrates how to verify W3C Verifiable Credentials presented to your service. Follow along to:
- Set up an HTTP trigger to receive credentials for verification
- Verify the cryptographic integrity and validity of credentials
- Validate credential data against business rules
- Return verification results to the requesting service
