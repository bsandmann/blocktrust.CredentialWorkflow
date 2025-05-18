---
title: Creating and Updating DIDs
layout: default
parent: User Guides
nav_order: 1
---

# Creating and Updating DIDs

The following sections provide a comprehensive guide on how to create and update Decentralized Identifiers (DIDs) and setup an issuing keypair using
the Blocktrust Credential Workflow platform.

## Prerequisites for Creating and Updating DIDs
### OpenPrismNode (OPN) Instance
Before you can create or update DIDs, ensure that you the connection Details and the walletId of a OpenPrismNode (OPN) instance.
If you are starting without any knowledge of what the OpenPrismNode (OPN) is you can read the OPN documentation here: https://bsandmann.github.io/OpenPrismNode/
This documentation provides step-by-step instructions for:
- Installing and running OpenPrismNode
- Creating and funding a wallet
- Managing DID using the OPN and the Universal Registrar endpoints. If you choose to use the universal registar endpoints directly to create a DID, you might not need the Workflow platform at all. But note, that the configuration options are a bit more complex and require either cURL commands a dedicated application. The OPN does only provide a UI for wallet management, not for DID management.

In case you don't want to run your own OPN instance, you can also use the hosted OPN instance at `https://opn.mainnet.blocktrust.dev` or `https://opn.preprod.blocktrust.dev`.
In the case of the hosted OPN instance for **preprod**, you can use this walletId for testing purposes ``beb041bbbc689c6762f7fb743735e9c39df25ad5``. If you need a custom Mainnet-Wallet on our hosted instances, please reach out to us.

### Registrar Settings
If you have a URL to a OPN instance and a walletId, you can set up the registrar settings in the Blocktrust Credential Workflow platform. Also see (here)[DidRegistrarSettings.md] for more information on the settings.
**Access the DID Registrar Settings**:
   - Navigate to the settings page for your tenant
   - Click on "DID Registrar Settings"
   - Provide the required data and click "Save". This now allows you to use the same settings in all your workflows. Alternatively you can also use custom settings for each workflow, but this is not recommended as it makes the management of the workflows more complex.

## Creating DIDs

To build a workflow that creates a DID we first need a trigger. To simply things we will use a manual trigger, which
allows us to execute the workflow manually.

1. **Create a new workflow**:
    - Navigate to the Workflows page
    - Click "Create New" to open the workflow designer
2. Select **Add Trigger or Action**
    - Select **Triggers** on the right side and select **Manual Trigger**
    - And then **Manual triggering**. This now creates the trigger as the first step of the workflow.
3. Select **Add Trigger or Action** again an now select
   - **Actions** on the right side and select **DID**
   - And then **Create DID**. This now creates the action as the second step of the workflow.
   - If you setup the DID registrar settings in the previous step, you can now select the **Use tenant settings** option. This will use the settings you configured in the previous step. If you want to use custom settings, you can also do this by selecting the **Use custom settings** option and providing the OPN URL and the walletId.
   - For the Verifaction Method you we can start with a single verification method. You can add more later if needed.
     - Key-Id is per default "key-1"
     - For purpose you can select one of the following options:
       - Authentication
       - KeyAgreement
       - AssertionMethod
       - CapabilityInvocation
       - CapabilityDelegation
     - Select **AssertionMethod** for this example, as we will be using this DID later to issue credentials.
     - For Curve you can select one of the following options:
       - secp256k1
       - Ed25519
     - Select **secp256k1** for this example, as this is the default option.
   - Click on **Add Service** and add a **LinkedDomain**-type service with a URL of your choice. Eg. "https://workflow.blocktrust.dev"
   - Click on **Save** to save the action.
4. **Rename and save the workflow**:
   - Click on the **Workflow Name** at the top left corner of the screen. Edit the name to something meaningful, like "Create DID Workflow".
   - Click on the "Save" button to save the workflow
5. **Execute** the workflow:
   - Click on the **Execute** button on the overview page to run the workflow manually.
   - This will contact the OPN, create a DID and return the result in the logs.
   - This process may take a few seconds, as the written will be published to the blockchain.
