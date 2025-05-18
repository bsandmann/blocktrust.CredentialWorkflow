---
title: Home
layout: default
nav_order: 1
permalink: /
---

# Credential Workflow Platform

Welcome to the **Blocktrust Credential Workflow Platform** documentation - your guide to building, managing, and automating SSI worksflows around Hyperledger Identus workflows.

## Overview

The Credential Workflow Platform is a powerful, extensible system that enables organizations to:

- Design custom workflows with triggers and sequential actions
- Automate credential issuance, verification, and management
- Implement decentralized identity (DID) management, by creating, updating, and deactivating DIDs
- Integrate with external systems through HTTP and DIDComm
- Fully Open Source based on the Apache 2.0 license

Built on modern .NET technology, this platform provides a flexible, rule-based approach to credential orchestration while maintaining high security and compliance standards.

## Key Features

- **IFTTT-Style Workflow Engine**: Design event-driven workflows with conditional logic
- **Multiple Trigger Types**: HTTP requests, timers, manual execution
- **Rich Action Library**: 
  - Issue W3C compliant credentials
  - Verify credentials (signature, expiry, revocation)
  - Make HTTP requests to external services
  - Send emails with customizable templates
  - Create and manage DIDs
  - Communicate using DIDComm messaging
  - Generate JWT tokens for authentication
  - Custom validation with business rules
- **Multi-Tenant Architecture**: Securely manage multiple organizational contexts
- **Parameter Substitution**: Dynamic content in templates and HTTP requests
- **Visual Workflow Designer**: Intuitive web interface for workflow creation

## Documentation Structure

This documentation is organized into the following sections:

- [Getting Started](UserGuides/index.md): Installation, setup, and basic configuration
- [Workflow Overview](WorkflowOverview): Platform concepts and architecture
- [Workflow Triggers](Triggers/): Different ways to initiate workflows
- [Actions](Actions/): Available actions and their configuration
- [Platform Settings](Settings/): Configuration options for the platform

## Supported Standards

- W3C Verifiable Credentials
- Decentralized Identifiers (DIDs)
- JWT (JSON Web Tokens)
- DIDComm Messaging
- OAuth 2.0 / OpenID Connect

## Contact & Support

For additional help or feature requests, please contact the Blocktrust team or open an issue in our GitHub repository.
