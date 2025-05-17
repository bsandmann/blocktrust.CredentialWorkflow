---
title: Verifying Credentials
layout: default
parent: UserGuides
nav_order: 3
---

# Verifying W3C Credentials
In this guide we assume that a user is presenting a W3C Verifiable Credential to the owner of service, either via a Wallet (DIDComm v2) or via some kind of Form. The service owner wants to verify the credential and check if the credential is valid. In this guide we'll send upa backend flow in which the service owner may validate the credential.

## Prerequisites
You have a have a test-Credential to verify. If not, please follow the [Issuing Credentials](../UserGuides/IssuingCredentials.md) guide to issue a test credential.


### Create a new workflow and a trigger
We start by setting put a Http-Request Trigger. The Http-Request Trigger allows you to receive any data over HTTP. This is useful if you want to process any arbitary data, to validate it and continue sending it to some other service. For more information on the Http-Request Trigger, please see the [Http-Request Trigger documentation](../Triggers/HttpTrigger.md).
1. Go to the **Workflows** page and click on the **Create Workflow** button.
2. In the top left rename and Workflow to your liking. For example `Verifying Credential`.
3. Select the **Custom Http Request** from the list of available triggers (under the **Incoming HTTP request** tab).
4. Select **POST** as the HTTP method and add a parameter called `credential` with the type `String`. This is the credential that will be sent to the workflow platfomr
5. Save the workflow.

### Verify the Credential
We now assume that the user is sending a W3C Verifiable Credential to the workflow platform. The credential is sent as a JWT and we want to verify it using the [Verify W3C Credential Action](../Actions/VerifyW3CCredentialAction.md). The Verify W3C Credential Action allows you to verify a W3C Verifiable Credential and check if the credential is valid. This means it performs checks for the signature and the expiration date as well as revocation status (if applicable).
1. Select the **Verify W3C Credential Action** from the list of available actions (under the **Verify credentials** tab).
2. For the **Credential source** select **From Trigger** and the `credential` field. This means that the credential will be verified using the credential sent to the workflow platform using the Http-Request Trigger.

## Validate the Credential
Verification is usally only the first step in a validation process, it proves that the credential is valid and not expired. But in many cases you want to validate the credential against some other data. For example you may want to check if the credential is issued by a trusted issuer or if the credential contains some specific claims. For this we can use the [W3C Validation action](../Actions/W3CValidationAction.md). The difference between the CustomValidation Action and the W3C Validation Action is that the W3C Validation Action is specifically designed to validate W3C Verifiable Credentials. It is able to validate teh credential even if it is not a Json object, but a JWT or base64 encoded JWT.
Lets first revist a sample credential (like the one we created earlier):
    - It started like this `eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJka...` meaning that it is a JWT.
    - The decoded payload looks like this:
     ```json
    {
      "iss": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
      "sub": "did:prism:327ec8ec5a36aed239acb28d27e996311b7049bd92f2f36d63b70326b04745b5",
      "vc": {
        "@context": [
          "https://www.w3.org/2018/credentials/v1"
        ],
        "type": [
          "VerifiableCredential"
        ],
        "issuer": "did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca",
        "expirationDate": "2026-05-16T23:59:59.9999999",
        "validFrom": "2025-05-16T17:06:02.6897954",
        "credentialSubject": {
          "id": "did:prism:327ec8ec5a36aed239acb28d27e996311b7049bd92f2f36d63b70326b04745b5",
          "name": "Björn Sandmann",
          "degree": "Completion of the Blocktrust course"
        }
      },
      "nbf": 1747407962,
      "exp": 1778968799
    }
     ```


1. Select the **W3C Validation Action** from the list of available actions (under the **Validation Actions** tab).
2. For **Credential source** select again **From Trigger** and the `credential` field. 
3. Add a new Validation Rule with the following values:
    - **Exact Value**: `issuer:did:prism:6b7c5e9dd3c0d5bae65d21ea26fe452cf05e36f1b7454aeb2b8e1ecd0c764eca`. This means that the issuer of the credential must be this specific DID. Compare this with the issuer in the decoded payload above.
4. Add another Validation Rule with the following values:
    - **Exact Value**: `credentialSubject.degree:Completion of the Blocktrust course`. 
5. Add another Validation Rule with the following values:
    - **Required Field**: `credentialSubject.name:`. Stating that the name field must be present in the credential, independent of the value.

**Note**: The default setup tries to resolve the DID only against the Cardano mainnet ledger. In our case the issuing DID was written to preprod. To also allow the resolution of that DID, you need to add a OPN node in the fallback configuration. For more information see also the [Fallback configuration](../Settings/Configuration.md) documentation.

## Send the Result
After the validation we want to send the result of the validation back to the service which initially requested the validation. To do this we now use the Http-Request Action