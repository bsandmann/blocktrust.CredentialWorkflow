# Blocktrust Workflow Platform

## TL;DR
Blocktrust Workflow Platform is an open-source, no-code solution for creating and managing Self-Sovereign Identity (SSI) workflows. The project heavily relies on [Hyperledger Identus](https://github.com/hyperledger/identus) (formaly Atala PRISM) and in particular its Cloud Agents. It simplifies the process of issuing and validating credentials, making SSI integration accessible to developers, businesses, and innovators without requiring deep SSI expertise.

## Project Overview

### Goals
The primary goal of the Blocktrust Workflow Platform is to accelerate the adoption of Self-Sovereign Identity (SSI) by providing an intuitive, flexible, and powerful toolset for creating SSI workflows based on [Hyperledger Identus](https://github.com/hyperledger/identus) (formaly Atala PRISM) . By eliminating the complexity typically associated with SSI integration, we aim to make digital identity solutions more accessible and implementable across various projects and use cases.

### Expected Results
Upon completion, the Blocktrust Workflow Platform will offer:

1. A user-friendly interface for creating, configuring, and managing SSI workflows
2. A set of triggers and actions for building versatile SSI processes
3. Templates for common SSI workflows to jumpstart integration
4. Open-source codebase allowing for community contributions and customizations
5. Compatibility with multiple operating systems and cloud environments

### Technical Details
- **Framework**: Built on .NET using C#
- **Database**: PostgreSQL
- **Deployment**: Docker image for easy setup and portability

### Key Features
1. **Triggers**: Listen for specific events or inputs, such as:
    - POST/GET requests to predefined endpoints
    - Form submissions
    - DIDComm messages (e.g., Basic messages, PRISM connect protocol requests)

2. **Actions**: Process and validate inputs, including:
    - DID resolution
    - Credential verification
    - Input value checks
    - Calling predefined endpoints
    - Issuing or revoking credentials
    - Sending DIDComm messages
    - Managing DIDs (create, update, deactivate, publish)
    - Generating QR codes for Out-of-Band connections

### Example Use Cases
1. **Automated Credential Issuance**: When a user completes a course, automatically issue a credential to their provided wallet address.
2. **Streamlined User Registration**: Upon QR code scan, register the user and redirect them to a specified page.
3. **KYC Verification**: Automatically verify a user's KYC credential and grant access to protected resources.
4. **SSI-based Sign-In**: Generate a JSON Web Token for users signing in with their SSI credentials.


### Contact
For questions, feedback, or collaboration opportunities, please reach out to the Blocktrust team at [blocktrust.dev](https://blocktrust.dev) or on our [Discord channel](https://discord.gg/6BghFzxnmt).
