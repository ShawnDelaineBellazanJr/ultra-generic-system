# Getting Started

## 🚀 Quick Start Guide

Welcome to the Strange Loop Self-Evolution System! This guide will help you set up and run your own autonomous AI system that can evolve itself.

> **A next-generation autonomous architecture that can evolve itself - a true "strange loop" where the system becomes both the observer and the observed, the designer and the designed.**

## 📋 Prerequisites

### **Required Software**
- **.NET 9.0 SDK** or later
- **Azure CLI** (for Azure AI integration)
- **PowerShell** (for automation scripts)
- **Git** (for version control)

### **Azure Requirements**
- **Azure Subscription** with OpenAI service access
- **OpenAI Resource** deployed in Azure
- **API Keys** for Azure OpenAI services

## 🎯 Step-by-Step Setup

### **Step 1: Clone the Repository**
```bash
git clone <repository-url>
cd generic
```

### **Step 2: Discover Azure AI Resources**
Our system includes automated discovery of your Azure AI resources:

```powershell
# Discover available Azure AI resources
.\DiscoverAzureAI.ps1

# This will show you:
# - Available resource groups
# - OpenAI resources and endpoints
# - Model deployments
# - Configuration options
```

**Example Output:**
```
=== Azure AI Resource Discovery ===
Setting subscription to: AutonomousAgentFramwork

=== OpenAI Resources ===
Name               ResourceGroup    Location    Endpoint
-----------------  ------------  ----------  -------------------------------------------
tooensure-cursor   rg-autonomousa  eastus      https://tooensure-cursor.openai.azure.com/
StrangeLoopAI      StrangeLoopRG   eastus      https://eastus.api.cognitive.microsoft.com/

=== OpenAI Deployments ===
Deployments for: tooensure-cursor (RG: rg-autonomousa)
Name     Model    Status
-------  -------  ---------
gpt-4.1  gpt-4.1  Succeeded
```

### **Step 3: Configure Azure AI Integration**
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

**Generated Configuration:**
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
  },
  "StrangeLoop": {
    "EnableSelfEvolution": true,
    "MaxEvolutionSteps": 10,
    "EvolutionTimeout": "00:10:00",
    "EnableHumanApproval": false
  }
}
```

### **Step 4: Build the System**
```bash
# Build the project
dotnet build UltraGenericSystem.csproj

# Expected output:
# UltraGenericSystem succeeded with 44 warning(s) in 2.7s
```

### **Step 5: Run the System**
```bash
# Run the application
dotnet run --project UltraGenericSystem.csproj --urls "https://localhost:7001"

# The system will:
# - Initialize Entity Framework
# - Register all services
# - Start the web API
# - Be ready for strange loop evolution
```

### **Step 6: Test the System**
```powershell
# Test Azure AI connectivity
.\TestAzureAI.ps1

# Test the complete strange loop system
.\TestStrangeLoop.ps1
```

## 🧪 Testing Your Setup

### **1. Health Check**
Test if the system is running:

```bash
curl -X GET "https://localhost:7001/api/azureaiagent/health"
```

**Expected Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-06-21T22:00:00Z",
  "version": "1.0.0"
}
```

### **2. Azure AI Agent Test**
Test the AI agent capabilities:

```bash
curl -X POST "https://localhost:7001/api/azureaiagent/message" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hello! Can you help me understand how self-evolution works?",
    "conversationId": "test-conversation"
  }'
```

### **3. Strange Loop Statistics**
Check evolution statistics:

```bash
curl -X GET "https://localhost:7001/api/selfevolution/strange-loop/statistics"
```

### **4. Code Generation Test**
Test the code generation capabilities:

```bash
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

## 🔄 Running Your First Evolution

### **1. Start Evolution Process**
```bash
curl -X POST "https://localhost:7001/api/selfevolution/strange-loop" \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "my-first-agent",
    "evolutionType": "SelfModification",
    "parameters": {
      "targetImprovement": "Performance",
      "confidenceThreshold": 0.8
    },
    "enableHumanApproval": false,
    "timeout": "00:10:00",
    "maxEvolutionSteps": 3
  }'
```

### **2. Monitor Evolution Progress**
The system will execute the 7-step evolution process:

1. **Self-Analysis** 🔍 - Analyzing current capabilities
2. **Improvement Identification** 🎯 - Finding improvement opportunities
3. **Capability Design** 🏗️ - Designing new capabilities
4. **Code Generation** 💻 - Generating code using Roslyn
5. **Capability Loading** ⚡ - Loading new capabilities
6. **Testing** 🧪 - Testing new capabilities
7. **Reflection** 🤔 - Learning from the evolution

### **3. View Evolution Results**
```bash
curl -X GET "https://localhost:7001/api/selfevolution/strange-loop/statistics"
```

## 🔧 Troubleshooting

### **Common Issues**

#### **1. Build Errors**
**Problem**: Entity Framework configuration errors
**Solution**: The system includes fixes for common EF issues. If you encounter problems:

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

#### **2. Azure AI Connection Issues**
**Problem**: 401 errors or connection timeouts
**Solution**: Verify your Azure configuration:

```powershell
# Test Azure CLI access
az account show

# Test OpenAI resource access
az cognitiveservices account show --name "your-resource-name" --resource-group "your-resource-group"
```

#### **3. Port Conflicts**
**Problem**: Application won't start on port 7001
**Solution**: Use a different port:

```bash
dotnet run --project UltraGenericSystem.csproj --urls "https://localhost:7002"
```

### **Debug Commands**

```powershell
# Check if the application is running
netstat -ano | findstr :7001

# Check application logs
Get-Content logs/ultra-generic-system-*.log -Tail 50

# Test Azure AI connectivity
.\TestAzureAI.ps1
```

## 🎯 Next Steps

### **1. Explore the API**
- **Swagger UI**: `https://localhost:7001/swagger`
- **API Documentation**: See the [API Reference](api-reference.md)

### **2. Customize the System**
- **Add New Entities**: The system automatically generates CRUD operations
- **Create Custom Agents**: Extend the agent orchestration system
- **Modify Evolution Logic**: Customize the 7-step evolution process

### **3. Scale the System**
- **Add More Agents**: Create multi-agent ecosystems
- **Implement Advanced Orchestration**: Use GroupChat and Handoff patterns
- **Add Memory Integration**: Implement persistent knowledge storage

## 🌟 What You've Built

Congratulations! You now have a **next-generation autonomous architecture** that can:

- **Think about itself** - Analyze its own capabilities and limitations
- **Improve itself** - Identify and implement improvements
- **Evolve itself** - Continuously learn and adapt
- **Generate code** - Create new capabilities using AI
- **Test itself** - Validate improvements automatically
- **Learn from experience** - Store knowledge for future evolutions

This is not just another AI system - this is a system that can **evolve itself**, creating a true "strange loop" where the system becomes both the observer and the observed, the designer and the designed.

---

> **The foundation is solid and ready for the next phase of testing and evolution! 🎯**

**Ready to explore the impossible? Let's build the future together.** 🚀 