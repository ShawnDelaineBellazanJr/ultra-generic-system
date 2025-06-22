# Azure AI Setup Guide for Strange Loop System

## Overview

This guide shows how to use Azure CLI to configure Azure AI services for the Strange Loop Self-Evolution System. We'll set up the necessary resources in the `AutonomousAgentFramwork` subscription and configure the system to use real Azure AI endpoints.

## Prerequisites

1. **Azure CLI installed**
2. **Azure subscription access** (AutonomousAgentFramwork)
3. **Azure AI Studio access**
4. **PowerShell or Command Prompt**

## Step 1: Azure CLI Authentication and Setup

### Login to Azure
```bash
# Login to Azure
az login

# Set the subscription
az account set --subscription "AutonomousAgentFramwork"

# Verify current subscription
az account show
```

### Install Azure AI CLI Extension
```bash
# Install the Azure AI CLI extension
az extension add --name ai

# Update extensions
az extension update --name ai
```

## Step 2: Discover Azure AI Resources

### List AI Services
```bash
# List all AI services in the subscription
az ai list --resource-group "*" --subscription "AutonomousAgentFramwork"

# List specific resource groups
az group list --query "[?contains(name, 'ai') || contains(name, 'openai') || contains(name, 'cognitive')].{Name:name, Location:location}" --output table
```

### Find OpenAI Resources
```bash
# List OpenAI resources
az cognitiveservices account list --subscription "AutonomousAgentFramwork" --query "[?kind=='OpenAI'].{Name:name, ResourceGroup:resourceGroup, Location:location, Endpoint:properties.endpoint}" --output table

# Get specific OpenAI resource details
az cognitiveservices account show --name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork"
```

### Get Model Deployments
```bash
# List model deployments
az cognitiveservices account deployment list --name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork"

# Get specific deployment details
az cognitiveservices account deployment show --name "gpt-4" --account-name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork"
```

## Step 3: Get API Keys and Endpoints

### Get API Key
```bash
# Get the API key for your OpenAI resource
az cognitiveservices account keys list --name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork" --query "key1" --output tsv
```

### Get Endpoint URL
```bash
# Get the endpoint URL
az cognitiveservices account show --name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork" --query "properties.endpoint" --output tsv
```

### Get Model Information
```bash
# List available models
az cognitiveservices account deployment list --name "your-openai-resource-name" --resource-group "your-resource-group" --subscription "AutonomousAgentFramwork" --query "[].{Name:name, Model:properties.model.name, Status:properties.provisioningState}" --output table
```

## Step 4: PowerShell Script for Automated Setup

Create a PowerShell script to automate the setup:

```powershell
# AzureSetup.ps1
param(
    [string]$ResourceGroupName,
    [string]$OpenAIResourceName,
    [string]$SubscriptionName = "AutonomousAgentFramwork"
)

Write-Host "Setting up Azure AI configuration for Strange Loop System..." -ForegroundColor Green

# Set subscription
az account set --subscription $SubscriptionName
Write-Host "Set subscription to: $SubscriptionName" -ForegroundColor Yellow

# Get API Key
$apiKey = az cognitiveservices account keys list --name $OpenAIResourceName --resource-group $ResourceGroupName --query "key1" --output tsv
Write-Host "Retrieved API Key" -ForegroundColor Yellow

# Get Endpoint
$endpoint = az cognitiveservices account show --name $OpenAIResourceName --resource-group $ResourceGroupName --query "properties.endpoint" --output tsv
Write-Host "Retrieved Endpoint: $endpoint" -ForegroundColor Yellow

# Get Model Deployments
$deployments = az cognitiveservices account deployment list --name $OpenAIResourceName --resource-group $ResourceGroupName --query "[].{Name:name, Model:properties.model.name, Status:properties.provisioningState}" --output table
Write-Host "Available Deployments:" -ForegroundColor Yellow
Write-Host $deployments

# Generate appsettings.json content
$appsettingsContent = @"
{
  "AzureOpenAI": {
    "Endpoint": "$endpoint",
    "ApiKey": "$apiKey",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002",
    "MaxTokens": 4000,
    "Temperature": 0.7,
    "EnableStreaming": true,
    "EnableFunctionCalling": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
"@

# Save to appsettings.json
$appsettingsContent | Out-File -FilePath "appsettings.json" -Encoding UTF8
Write-Host "Generated appsettings.json" -ForegroundColor Green

# Generate environment variables
$envContent = @"
AZURE_OPENAI_ENDPOINT=$endpoint
AZURE_OPENAI_API_KEY=$apiKey
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4
AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME=text-embedding-ada-002
"@

$envContent | Out-File -FilePath ".env" -Encoding UTF8
Write-Host "Generated .env file" -ForegroundColor Green

Write-Host "Setup complete! You can now run the Strange Loop System." -ForegroundColor Green
```

## Step 5: Configure the Strange Loop System

