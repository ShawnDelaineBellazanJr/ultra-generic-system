# DiscoverAzureAI.ps1
# Script to discover Azure AI resources in the AutonomousAgentFramwork subscription

param(
    [string]$SubscriptionName = "AutonomousAgentFramwork"
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

# Main discovery function
function Start-AzureDiscovery {
    Write-ColorOutput "=== Azure AI Resource Discovery ===" "Green"
    Write-ColorOutput "Discovering Azure AI resources in subscription: $SubscriptionName" "Yellow"
    
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
    
    Write-ColorOutput "`n=== Resource Groups ===" "Green"
    $resourceGroups = az group list --subscription $SubscriptionName --query "[?contains(name, 'ai') || contains(name, 'openai') || contains(name, 'cognitive') || contains(name, 'ml')].{Name:name, Location:location}" --output table
    Write-Host $resourceGroups
    
    Write-ColorOutput "`n=== All Resource Groups ===" "Green"
    $allResourceGroups = az group list --subscription $SubscriptionName --query "[].{Name:name, Location:location}" --output table
    Write-Host $allResourceGroups
    
    Write-ColorOutput "`n=== Cognitive Services Accounts ===" "Green"
    $cognitiveServices = az cognitiveservices account list --subscription $SubscriptionName --query "[].{Name:name, ResourceGroup:resourceGroup, Kind:kind, Location:location, Endpoint:properties.endpoint}" --output table
    Write-Host $cognitiveServices
    
    Write-ColorOutput "`n=== OpenAI Resources ===" "Green"
    $openaiResources = az cognitiveservices account list --subscription $SubscriptionName --query "[?kind=='OpenAI'].{Name:name, ResourceGroup:resourceGroup, Location:location, Endpoint:properties.endpoint}" --output table
    Write-Host $openaiResources
    
    Write-ColorOutput "`n=== AI Services ===" "Green"
    try {
        $aiServices = az ai list --subscription $SubscriptionName --resource-group "*" --query "[].{Name:name, ResourceGroup:resourceGroup, Kind:kind}" --output table
        Write-Host $aiServices
    } catch {
        Write-ColorOutput "AI CLI extension not available or no AI services found" "Yellow"
    }
    
    Write-ColorOutput "`n=== Machine Learning Workspaces ===" "Green"
    try {
        $mlWorkspaces = az ml workspace list --subscription $SubscriptionName --query "[].{Name:name, ResourceGroup:resourceGroup, Location:location}" --output table
        Write-Host $mlWorkspaces
    } catch {
        Write-ColorOutput "ML CLI extension not available or no ML workspaces found" "Yellow"
    }
    
    # If OpenAI resources found, show deployments
    if ($openaiResources -and $openaiResources -ne "[]") {
        Write-ColorOutput "`n=== OpenAI Deployments ===" "Green"
        $openaiAccounts = az cognitiveservices account list --subscription $SubscriptionName --query "[?kind=='OpenAI'].{Name:name, ResourceGroup:resourceGroup}" --output json | ConvertFrom-Json
        
        foreach ($account in $openaiAccounts) {
            Write-ColorOutput "`nDeployments for: $($account.Name) (RG: $($account.ResourceGroup))" "Yellow"
            try {
                $deployments = az cognitiveservices account deployment list --name $account.Name --resource-group $account.ResourceGroup --query "[].{Name:name, Model:properties.model.name, Status:properties.provisioningState}" --output table
                Write-Host $deployments
            } catch {
                Write-ColorOutput "  No deployments found or access denied" "Red"
            }
        }
    }
    
    Write-ColorOutput "`n=== Summary ===" "Green"
    Write-ColorOutput "To use the AzureSetup.ps1 script, you'll need:" "Yellow"
    Write-ColorOutput "1. Resource Group name" "White"
    Write-ColorOutput "2. OpenAI/Cognitive Services resource name" "White"
    Write-ColorOutput "" "White"
    Write-ColorOutput "Example usage:" "Yellow"
    Write-ColorOutput "  .\AzureSetup.ps1 -ResourceGroupName 'your-rg-name' -OpenAIResourceName 'your-openai-resource'" "White"
    
    Write-ColorOutput "`nDiscovery complete!" "Green"
}

# Main execution
Start-AzureDiscovery 