# Blocktrust Credential Workflow Platform

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

An open‑source platform for designing, automating, and managing self‑sovereign identity (SSI) workflows using DID Prism. 

---

## 📚 Documentation
Extensive documentation is available to help you get started can be found here: **[https://docs.workflow.blocktrust.dev](https://docs.workflow.blocktrust.dev)**.

A guided video is available here: **[https://youtu.be/9XJUgtR4sR0](https://youtu.be/9XJUgtR4sR0)**.

---

## ✨ Key Features

* **IFTTT‑style workflow engine** – build event‑driven processes with rich conditional logic
* **Multiple triggers** – HTTP, timers, manual actions, forms, wallet interactions
* **Extensive action library**

   * Issue, verify, and revoke **W3C Verifiable Credentials**
   * Create, update, and deactivate **Decentralized Identifiers (DIDs)**
   * Send HTTP requests, emails (SendGrid), DIDComm messages, and JWTs
   * Custom validation using business rules
* **Multi‑tenant** by design – clean separation of organizational data
* **Visual designer** – drag‑and‑drop UI for building workflows
* **Extensible & secure** – written in modern .NET with a plug‑in architecture

---

## 🚀 Quick Start

### 1. Try the hosted instance

[https://workflow.blocktrust.dev](https://workflow.blocktrust.dev)


Create an account and start building.
*Best for development on the **preprod** network.*

### 2. Self‑Host with Docker Compose

```yaml
version: "3.9"

services:
  credential-workflow-web:
    image: ghcr.io/bsandmann/workflowplatform:latest
    container_name: credential-workflow-web
    ports:
      - "80:80"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://0.0.0.0:80

      AppSettings__PrismBaseUrl: https://opn.mainnet.blocktrust.dev
      AppSettings__PrismDefaultLedger: mainnet
      AppSettings__PrismBaseUrlFallback: https://opn.preprod.blocktrust.dev
      AppSettings__PrismDefaultLedgerFallback: preprod

      ConnectionStrings__DefaultConnection: Host=postgres;Username=postgres;Password=postgres;Database=workflowdatabase

      EmailSettings__SendGridKey: <YOUR_SENDGRID_KEY>
      EmailSettings__SendGridFromEmail: <YOUR_FROM_EMAIL>
      EmailSettings__DefaultFromName: Credential Workflow Platform

    depends_on:
      - postgres

  postgres:
    image: postgres:15
    container_name: credential-workflow-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: workflowdatabase
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

> **Tip:** prefer running in a secure environment for production workloads.

### 3. Build from Source

See the documentation for more [information](https://docs.workflow.blocktrust.dev/GettingStarted.html)

---

## 🛠 Supported Standards

* W3C Verifiable Credentials
* Decentralized Identifiers (DIDs)
* DIDComm Messaging
* JWT (JSON Web Tokens)
* OAuth 2.0 / OpenID Connect

---

## 📄 License

This project is licensed under the **Apache License 2.0** – see the [LICENSE](LICENSE) file for details.

---

## 📬 Contact

For questions, feedback, or collaboration opportunities, please reach out to the Blocktrust team at **blocktrust.dev** or join our **Discord** community.
