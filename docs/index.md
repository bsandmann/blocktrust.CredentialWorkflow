---
title: Home
layout: default
nav_order: 1
permalink: /
---

# Credential Workflow Platform

Welcome to the **Blocktrust Credential Workflow Platform** documentation - your comprehensive guide to building, managing, and automating verifiable credential workflows.

## Overview

The Credential Workflow Platform is a powerful, extensible system that enables organizations to:

- Create and manage verifiable credentials using W3C standards
- Design custom workflows with triggers and sequential actions
- Automate credential issuance, verification, and management
- Integrate with external systems through HTTP and DIDComm
- Implement decentralized identity (DID) management

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
  - Generate JWT tokens
  - Custom validation with business rules
- **Multi-Tenant Architecture**: Securely manage multiple organizational contexts
- **Parameter Substitution**: Dynamic content in templates and HTTP requests
- **Visual Workflow Designer**: Intuitive web interface for workflow creation

## Documentation Structure

This documentation is organized into the following sections:

- [Getting Started](getting-started.md): Installation, setup, and basic configuration
- [Core Concepts](concepts/index.md): Fundamental architecture and design
- [Workflow Triggers](triggers/index.md): Different ways to initiate workflows
- [Actions](actions/index.md): Available actions and their configuration
- [Web Interface](web/index.md): Using the web-based workflow designer
- [API Reference](api/index.md): Programmatic interfaces
- [Development Guide](development/index.md): Extending the platform
- [Tutorials](tutorials/index.md): Guided examples for common use cases

## Quick Links

- [Platform Architecture](concepts/architecture.md)
- [HTTP Action Guide](actions/http.md)
- [Email Action Guide](actions/email.md)
- [W3C Credential Operations](actions/credentials.md)
- [DID Management](actions/did.md)
- [Parameter Substitution](concepts/parameters.md)
- [Workflow Context](concepts/context.md)
- [Custom Validation](actions/custom-validation.md)

## Supported Standards

- W3C Verifiable Credentials
- Decentralized Identifiers (DIDs)
- JWT (JSON Web Tokens)
- DIDComm Messaging
- OAuth 2.0 / OpenID Connect

## Contact & Support

For additional help or feature requests, please contact the Blocktrust team or open an issue in our GitHub repository.

---

Copyright � Blocktrust. All rights reserved.