6. **Check the logs**:
   - Since we did not use a final action to send the result somewhere e.g. a email action, we can go to the logs (top right corner) and check the result of the workflow.

    The output in the logs may look something like this:
    ```json
    {
      "state": "finished",
      "did": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
      "secret": {
        "verificationMethod": [
          {
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "purpose": [
              "masterKey"
            ],
            "privateKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "c_n63Dr3BHlceASIKnfuzL6p77dMrEOWVafTSIiDqdY",
              "y": "ZWGG008hovdmmsf-lkDGm9EDbx2FmM3iQTQwFLi342Y",
              "d": "J-1mqeNl-9zoRTWYBGfCbHBtd_1Tc7xjYXnN9Chxq5w"
            }
          },
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-1",
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "purpose": [
              "assertionMethod"
            ],
            "privateKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "rS0gI6S32Vt_9wO_mhO820oAib86X5xKEiZyUanpxSw",
              "y": "gl-w_gJX4CbQmgW6BDoKXbdLVf5MQkwUStOk6n8TnG0",
              "d": "02nnKLLcs9weejHteLuLryrVTQ6R1ffPU_9DP3qj5dU"
            }
          }
        ]
      },
      "didDocument": {
        "@context": [
          "https://www.w3.org/ns/did/v1",
          "https://w3id.org/security/suites/jws-2020/v1"
        ],
        "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
        "verificationMethod": [
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-1",
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "publicKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "rS0gI6S32Vt_9wO_mhO820oAib86X5xKEiZyUanpxSw",
              "y": "gl-w_gJX4CbQmgW6BDoKXbdLVf5MQkwUStOk6n8TnG0"
            }
          }
        ],
        "assertionMethod": [
          "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-1"
        ],
        "service": [
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#service-1",
            "type": "LinkedDomain",
            "serviceEndpoint": "https://workflow.blocktrust.dev/"
          }
        ]
      }
    }
    ```

    For more information on the output, please refer to the OPN documentation or the [DID Universal Registrar documentation](https://identity.foundation/did-registration/).
    But the important thing to note here is the secret section, as it contains the private keys for the DID. The first key is always the **masterKey** which is used to make changes to the DID itself. The second key in there is the issuing key, we explicitly set to **assertionMethod** in the previous step. This key is used to sign credentials and other documents. The service section contains the service we added in the previous step. Take note of these values:
   ```
   "x": "rS0gI6S32Vt_9wO_mhO820oAib86X5xKEiZyUanpxSw",
   "y": "gl-w_gJX4CbQmgW6BDoKXbdLVf5MQkwUStOk6n8TnG0",
   "d": "02nnKLLcs9weejHteLuLryrVTQ6R1ffPU_9DP3qj5dU"
   ```
   The first two are the x and y values of the publicKey in a Base64Url encoded format. The third one is the private key in a Base64Url encoded format. 

7. **Issuing Key setup**
    - Now to to the profile page and select the **Issuing Keys** tab. For more information on the Issuing Keys, please refer to the [Issuing Keys Settings documentation](IssuingKeysSettings.md).
    - Enter the following values: 
      - A name of your choice for the issuing key
      - The DID you just created (in our example this would be `did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05ste36f1b7454aeb2b8e1ecd0c764eca`)
      - The KeyType which *must* be set to *secp256k1*
      - The PublicKey which is the x and y values from the previous step ("two part variant" / "uncompressed variant")
        - `rS0gI6S32Vt_9wO_mhO820oAib86X5xKEiZyUanpxSw`
        - `gl-w_gJX4CbQmgW6BDoKXbdLVf5MQkwUStOk6n8TnG0`
      - The PrivateKey which is the d value from the previous step. In our example:
        - `02nnKLLcs9weejHteLuLryrVTQ6R1ffPU_9DP3qj5dU`
   This Issuing-Key-pair will later allow us to issuing a W3C Verifiable  Credential.    

8. Updating DIDs
    - To update a DID, you can use the same workflow as above, but select the **Update DID** action instead of the **Create DID** action. This will allow you to update the existing DID with new values.
    - You can also use the **Deactivate DID** action to deactivate a DID if needed (not shown in this example).
    - The important part is to use the private key of the masterKey to sign the update request. In our example this means using this value `J-1mqeNl-9zoRTWYBGfCbHBtd_1Tc7xjYXnN9Chxq5w` ("d") as the **Masterkey-Secret**.
    - Lets add a second key to the DID as an authentication key. The result in the logs would look something like this:
    ```json
    {
      "state": "finished",
      "did": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
      "secret": {
        "verificationMethod": [
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-2",
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "purpose": [
              "authentication"
            ],
            "privateKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "js6c0st4Vmg0ck6buVo8KtobAeMWwCMzV-k0dNVynvc",
              "y": "zrl0fDBynI9rOp_o8ZAW3Edd8Z90GzRfAS9AD4dKMYQ",
              "d": "Z3SNYAJnWCb-8U32dcUSoSJzCqYuJktSiHiP38Sjw0M"
            }
          }
        ]
      },
      "didDocument": {
        "@context": [
          "https://www.w3.org/ns/did/v1",
          "https://w3id.org/security/suites/jws-2020/v1"
        ],
        "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
        "verificationMethod": [
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-1",
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "publicKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "rS0gI6S32Vt_9wO_mhO820oAib86X5xKEiZyUanpxSw",
              "y": "gl-w_gJX4CbQmgW6BDoKXbdLVf5MQkwUStOk6n8TnG0"
            }
          },
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-2",
            "type": "JsonWebKey2020",
            "controller": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
            "publicKeyJwk": {
              "crv": "secp256k1",
              "kty": "EC",
              "x": "js6c0st4Vmg0ck6buVo8KtobAeMWwCMzV-k0dNVynvc",
              "y": "zrl0fDBynI9rOp_o8ZAW3Edd8Z90GzRfAS9AD4dKMYQ"
            }
          }
        ],
        "authentication": [
          "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-2"
        ],
        "assertionMethod": [
          "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#key-1"
        ],
        "service": [
          {
            "id": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca#service-1",
            "type": "LinkedDomain",
            "serviceEndpoint": "https://workflow.blocktrust.dev/"
          }
        ]
      }
    }
    ```
    We can see the second key listed as `authentication` in the output. In the `secrets` section the private key is also listed.

If you like you can continue with the guide in the next [section](IssuingCredentials.md) to issue a W3C Verifiable Credential using the DID you just created.