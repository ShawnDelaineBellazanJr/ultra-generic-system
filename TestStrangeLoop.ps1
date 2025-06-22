# TestStrangeLoop.ps1
# Test script for Strange Loop System with real Azure AI

$baseUrl = "https://localhost:7001"

Write-Host "=== Testing Strange Loop System ===" -ForegroundColor Green
Write-Host "Using Azure AI endpoint: https://tooensure-cursor.openai.azure.com/" -ForegroundColor Yellow

# Test 1: Health endpoint
Write-Host "`n1. Testing health endpoint..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/api/azureaiagent/health" -Method GET -TimeoutSec 10
    Write-Host "✓ Health Status: $($health.status)" -ForegroundColor Green
} catch {
    Write-Host "✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Azure AI Agent message
Write-Host "`n2. Testing Azure AI Agent..." -ForegroundColor Yellow
try {
    $messageRequest = @{
        message = "Hello! Can you help me understand how self-evolution works in AI systems?"
        conversationId = "test-conversation-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    } | ConvertTo-Json -Depth 10

    $response = Invoke-RestMethod -Uri "$baseUrl/api/azureaiagent/message" -Method POST -Body $messageRequest -ContentType "application/json" -TimeoutSec 30
    Write-Host "✓ Azure AI Agent Response: $($response.response.Substring(0, [Math]::Min(100, $response.response.Length)))..." -ForegroundColor Green
} catch {
    Write-Host "✗ Azure AI Agent test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Strange Loop Statistics
Write-Host "`n3. Testing strange loop statistics..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "$baseUrl/api/selfevolution/strange-loop/statistics" -Method GET -TimeoutSec 10
    Write-Host "✓ Evolution Statistics: $($stats.totalEvolutions) total evolutions" -ForegroundColor Green
} catch {
    Write-Host "✗ Statistics check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Code Generation
Write-Host "`n4. Testing code generation..." -ForegroundColor Yellow
try {
    $codeRequest = @{
        templateName = "Entity"
        parameters = @{
            EntityName = "TestEntity"
            Properties = @{
                Name = "string"
                Value = "int"
                CreatedAt = "DateTime"
            }
        }
        outputPath = "Models/Generated/TestEntity.cs"
        compileImmediately = $true
    } | ConvertTo-Json -Depth 10

    $codeResult = Invoke-RestMethod -Uri "$baseUrl/api/selfevolution/generate" -Method POST -Body $codeRequest -ContentType "application/json" -TimeoutSec 30
    Write-Host "✓ Code Generation: $($codeResult.success)" -ForegroundColor Green
    if ($codeResult.success) {
        Write-Host "  Generated file: $($codeResult.generatedFilePath)" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Code generation test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Strange Loop Evolution (if system is ready)
Write-Host "`n5. Testing strange loop evolution..." -ForegroundColor Yellow
try {
    $evolutionRequest = @{
        agentId = "test-agent-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        evolutionType = "SelfModification"
        parameters = @{
            targetImprovement = "Performance"
            confidenceThreshold = 0.8
        }
        enableHumanApproval = $false
        timeout = "00:05:00"
        maxEvolutionSteps = 3
    } | ConvertTo-Json -Depth 10

    $evolutionResult = Invoke-RestMethod -Uri "$baseUrl/api/selfevolution/strange-loop" -Method POST -Body $evolutionRequest -ContentType "application/json" -TimeoutSec 60
    Write-Host "✓ Evolution Result: $($evolutionResult.success)" -ForegroundColor Green
    Write-Host "  Evolution Steps: $($evolutionResult.evolutionSteps.Count)" -ForegroundColor Green
    Write-Host "  Final Status: $($evolutionResult.finalStatus)" -ForegroundColor Green
} catch {
    Write-Host "✗ Evolution test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Green
Write-Host "Tests completed! Check the results above." -ForegroundColor White
Write-Host "If all tests pass, your Strange Loop System is working with Azure AI!" -ForegroundColor Green 