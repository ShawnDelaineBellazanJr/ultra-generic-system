# Azure AI Integration

## ☁️ Azure AI Services Integration

The Strange Loop Self-Evolution System is powered by real Azure AI services, providing the intelligence and reasoning capabilities needed for autonomous evolution. This integration enables the system to think, learn, and improve itself using state-of-the-art AI models.

## 🎯 Current Configuration

### **Azure OpenAI Resource**
- **Resource Name**: `tooensure-cursor`
- **Resource Group**: `rg-autonomousa`
- **Subscription**: `AutonomousAgentFramwork`
- **Endpoint**: `https://tooensure-cursor.openai.azure.com/`
- **Model Deployment**: `gpt-4.1` (Succeeded)

### **Configuration Status**
> **The foundation is solid and ready for the next phase of testing and evolution! 🎯**

## 🚀 Setup and Configuration

### **1. Azure CLI Discovery**
Our system includes automated discovery of Azure AI resources:

```powershell
# Discover available Azure AI resources
.\DiscoverAzureAI.ps1

# This will show:
# - Available resource groups
# - OpenAI resources and endpoints
# - Model deployments
# - API keys (when authorized)
```

### **2. Automated Configuration**
Use our automated setup script to configure the system:

```powershell
# Set up Azure AI configuration
.\AzureSetup.ps1 -ResourceGroupName "rg-autonomousa" -OpenAIResourceName "tooensure-cursor"

# This will:
# - Retrieve API keys and endpoints
# - Generate appsettings.json
# - Create environment variables
# - Generate test scripts
```

### **3. Manual Configuration**
If you prefer manual setup, configure these settings:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://tooensure-cursor.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4.1",
    "EmbeddingDeploymentName": "text-embedding-ada-002",
    "MaxTokens": 4000,
    "Temperature": 0.7,
    "EnableStreaming": true,
    "EnableFunctionCalling": true
  }
}
```

## 🧠 AI Capabilities

### **1. GPT-4.1 Integration**
The system uses GPT-4.1 for advanced reasoning and decision-making:

```csharp
// Azure AI Agent Service Configuration
public class AzureAIAgentConfig
{
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ModelDeploymentName { get; set; } = "gpt-4.1";
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    public int MaxTokens { get; set; } = 4000;
    public double Temperature { get; set; } = 0.7;
    public bool EnableStreaming { get; set; } = true;
    public bool EnableFunctionCalling { get; set; } = true;
}
```

### **2. Semantic Kernel Integration**
Azure AI services are integrated through Semantic Kernel:

```csharp
// Kernel configuration with Azure OpenAI
builder.Services.AddSingleton<Kernel>(provider =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4.1",
        endpoint: "https://tooensure-cursor.openai.azure.com/",
        apiKey: "your-api-key"
    );
    
    return kernelBuilder.Build();
});
```

### **3. Function Calling**
The system uses Azure AI's function calling capabilities for dynamic capability invocation:

```csharp
// Function calling for dynamic operations
public async Task<object> InvokeFunctionAsync(string functionName, object parameters)
{
    var kernel = _serviceProvider.GetRequiredService<Kernel>();
    
    var arguments = new KernelArguments
    {
        ["functionName"] = functionName,
        ["parameters"] = JsonSerializer.Serialize(parameters)
    };
    
    var result = await kernel.InvokePromptAsync(
        "Invoke the function {{$functionName}} with parameters {{$parameters}}",
        arguments);
    
    return result.GetValue<string>();
}
```

## 🔄 AI-Powered Evolution

### **1. Self-Analysis with AI**
The system uses Azure AI to analyze its own capabilities:

```csharp
public async Task<SelfAnalysisResult> AnalyzeSelfAsync(string agentId)
{
    var prompt = @"
    Analyze the current system capabilities and identify areas for improvement.
    Consider:
    - Performance metrics
    - Current limitations
    - Bottlenecks
    - Improvement opportunities
    
    Provide a detailed analysis with specific recommendations.
    ";
    
    var result = await _azureAIService.GetResponseAsync(prompt);
    return ParseAnalysisResult(result);
}
```

### **2. Improvement Identification**
AI helps identify the most impactful improvements:

```csharp
public async Task<List<ImprovementOpportunity>> IdentifyImprovementsAsync(
    SelfAnalysisResult analysis)
{
    var prompt = $@"
    Based on this system analysis:
    {JsonSerializer.Serialize(analysis)}
    
    Identify specific improvement opportunities with:
    - Priority levels
    - Estimated impact
    - Implementation approach
    - Confidence scores
    ";
    
    var result = await _azureAIService.GetResponseAsync(prompt);
    return ParseImprovementOpportunities(result);
}
```

### **3. Code Generation**
Azure AI generates code for new capabilities:

```csharp
public async Task<string> GenerateCodeAsync(string capabilityDescription)
{
    var prompt = $@"
    Generate C# code for the following capability:
    {capabilityDescription}
    
    Requirements:
    - Follow C# best practices
    - Include proper error handling
    - Add XML documentation
    - Ensure thread safety
    - Use dependency injection
    ";
    
    return await _azureAIService.GetResponseAsync(prompt);
}
```

## 🧪 Testing Azure AI Integration

### **1. Connectivity Test**
Test basic Azure AI connectivity:

```powershell
# Test Azure AI connectivity
.\TestAzureAI.ps1

