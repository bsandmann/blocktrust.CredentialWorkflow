---
title: Settings
layout: default
nav_order: 6
has_children: true
---

# Platform Settings

The Settings section provides configuration options for various components of the Credential Workflow Platform. Proper configuration of these settings is essential for the effective operation of workflows, especially those involving credential operations, DID management, and external integrations.

## Settings Categories

The platform includes the following settings categories:

1. **DID Management**
   - DID Registrar Configuration
   - PeerDID Settings

2. **Key Management**
   - Issuing Keys Settings
   - JWT Keys Settings

3. **Integration Settings**
   - Email Configuration
   - External Service Connections
   - [Application Configuration](Configuration.md)

## Configuration Persistence

All settings are:

- Tenant-specific, providing isolation in multi-tenant deployments
- Persisted in the database, ensuring configuration survives application restarts
- Audited for changes, maintaining a record of configuration modifications
- Access-controlled, limiting visibility and modification rights to authorized users

## Settings Impact

Configuration settings directly affect workflow execution:

- **DID Registrar Settings**: Determine how DIDs are created and managed
- **Issuing Keys Settings**: Control credential signing and issuance capabilities
- **JWT Keys Settings**: Configure token generation for authentication and authorization
- **PeerDID Settings**: Establish parameters for DIDComm messaging

## Configuration Best Practices

When configuring platform settings:

1. **Use Meaningful Names**: Clear, descriptive names for keys and configurations
2. **Manage Permissions**: Restrict configuration access to administrative users
3. **Test After Changes**: Verify workflow operation after significant setting updates
4. **Document Custom Settings**: Maintain records of non-default configurations
5. **Regular Review**: Periodically review settings for security and relevance

For detailed information about each settings category, refer to the specific documentation pages listed below.