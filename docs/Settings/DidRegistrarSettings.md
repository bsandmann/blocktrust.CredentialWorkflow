# DID Registrar Settings Documentation

The DID Registrar settings allow you to configure how your tenant interacts with an instance of a DID Registrar, in our case the OPN (OpenPrismNode).

## Overview

To create or update DIDs within the Blocktrust Credential Workflow Platform, you need to define a connection to a DID Registrar. The platform currently supports the [OpenPrismNode (OPN)](https://bsandmann.github.io/OpenPrismNode/)—an open-source implementation of the PRISM node that adheres to the DID-PRISM specification and interacts with the Cardano blockchain.

By configuring the DID Registrar Settings, you enable your workflows to register and update decentralized identifiers (DIDs) using this connection. This ensures your workflows can function without needing to repeatedly enter the connection details.

## Accessing the DID Registrar Settings

To configure the registrar settings:

1. Navigate to your **Tenant Settings** page.
2. Select **"DID Registrar Settings"** from the navigation menu.
3. Fill in the required fields (detailed below).
4. Click **"Save"** to persist the configuration.

These settings will now be used as the default for all workflows under your tenant. You can override them per workflow, but this is generally discouraged as it complicates maintenance.

## Using a Hosted OPN Instance

If you do not want to run your own instance of OPN, you may use one of our hosted environments:

- **Mainnet**: `https://opn.mainnet.blocktrust.dev`
- **Preprod**: `https://opn.preprod.blocktrust.dev`

For testing on **preprod**, you may use the following shared test wallet ID: `beb041bbbc689c6762f7fb743735e9c39df25ad5`

## Technical Details

Once configured, the Credential Workflow Platform will:

- Use the OPN Registrar URL to send DID operations (create, update, deactivate).
- Authenticate and sign transactions using the configured Wallet ID.
- Automatically select the correct Universal Registrar endpoint based on your settings.

This setup abstracts away the underlying API complexity and allows you to define DID-related steps in your workflow designer without additional configuration.