# This will test:
# - Azure CLI access
# - OpenAI resource access
# - Direct API calls
# - Configuration validation
```

### **2. System Integration Test**
Test the full system with Azure AI:

```powershell
# Test the complete strange loop system
.\TestStrangeLoop.ps1

# This will test:
# - Health endpoints
# - Azure AI agent responses
# - Strange loop statistics
# - Code generation
# - Evolution process
```

### **3. Manual API Testing**
Test Azure AI APIs directly:

```bash
# Test chat completion
curl -X POST "https://tooensure-cursor.openai.azure.com/openai/deployments/gpt-4.1/chat/completions?api-version=2024-02-15-preview" \
  -H "Content-Type: application/json" \
  -H "api-key: your-api-key" \
  -d '{
    "messages": [
      {
        "role": "user",
        "content": "Hello! Can you help me understand how self-evolution works in AI systems?"
      }
    ],
    "max_tokens": 100,
    "temperature": 0.7
  }'
```

## 🔧 Troubleshooting

### **Common Issues**

#### **1. 401 Permission Denied**
**Symptoms**: API calls return 401 errors
**Solutions**:
- Verify API key permissions in Azure portal
- Check network connectivity and firewall settings
- Validate endpoint URL format
- Ensure subscription has access to OpenAI services

#### **2. Connection Timeouts**
**Symptoms**: Requests timeout or fail to connect
**Solutions**:
- Check network connectivity
- Verify endpoint is accessible
- Check firewall and proxy settings
- Validate DNS resolution

#### **3. Model Deployment Issues**
**Symptoms**: Model not found or deployment failed
**Solutions**:
- Verify model deployment status in Azure portal
- Check deployment name matches configuration
- Ensure model is available in your region
- Validate deployment permissions

### **Debug Commands**

```powershell
# Check Azure CLI status
az account show

# List OpenAI resources
az cognitiveservices account list --subscription "AutonomousAgentFramwork"

# Check model deployments
az cognitiveservices account deployment list --name "tooensure-cursor" --resource-group "rg-autonomousa"

# Test API key
az cognitiveservices account keys list --name "tooensure-cursor" --resource-group "rg-autonomousa"
```

## 🎯 Best Practices

### **1. Security**
- Store API keys securely (use Azure Key Vault in production)
- Use least privilege access
- Monitor API usage and costs
- Implement rate limiting

### **2. Performance**
- Use streaming for long responses
- Implement caching for repeated requests
- Monitor token usage and costs
- Optimize prompt engineering

### **3. Reliability**
- Implement retry logic with exponential backoff
- Use circuit breakers for fault tolerance
- Monitor API health and availability
- Implement fallback mechanisms

## 🔮 Future Enhancements

### **Planned Features**
- **Multi-Model Support**: Integration with additional Azure AI models
- **Advanced Prompting**: Dynamic prompt generation and optimization
- **Cost Optimization**: Intelligent token usage and caching
- **Real-time Learning**: Continuous model fine-tuning and adaptation

### **Integration Roadmap**
- **Azure Cognitive Services**: Vision, speech, and language services
- **Azure Machine Learning**: Custom model training and deployment
- **Azure Bot Service**: Conversational AI capabilities
- **Azure Search**: Advanced search and retrieval capabilities

---

> **The Azure AI integration provides the intelligence foundation for autonomous evolution, enabling the system to think, learn, and improve itself using state-of-the-art AI capabilities.**

The foundation is solid and ready for the next phase of testing and evolution! 🎯 