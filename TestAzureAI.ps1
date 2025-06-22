# TestAzureAI.ps1
# Simple test to verify Azure AI configuration works

Write-Host "=== Testing Azure AI Configuration ===" -ForegroundColor Green

# Test Azure CLI access
Write-Host "`n1. Testing Azure CLI access..." -ForegroundColor Yellow
try {
    $account = az account show --query "name" --output tsv
    Write-Host "✓ Connected to Azure subscription: $account" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure CLI access failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test OpenAI resource access
Write-Host "`n2. Testing OpenAI resource access..." -ForegroundColor Yellow
try {
    $endpoint = az cognitiveservices account show --name "tooensure-cursor" --resource-group "rg-autonomousa" --query "properties.endpoint" --output tsv
    Write-Host "✓ OpenAI endpoint: $endpoint" -ForegroundColor Green
    
    $apiKey = az cognitiveservices account keys list --name "tooensure-cursor" --resource-group "rg-autonomousa" --query "key1" --output tsv
    Write-Host "✓ API Key retrieved successfully" -ForegroundColor Green
    
    $deployments = az cognitiveservices account deployment list --name "tooensure-cursor" --resource-group "rg-autonomousa" --query "[].{Name:name, Model:properties.model.name, Status:properties.provisioningState}" --output table
    Write-Host "✓ Deployments:" -ForegroundColor Green
    Write-Host $deployments
} catch {
    Write-Host "✗ OpenAI resource access failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test direct API call
Write-Host "`n3. Testing direct API call..." -ForegroundColor Yellow
try {
    $endpoint = az cognitiveservices account show --name "tooensure-cursor" --resource-group "rg-autonomousa" --query "properties.endpoint" --output tsv
    $apiKey = az cognitiveservices account keys list --name "tooensure-cursor" --resource-group "rg-autonomousa" --query "key1" --output tsv
    
    $headers = @{
        "Content-Type" = "application/json"
        "api-key" = $apiKey
    }
    
    $body = @{
        messages = @(
            @{
                role = "user"
                content = "Hello! Can you help me understand how self-evolution works in AI systems?"
            }
        )
        max_tokens = 100
        temperature = 0.7
    } | ConvertTo-Json -Depth 10
    
    $response = Invoke-RestMethod -Uri "$endpoint/openai/deployments/gpt-4.1/chat/completions?api-version=2024-02-15-preview" -Method POST -Headers $headers -Body $body -TimeoutSec 30
    
    Write-Host "✓ Direct API call successful!" -ForegroundColor Green
    Write-Host "  Response: $($response.choices[0].message.content.Substring(0, [Math]::Min(100, $response.choices[0].message.content.Length)))..." -ForegroundColor Green
} catch {
    Write-Host "✗ Direct API call failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Green
Write-Host "Azure AI configuration test completed!" -ForegroundColor White 