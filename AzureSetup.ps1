# AzureSetup.ps1
# Automated setup script for Azure AI configuration in the Strange Loop System
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$OpenAIResourceName,
    
    [string]$SubscriptionName = "AutonomousAgentFramwork",
    
    [switch]$SkipConfirmation
)

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to check if Azure CLI is installed
function Test-AzureCLI {
    try {
        $null = az --version
        return $true
    }
    catch {
        return $false
    }
}

# Function to check if user is logged in
function Test-AzureLogin {
    try {
        $account = az account show 2>$null | ConvertFrom-Json
        return $account -ne $null
    }
    catch {
        return $false
    }
}

# Main setup function
function Start-AzureSetup {
    Write-ColorOutput "=== Azure AI Setup for Strange Loop System ===" "Green"
    Write-ColorOutput "Setting up Azure AI configuration..." "Yellow"
    
    # Check Azure CLI
    if (-not (Test-AzureCLI)) {
        Write-ColorOutput "Azure CLI is not installed. Please install it first." "Red"
        Write-ColorOutput "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" "Yellow"
        return
    }
    
    # Check login status
    if (-not (Test-AzureLogin)) {
        Write-ColorOutput "Not logged into Azure. Please run 'az login' first." "Red"
        return
    }
    
    # Set subscription
    Write-ColorOutput "Setting subscription to: $SubscriptionName" "Yellow"
    az account set --subscription $SubscriptionName
    
    # Verify subscription
    $currentSub = az account show --query "name" --output tsv
    if ($currentSub -ne $SubscriptionName) {
        Write-ColorOutput "Warning: Could not set subscription to $SubscriptionName. Current: $currentSub" "Yellow"
    }
    
    # Get API Key
    Write-ColorOutput "Retrieving API Key..." "Yellow"
    $apiKey = az cognitiveservices account keys list --name $OpenAIResourceName --resource-group $ResourceGroupName --query "key1" --output tsv
    if (-not $apiKey) {
        Write-ColorOutput "Error: Could not retrieve API key. Check resource name and permissions." "Red"
        return
    }
    Write-ColorOutput "API Key retrieved successfully" "Green"
    
    # Get Endpoint
    Write-ColorOutput "Retrieving endpoint..." "Yellow"
    $endpoint = az cognitiveservices account show --name $OpenAIResourceName --resource-group $ResourceGroupName --query "properties.endpoint" --output tsv
    if (-not $endpoint) {
        Write-ColorOutput "Error: Could not retrieve endpoint. Check resource name and permissions." "Red"
        return
    }
    Write-ColorOutput "Endpoint: $endpoint" "Green"
    
    # Get Model Deployments
    Write-ColorOutput "Retrieving model deployments..." "Yellow"
    $deployments = az cognitiveservices account deployment list --name $OpenAIResourceName --resource-group $ResourceGroupName --query "[].{Name:name, Model:properties.model.name, Status:properties.provisioningState}" --output table
    Write-ColorOutput "Available Deployments:" "Yellow"
    Write-Host $deployments
    
    # Check for required deployments
    $deploymentNames = az cognitiveservices account deployment list --name $OpenAIResourceName --resource-group $ResourceGroupName --query "[].name" --output tsv
    $hasGpt4 = $deploymentNames -contains "gpt-4"
    $hasEmbedding = $deploymentNames -contains "text-embedding-ada-002"
    
    if (-not $hasGpt4) {
        Write-ColorOutput "Warning: No 'gpt-4' deployment found. Available: $deploymentNames" "Yellow"
    }
    if (-not $hasEmbedding) {
        Write-ColorOutput "Warning: No 'text-embedding-ada-002' deployment found. Available: $deploymentNames" "Yellow"
    }
    
    # Generate appsettings.json content
    Write-ColorOutput "Generating appsettings.json..." "Yellow"
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
"@
    
    # Save to appsettings.json
    $appsettingsContent | Out-File -FilePath "appsettings.json" -Encoding UTF8
    Write-ColorOutput "Generated appsettings.json" "Green"
    
    # Generate environment variables file
    Write-ColorOutput "Generating .env file..." "Yellow"
    $envContent = @"
AZURE_OPENAI_ENDPOINT=$endpoint
AZURE_OPENAI_API_KEY=$apiKey
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4
AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME=text-embedding-ada-002
"@
    
    $envContent | Out-File -FilePath ".env" -Encoding UTF8
    Write-ColorOutput "Generated .env file" "Green"
    
    # Generate test script
    Write-ColorOutput "Generating test script..." "Yellow"
    $testScriptContent = @"
# TestStrangeLoop.ps1
# Generated test script for Strange Loop System

`$baseUrl = "https://localhost:7001"

Write-Host "Testing Strange Loop System..." -ForegroundColor Green

# Test health endpoint
Write-Host "Testing health endpoint..." -ForegroundColor Yellow
try {
    `$health = Invoke-RestMethod -Uri "`$baseUrl/api/azureaiagent/health" -Method GET
    Write-Host "Health Status: `$(`$health.status)" -ForegroundColor Green
} catch {
    Write-Host "Health check failed: `$(`$_.Exception.Message)" -ForegroundColor Red
}

