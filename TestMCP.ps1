# Test MCP (Model Context Protocol) Integration with GitHub API
# This script tests the self-evolving system's ability to discover and auto-generate SK plugins from MCP schemas

param(
    [string]$BaseUrl = "https://localhost:7001",
    [string]$GitHubToken = "",
    [string]$TestRepo = "ShawnDelaineBellazanJr/ultra-generic-system"
)

Write-Host "🚀 Testing MCP Integration with GitHub API" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Test Repository: $TestRepo" -ForegroundColor Yellow

# Test 1: Discover GitHub MCP Server
Write-Host "`n📡 Test 1: Discovering GitHub MCP Server..." -ForegroundColor Green

$discoveryRequest = @{
    GitHubToken = $GitHubToken
} | ConvertTo-Json

try {
    $discoveryResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/discover-github" -Method POST -Body $discoveryRequest -ContentType "application/json"
    
    if ($discoveryResponse.success) {
        Write-Host "✅ GitHub MCP Server discovered successfully!" -ForegroundColor Green
        Write-Host "   Capabilities found: $($discoveryResponse.capabilities.Count)" -ForegroundColor White
        Write-Host "   Server: $($discoveryResponse.server.name)" -ForegroundColor White
        Write-Host "   Endpoint: $($discoveryResponse.server.endpoint)" -ForegroundColor White
    } else {
        Write-Host "❌ GitHub MCP Server discovery failed: $($discoveryResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error discovering GitHub MCP Server: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get Connected MCP Servers
Write-Host "`n🔗 Test 2: Getting Connected MCP Servers..." -ForegroundColor Green

try {
    $serversResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/servers" -Method GET
    
    Write-Host "✅ Connected MCP Servers:" -ForegroundColor Green
    foreach ($server in $serversResponse.Keys) {
        $serverInfo = $serversResponse[$server]
        Write-Host "   - $server : $($serverInfo.name) at $($serverInfo.endpoint)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Error getting connected servers: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Repository Information
Write-Host "`n📂 Test 3: Getting Repository Information..." -ForegroundColor Green

$owner, $repo = $TestRepo.Split('/')

try {
    $repoResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/github/repository/$owner/$repo" -Method GET
    
    if ($repoResponse.success) {
        Write-Host "✅ Repository information retrieved successfully!" -ForegroundColor Green
        Write-Host "   Repository: $($repoResponse.owner)/$($repoResponse.repository)" -ForegroundColor White
        Write-Host "   Data retrieved: $($repoResponse.data -ne $null)" -ForegroundColor White
    } else {
        Write-Host "❌ Repository information retrieval failed: $($repoResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error getting repository information: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: List Issues
Write-Host "`n📋 Test 4: Listing Issues..." -ForegroundColor Green

try {
    $issuesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/github/issues/$owner/$repo?state=open" -Method GET
    
    if ($issuesResponse.success) {
        Write-Host "✅ Issues retrieved successfully!" -ForegroundColor Green
        Write-Host "   Repository: $($issuesResponse.owner)/$($issuesResponse.repository)" -ForegroundColor White
        Write-Host "   Issue count: $($issuesResponse.issueCount)" -ForegroundColor White
        Write-Host "   Issues data retrieved: $($issuesResponse.issues -ne $null)" -ForegroundColor White
    } else {
        Write-Host "❌ Issues retrieval failed: $($issuesResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error listing issues: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Execute Custom GitHub Operation
Write-Host "`n⚡ Test 5: Executing Custom GitHub Operation..." -ForegroundColor Green

$operationRequest = @{
    Operation = "get_repository"
    Parameters = @{
        owner = $owner
        repo = $repo
    }
} | ConvertTo-Json

try {
    $operationResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/github/execute" -Method POST -Body $operationRequest -ContentType "application/json"
    
    if ($operationResponse.success) {
        Write-Host "✅ Custom GitHub operation executed successfully!" -ForegroundColor Green
        Write-Host "   Operation: $($operationResponse.operation)" -ForegroundColor White
        Write-Host "   Result retrieved: $($operationResponse.result -ne $null)" -ForegroundColor White
        Write-Host "   Request ID: $($operationResponse.requestId)" -ForegroundColor White
    } else {
        Write-Host "❌ Custom GitHub operation failed: $($operationResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error executing custom GitHub operation: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get Self-Evolution Context
Write-Host "`n🧠 Test 6: Getting Self-Evolution Context..." -ForegroundColor Green

try {
    $contextResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/evolution-context" -Method GET
    
    Write-Host "✅ Self-Evolution Context retrieved successfully!" -ForegroundColor Green
    Write-Host "   Connected servers: $($contextResponse.connectedServers.Count)" -ForegroundColor White
    Write-Host "   Discovered capabilities: $($contextResponse.discoveredCapabilities.Count)" -ForegroundColor White
    Write-Host "   Generated plugins: $($contextResponse.generatedPlugins.Count)" -ForegroundColor White
    Write-Host "   Learning insights: $($contextResponse.learningInsights.Count)" -ForegroundColor White
    Write-Host "   Evolution metrics: $($contextResponse.evolutionMetrics.Count)" -ForegroundColor White
} catch {
    Write-Host "❌ Error getting self-evolution context: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Test MCP Connection
Write-Host "`n🔌 Test 7: Testing MCP Connection..." -ForegroundColor Green

$testRequest = @{
    Name = "Test GitHub MCP"
    Endpoint = "http://localhost:3000"
    Authentication = @{
        Type = "github_token"
        Token = $GitHubToken
    }
} | ConvertTo-Json

try {
    $testResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/test-connection" -Method POST -Body $testRequest -ContentType "application/json"
    
    if ($testResponse.success) {
        Write-Host "✅ MCP connection test successful!" -ForegroundColor Green
        Write-Host "   Endpoint: $($testResponse.endpoint)" -ForegroundColor White
        Write-Host "   Capabilities found: $($testResponse.capabilitiesCount)" -ForegroundColor White
    } else {
        Write-Host "❌ MCP connection test failed: $($testResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error testing MCP connection: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: Trigger Self-Evolving Plugin Generation
Write-Host "`n🧬 Test 8: Triggering Self-Evolving Plugin Generation..." -ForegroundColor Green

$pluginRequest = @{
    Server = @{
        Name = "GitHub MCP"
        Endpoint = "http://localhost:3000"
        Capabilities = @("github_api", "repositories", "issues", "pull_requests")
    }
    Discovery = @{
        Success = $true
        Capabilities = @(
            @{
                Name = "get_repository"
                Type = "tool"
                Description = "Get repository information"
            },
            @{
                Name = "list_issues"
                Type = "tool"
                Description = "List repository issues"
            }
        )
    }
    Goal = "Generate SK plugins from GitHub MCP schema using self-evolving capabilities"
    TargetCapabilities = @("get_repository", "list_issues")
    Preferences = @{
        CodeGenerationApproach = "template_based"
        ErrorHandlingStrategy = "graceful_degradation"
        TestingApproach = "generative_testing"
    }
} | ConvertTo-Json

try {
    $pluginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/mcp/generate-plugins" -Method POST -Body $pluginRequest -ContentType "application/json"
    
    if ($pluginResponse.success) {
        Write-Host "✅ Self-evolving plugin generation triggered successfully!" -ForegroundColor Green
        Write-Host "   Generated plugins: $($pluginResponse.generatedPlugins.Count)" -ForegroundColor White
        Write-Host "   Evolution insights: $($pluginResponse.evolutionInsights.Count)" -ForegroundColor White
        
        foreach ($plugin in $pluginResponse.generatedPlugins) {
            Write-Host "   - Plugin: $($plugin.name) (v$($plugin.version))" -ForegroundColor White
            Write-Host "     Methods: $($plugin.methods.Count)" -ForegroundColor Gray
            Write-Host "     Status: $($plugin.compilationStatus)/$($plugin.integrationStatus)" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ Self-evolving plugin generation failed: $($pluginResponse.errorMessage)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error triggering plugin generation: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 MCP Integration Testing Complete!" -ForegroundColor Cyan
Write-Host "`n💡 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Install and run a GitHub MCP server (e.g., github-mcp-server)" -ForegroundColor White
Write-Host "   2. Set up GitHub token for authentication" -ForegroundColor White
Write-Host "   3. Watch the self-evolving system discover and generate plugins" -ForegroundColor White
Write-Host "   4. Monitor the strange loop recursive learning process" -ForegroundColor White 