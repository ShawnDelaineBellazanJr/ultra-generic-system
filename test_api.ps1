# Test API endpoints with proper JSON payloads

Write-Host "Testing Advanced Orchestration Workflow..." -ForegroundColor Green

$workflowPayload = @{
    input = @{
        goal = "Test conversational logging"
        context = "Validate agent service logging"
        developer_context = "Testing unified logging"
    }
    steps = @("plan", "execute", "check")
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/AdvancedOrchestration/workflow" -Method POST -Body $workflowPayload -ContentType "application/json"
    Write-Host "Workflow Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Workflow Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "`nTesting Ollama Chat..." -ForegroundColor Green

$ollamaPayload = @{
    model = "llama3:latest"
    prompt = "Hello, test conversational logging"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/Ollama/chat" -Method POST -Body $ollamaPayload -ContentType "application/json"
    Write-Host "Ollama Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Ollama Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "`nTesting Memory Retrieval..." -ForegroundColor Green

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/Memory/get/ultra_generic_system_knowledge" -Method GET
    Write-Host "Memory Response:" -ForegroundColor Yellow
    Write-Host "Content: $($response.content)" -ForegroundColor Cyan
} catch {
    Write-Host "Memory Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting Self Evolution Statistics..." -ForegroundColor Green

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/SelfEvolution/statistics" -Method GET
    Write-Host "Statistics Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Statistics Error: $($_.Exception.Message)" -ForegroundColor Red
} 