# Test strange loop statistics
Write-Host "Testing strange loop statistics..." -ForegroundColor Yellow
try {
    `$stats = Invoke-RestMethod -Uri "`$baseUrl/api/selfevolution/strange-loop/statistics" -Method GET
    Write-Host "Evolution Statistics: `$(`$stats.totalEvolutions) total evolutions" -ForegroundColor Green
} catch {
    Write-Host "Statistics check failed: `$(`$_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Test completed!" -ForegroundColor Green
"@
    
    $testScriptContent | Out-File -FilePath "TestStrangeLoop.ps1" -Encoding UTF8
    Write-ColorOutput "Generated TestStrangeLoop.ps1" "Green"
    
    # Summary
    Write-ColorOutput "`n=== Setup Summary ===" "Green"
    Write-ColorOutput "Resource Group: $ResourceGroupName" "White"
    Write-ColorOutput "OpenAI Resource: $OpenAIResourceName" "White"
    Write-ColorOutput "Endpoint: $endpoint" "White"
    Write-ColorOutput "API Key: [HIDDEN]" "White"
    Write-ColorOutput "Files Generated:" "White"
    Write-ColorOutput "  - appsettings.json" "White"
    Write-ColorOutput "  - .env" "White"
    Write-ColorOutput "  - TestStrangeLoop.ps1" "White"
    
    Write-ColorOutput "`nNext Steps:" "Green"
    Write-ColorOutput "1. Build the project: dotnet build UltraGenericSystem.csproj" "White"
    Write-ColorOutput "2. Run the application: dotnet run --project UltraGenericSystem.csproj" "White"
    Write-ColorOutput "3. Test the system: .\TestStrangeLoop.ps1" "White"
    
    Write-ColorOutput "`nSetup complete! The Strange Loop System is ready to use Azure AI services." "Green"
}

# Function to show help
function Show-Help {
    Write-ColorOutput "Azure AI Setup Script for Strange Loop System" "Green"
    Write-ColorOutput "Usage: .\AzureSetup.ps1 -ResourceGroupName <rg-name> -OpenAIResourceName <resource-name> [options]" "White"
    Write-ColorOutput "" "White"
    Write-ColorOutput "Parameters:" "Yellow"
    Write-ColorOutput "  -ResourceGroupName    The resource group containing the OpenAI resource" "White"
    Write-ColorOutput "  -OpenAIResourceName   The name of the OpenAI/Cognitive Services resource" "White"
    Write-ColorOutput "  -SubscriptionName     Azure subscription name (default: AutonomousAgentFramwork)" "White"
    Write-ColorOutput "  -SkipConfirmation     Skip confirmation prompts" "White"
    Write-ColorOutput "" "White"
    Write-ColorOutput "Example:" "Yellow"
    Write-ColorOutput "  .\AzureSetup.ps1 -ResourceGroupName 'my-ai-rg' -OpenAIResourceName 'my-openai-resource'" "White"
}

# Main execution
if ($args -contains "-h" -or $args -contains "--help" -or $args -contains "-?") {
    Show-Help
    return
}

if (-not $SkipConfirmation) {
    Write-ColorOutput "This script will:" "Yellow"
    Write-ColorOutput "1. Set Azure subscription to: $SubscriptionName" "White"
    Write-ColorOutput "2. Retrieve API key and endpoint from: $OpenAIResourceName" "White"
    Write-ColorOutput "3. Generate appsettings.json and .env files" "White"
    Write-ColorOutput "4. Create a test script" "White"
    Write-ColorOutput "" "White"
    
    $confirmation = Read-Host "Do you want to continue? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-ColorOutput "Setup cancelled." "Yellow"
        return
    }
}

Start-AzureSetup 