### Update appsettings.json
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002",
    "MaxTokens": 4000,
    "Temperature": 0.7,
    "EnableStreaming": true,
    "EnableFunctionCalling": true
  },
  "StrangeLoop": {
    "EnableSelfEvolution": true,
    "MaxEvolutionSteps": 10,
    "EvolutionTimeout": "00:10:00",
    "EnableHumanApproval": false,
    "AllowedNamespaces": [
      "UltraGenericSystem.Generated",
      "System.Threading.Tasks",
      "System.Collections.Generic"
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "UltraGenericSystem.Services.SelfEvolution": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables (Alternative)
```bash
# Set environment variables
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key-here"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4"
export AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME="text-embedding-ada-002"
```

## Step 6: Test the Strange Loop System

### Build and Run
```bash
# Build the project
dotnet build UltraGenericSystem.csproj

# Run the application
dotnet run --project UltraGenericSystem.csproj
```

### Test Strange Loop API
```bash
# Test the strange loop endpoint
curl -X POST "https://localhost:7001/api/selfevolution/strange-loop" \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "your-agent-id",
    "evolutionType": "SelfModification",
    "parameters": {
      "targetImprovement": "Performance",
      "confidenceThreshold": 0.8
    },
    "enableHumanApproval": false,
    "timeout": "00:10:00",
    "maxEvolutionSteps": 10
  }'
```

### PowerShell Test Script
```powershell
# TestStrangeLoop.ps1
$baseUrl = "https://localhost:7001"

# Test health endpoint
Write-Host "Testing health endpoint..." -ForegroundColor Yellow
$health = Invoke-RestMethod -Uri "$baseUrl/api/azureaiagent/health" -Method GET
Write-Host "Health Status: $($health.status)" -ForegroundColor Green

# Test strange loop readiness
Write-Host "Testing agent readiness..." -ForegroundColor Yellow
$agentId = "your-agent-id"  # Replace with actual agent ID
$readiness = Invoke-RestMethod -Uri "$baseUrl/api/selfevolution/strange-loop/ready/$agentId" -Method GET
Write-Host "Agent Ready: $readiness" -ForegroundColor Green

# Test strange loop execution
Write-Host "Testing strange loop execution..." -ForegroundColor Yellow
$evolutionRequest = @{
    agentId = $agentId
    evolutionType = "SelfModification"
    parameters = @{
        targetImprovement = "Performance"
        confidenceThreshold = 0.8
    }
    enableHumanApproval = $false
    timeout = "00:10:00"
    maxEvolutionSteps = 10
} | ConvertTo-Json -Depth 10

$evolutionResult = Invoke-RestMethod -Uri "$baseUrl/api/selfevolution/strange-loop" -Method POST -Body $evolutionRequest -ContentType "application/json"
Write-Host "Evolution Result: $($evolutionResult.success)" -ForegroundColor Green
Write-Host "Evolution Steps: $($evolutionResult.evolutionSteps.Count)" -ForegroundColor Green
```

## Step 7: Monitor and Debug

### Check Logs
```bash
# View application logs
dotnet run --project UltraGenericSystem.csproj --environment Development

# Or check log files
tail -f logs/strange-loop-*.log
```

### Azure Monitor
```bash
# Get resource metrics
az monitor metrics list --resource "/subscriptions/your-subscription-id/resourceGroups/your-resource-group/providers/Microsoft.CognitiveServices/accounts/your-openai-resource" --metric "TotalCalls" --interval PT1H
```

### Test Individual Components
```bash
# Test Azure AI Agent
curl -X POST "https://localhost:7001/api/azureaiagent/message" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hello, can you help me evolve my capabilities?",
    "conversationId": "test-conversation"
  }'

# Test Code Generation
curl -X POST "https://localhost:7001/api/selfevolution/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "templateName": "Entity",
    "parameters": {
      "EntityName": "TestEntity",
      "Properties": {
        "Name": "string",
        "Value": "int"
      }
    },
    "outputPath": "Models/Generated/TestEntity.cs",
    "compileImmediately": true
  }'
```

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   ```bash
   # Re-authenticate
   az login
   az account set --subscription "AutonomousAgentFramwork"
   ```

2. **Resource Not Found**
   ```bash
   # List all resources
   az resource list --subscription "AutonomousAgentFramwork" --query "[?type=='Microsoft.CognitiveServices/accounts'].{Name:name, ResourceGroup:resourceGroup, Kind:kind}" --output table
   ```

3. **API Key Issues**
   ```bash
   # Regenerate API key
   az cognitiveservices account keys regenerate --name "your-resource-name" --resource-group "your-resource-group" --key-name key1
   ```

4. **Model Deployment Issues**
   ```bash
   # Check deployment status
   az cognitiveservices account deployment show --name "gpt-4" --account-name "your-resource-name" --resource-group "your-resource-group"
   ```

### Debug Mode
```bash
# Run in debug mode
dotnet run --project UltraGenericSystem.csproj --environment Development --verbosity detailed
```

## Security Considerations

1. **API Key Security**
   - Never commit API keys to source control
   - Use Azure Key Vault for production
   - Rotate keys regularly

2. **Network Security**
   - Use private endpoints for production
   - Configure firewall rules
   - Enable Azure AD authentication

3. **Access Control**
   - Use least privilege principle
   - Monitor API usage
   - Set up alerts for unusual activity

## Next Steps

1. **Run the setup script** to configure Azure AI
2. **Test the strange loop system** with real Azure endpoints
3. **Monitor the evolution process** and analyze results
4. **Iterate and improve** based on performance

The Strange Loop System is now ready to use real Azure AI services for self-evolution! 