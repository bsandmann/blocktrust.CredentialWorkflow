#################### ATTENTION
# Docker build must be run from /home/bjoern/_Projekte, i.e.:
#   cd /home/bjoern/_Projekte
#   docker build -f blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web/Dockerfile -t credential-workflow-web .
#

# ------------------------------------------------------
# Stage 1: Build & Publish
# ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# 1) Install Node & npm
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_14.x | bash - && \
    apt-get update && \
    apt-get install -y nodejs npm && \
    rm -rf /var/lib/apt/lists/*

# (Optional) Quick sanity check to confirm Node & npm are installed
RUN node -v && npm -v

WORKDIR /src

# 2) Copy *.csproj files first (for faster incremental restore)
COPY blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web/Blocktrust.CredentialWorkflow.Web.csproj \
     blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web/
COPY blocktrust.Core/Blocktrust.Common/Blocktrust.Common.csproj \
     blocktrust.Core/Blocktrust.Common/
COPY blocktrust.Mediator/Blocktrust.Mediator.Client/Blocktrust.Mediator.Client.csproj \
     blocktrust.Mediator/Blocktrust.Mediator.Client/
COPY blocktrust.Mediator/Blocktrust.Mediator.Common/Blocktrust.Mediator.Common.csproj \
     blocktrust.Mediator/Blocktrust.Mediator.Common/
COPY blocktrust.DIDComm/Blocktrust.DIDComm/Blocktrust.DIDComm.csproj \
     blocktrust.DIDComm/Blocktrust.DIDComm/
COPY blocktrust.PeerDID/Blocktrust.PeerDID/Blocktrust.PeerDID.csproj \
     blocktrust.PeerDID/Blocktrust.PeerDID/
COPY blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Core/Blocktrust.CredentialWorkflow.Core.csproj \
     blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Core/
COPY blocktrust.VerifiableCredential/Blocktrust.VerifiableCredential/Blocktrust.VerifiableCredential.csproj \
     blocktrust.VerifiableCredential/Blocktrust.VerifiableCredential/
COPY DidPrismResolverClient/DidPrismResolverClient.csproj \
     DidPrismResolverClient/

# 3) dotnet restore
RUN dotnet restore "blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web/Blocktrust.CredentialWorkflow.Web.csproj"

# 4) Copy the rest of source
COPY blocktrust.CredentialWorkflow blocktrust.CredentialWorkflow
COPY blocktrust.Core blocktrust.Core
COPY blocktrust.Mediator blocktrust.Mediator
COPY blocktrust.DIDComm blocktrust.DIDComm
COPY blocktrust.PeerDID blocktrust.PeerDID
COPY blocktrust.VerifiableCredential blocktrust.VerifiableCredential
COPY DidPrismResolverClient DidPrismResolverClient

#
# 5) NPM install for Tailwind or other Node-based dependencies
#    Ensure a package.json (and package-lock.json) exist in Blocktrust.CredentialWorkflow.Web
#

WORKDIR /src/blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web

# Copy package.json (and optionally package-lock.json) if you haven’t copied them already
# Adjust names/paths if your file is named differently
COPY blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web/package*.json ./

RUN npm install

#
# 6) Build
#
RUN dotnet build "Blocktrust.CredentialWorkflow.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# ------------------------------------------------------
# Stage 2: Publish
# ------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

WORKDIR /src/blocktrust.CredentialWorkflow/Blocktrust.CredentialWorkflow.Web
RUN dotnet publish "Blocktrust.CredentialWorkflow.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------
# Stage 3: Final Runtime Image
# ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app
COPY --from=publish /app/publish .

# Expose your application ports
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Blocktrust.CredentialWorkflow.Web.dll"]
