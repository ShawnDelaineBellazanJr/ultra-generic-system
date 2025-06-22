# Testing Guide

## 🧪 Comprehensive Testing Strategy

The Strange Loop Self-Evolution System requires thorough testing to ensure reliable autonomous evolution. This guide covers all testing aspects from basic connectivity to advanced evolution scenarios.

> **A next-generation autonomous architecture that can evolve itself - a true "strange loop" where the system becomes both the observer and the observed, the designer and the designed.**

## 🎯 Testing Categories

### **1. Infrastructure Testing**
- Azure AI connectivity and configuration
- Database connectivity and Entity Framework
- Service registration and dependency injection
- API endpoints and routing

### **2. Agent Testing**
- Semantic Kernel agent functionality
- Azure AI agent responses
- Agent orchestration patterns
- Streaming and function calling

### **3. Evolution Testing**
- Strange loop process validation
- Code generation and compilation
- Dynamic plugin loading
- Self-modification capabilities

### **4. Integration Testing**
- End-to-end evolution cycles
- Memory and RAG integration
- Multi-agent collaboration
- Performance and scalability

## 🚀 Automated Testing Scripts

### **1. Azure AI Connectivity Test**
```powershell
# Test Azure AI connectivity
.\TestAzureAI.ps1

# This comprehensive test covers:
# - Azure CLI authentication
# - OpenAI resource access
# - API key validation
# - Model deployment status
# - Direct API calls
# - Configuration validation
```

**Expected Output:**
```
=== Azure AI Connectivity Test ===
✓ Azure CLI authenticated
✓ OpenAI resource accessible: tooensure-cursor
✓ API key valid
✓ Model deployment ready: gpt-4.1
✓ Direct API call successful
✓ Configuration valid
=== All tests passed ===
```

### **2. Strange Loop System Test**
```powershell
# Test the complete strange loop system
.\TestStrangeLoop.ps1

# This comprehensive test covers:
# - Application startup and health
# - Azure AI agent responses
# - Strange loop statistics
# - Code generation capabilities
# - Evolution process validation
# - Memory integration
```

**Expected Output:**
```
=== Strange Loop System Test ===
✓ Application health check passed
✓ Azure AI agent responding
✓ Strange loop statistics accessible
✓ Code generation working
✓ Evolution process ready
✓ Memory integration functional
=== All tests passed ===
```

## 🔧 Manual Testing Procedures

### **1. Health Check Testing**

#### **Application Health**
```bash
curl -X GET "https://localhost:7001/api/azureaiagent/health"
```

**Expected Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-06-21T22:00:00Z",
  "version": "1.0.0",
  "azureAIStatus": "connected",
  "databaseStatus": "connected",
  "evolutionStatus": "ready"
}
```

#### **Database Health**
```bash
curl -X GET "https://localhost:7001/api/generic/health"
```

**Expected Response:**
```json
{
  "status": "healthy",
  "databaseConnection": "connected",
  "entityCount": 0,
  "lastMigration": "2025-06-21T22:00:00Z"
}
```

### **2. Azure AI Agent Testing**

#### **Basic Message Test**
```bash
curl -X POST "https://localhost:7001/api/azureaiagent/message" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hello! Can you help me understand how self-evolution works?",
    "conversationId": "test-conversation-001"
  }'
```

**Expected Response:**
```json
{
  "response": "Hello! I'd be happy to explain how self-evolution works in AI systems...",
  "conversationId": "test-conversation-001",
  "timestamp": "2025-06-21T22:00:00Z",
  "tokensUsed": 150,
  "modelUsed": "gpt-4.1"
}
```

#### **Streaming Response Test**
```bash
curl -X POST "https://localhost:7001/api/azureaiagent/message/stream" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Explain the strange loop concept in detail",
    "conversationId": "test-conversation-002"
  }'
```

**Expected Response:** (Streaming chunks)
```
data: {"chunk": "The", "conversationId": "test-conversation-002"}
data: {"chunk": " strange", "conversationId": "test-conversation-002"}
data: {"chunk": " loop", "conversationId": "test-conversation-002"}
...
```

### **3. Strange Loop Evolution Testing**

#### **Evolution Statistics**
```bash
curl -X GET "https://localhost:7001/api/selfevolution/strange-loop/statistics"
```

**Expected Response:**
```json
{
  "totalEvolutions": 0,
  "successfulEvolutions": 0,
  "failedEvolutions": 0,
  "averageEvolutionTime": "00:00:00",
  "lastEvolutionDate": null,
  "evolutionTypes": {
    "SelfModification": 0,
    "PerformanceOptimization": 0,
    "CapabilityEnhancement": 0
  },
  "systemCapabilities": [
    "BasicAgentOrchestration",
    "AzureAIIntegration",
    "CodeGeneration",
    "DynamicPluginLoading"
  ]
}
```

#### **Start Evolution Process**
```bash
curl -X POST "https://localhost:7001/api/selfevolution/strange-loop" \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "test-evolution-agent",
    "evolutionType": "SelfModification",
    "parameters": {
      "targetImprovement": "Performance",
      "confidenceThreshold": 0.8,
      "maxCodeGenerationAttempts": 3
    },
    "enableHumanApproval": false,
    "timeout": "00:05:00",
    "maxEvolutionSteps": 2
  }'
```

**Expected Response:**
```json
{
  "evolutionId": "evol-001",
  "status": "started",
  "agentId": "test-evolution-agent",
  "evolutionType": "SelfModification",
  "startTime": "2025-06-21T22:00:00Z",
  "estimatedCompletion": "2025-06-21T22:05:00Z",
  "currentStep": "SelfAnalysis"
}
```

#### **Monitor Evolution Progress**
```bash
curl -X GET "https://localhost:7001/api/selfevolution/strange-loop/evol-001/status"
```

**Expected Response:**
```json
{
  "evolutionId": "evol-001",
  "status": "in_progress",
  "currentStep": "CodeGeneration",
  "completedSteps": [
    "SelfAnalysis",
    "ImprovementIdentification",
    "CapabilityDesign"
  ],
  "remainingSteps": [
    "CapabilityLoading",
    "Testing",
    "Reflection"
  ],
  "progress": 0.57,
  "startTime": "2025-06-21T22:00:00Z",
  "estimatedCompletion": "2025-06-21T22:05:00Z"
}
```

### **4. Code Generation Testing**

#### **Basic Code Generation**
```bash
curl -X POST "https://localhost:7001/api/selfevolution/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "templateName": "Entity",
    "parameters": {
      "EntityName": "TestEntity",
      "Properties": {
        "Name": "string",
        "Value": "int",
        "CreatedDate": "DateTime"
      }
    },
    "outputPath": "Models/Generated/TestEntity.cs",
    "compileImmediately": true
  }'
```

**Expected Response:**
```json
{
  "generationId": "gen-001",
  "status": "completed",
  "templateName": "Entity",
  "outputPath": "Models/Generated/TestEntity.cs",
  "compilationSuccess": true,
  "generatedCode": "using System;\n\nnamespace UltraGenericSystem.Models.Generated\n{\n    public class TestEntity : BaseEntity\n    {\n        public string Name { get; set; } = \"\";\n        public int Value { get; set; }\n        public DateTime CreatedDate { get; set; }\n    }\n}",
  "compilationResult": {
    "success": true,
    "errors": [],
    "warnings": []
  }
}
```

#### **Template Listing**
```bash
curl -X GET "https://localhost:7001/api/selfevolution/templates"
```

**Expected Response:**
```json
{
  "templates": [
    {
      "name": "Entity",
      "description": "Generate a new entity class",
      "parameters": {
        "EntityName": "string",
        "Properties": "object"
      }
    },
    {
      "name": "Controller",
      "description": "Generate a new controller",
      "parameters": {
        "ControllerName": "string",
        "EntityName": "string"
      }
    },
    {
      "name": "Service",
      "description": "Generate a new service",
      "parameters": {
        "ServiceName": "string",
        "InterfaceName": "string"
      }
    }
  ]
}
```

### **5. Memory and RAG Testing**

#### **Store Memory Entry**
```bash
curl -X POST "https://localhost:7001/api/memory/store" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "The strange loop system successfully evolved its performance capabilities",
    "metadata": {
      "type": "evolution_result",
      "agentId": "test-agent",
      "evolutionId": "evol-001"
    },
    "tags": ["evolution", "performance", "success"]
  }'
```

#### **Search Memory**
```bash
curl -X POST "https://localhost:7001/api/memory/search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "performance evolution",
    "maxResults": 5,
    "similarityThreshold": 0.7
  }'
```

## 📊 Performance Testing

### **1. Load Testing**
```bash
# Test concurrent evolution requests
for i in {1..10}; do
  curl -X POST "https://localhost:7001/api/selfevolution/strange-loop" \
    -H "Content-Type: application/json" \
    -d "{\"agentId\": \"load-test-agent-$i\", \"evolutionType\": \"SelfModification\"}" &
done
wait
```

### **2. Response Time Testing**
```bash
# Test Azure AI response times
time curl -X POST "https://localhost:7001/api/azureaiagent/message" \
  -H "Content-Type: application/json" \
  -d '{"message": "Test message", "conversationId": "perf-test"}'
```

### **3. Memory Usage Testing**
```bash
# Monitor memory usage during evolution
while true; do
  echo "$(date): $(ps aux | grep UltraGenericSystem | grep -v grep | awk '{print $6}') KB"
  sleep 5
done
```

## 🐛 Debugging and Troubleshooting

### **1. Application Logs**
```bash
# View application logs
tail -f logs/ultra-generic-system-*.log

# Search for errors
grep -i error logs/ultra-generic-system-*.log

# Search for evolution events
grep -i evolution logs/ultra-generic-system-*.log
```

### **2. Database Debugging**
```bash
# Check database connection
sqlite3 UltraGenericSystem.db ".tables"

# Check entity data
sqlite3 UltraGenericSystem.db "SELECT * FROM BaseEntities LIMIT 10;"

# Check memory entries
sqlite3 UltraGenericSystem.db "SELECT * FROM MemoryEntries LIMIT 10;"
```

### **3. Network Debugging**
```bash
# Test Azure AI endpoint connectivity
curl -I "https://tooensure-cursor.openai.azure.com/"

# Test local API connectivity
curl -I "https://localhost:7001/api/azureaiagent/health"

# Check port usage
netstat -ano | findstr :7001
```

## 🎯 Test Scenarios

### **Scenario 1: Basic Evolution Cycle**
1. Start a simple evolution process
2. Monitor progress through all 7 steps
3. Verify final reflection and statistics
4. Check that new capabilities are loaded

### **Scenario 2: Failed Evolution Recovery**
1. Start evolution with invalid parameters
2. Verify graceful error handling
3. Check that system remains stable
4. Verify error logging and reporting

### **Scenario 3: Multi-Agent Collaboration**
1. Start multiple evolution processes
2. Verify agent orchestration works
3. Check for resource conflicts
4. Verify final results are consistent

### **Scenario 4: Memory Integration**
1. Store evolution results in memory
2. Search for related information
3. Verify RAG integration works
4. Check memory persistence

## 📈 Test Metrics

### **Success Criteria**
- **Evolution Success Rate**: > 90%
- **Average Evolution Time**: < 5 minutes
- **API Response Time**: < 2 seconds
- **Memory Search Accuracy**: > 85%
- **Code Generation Success**: > 95%

### **Performance Benchmarks**
- **Concurrent Evolutions**: Support 10+ simultaneous processes
- **Memory Storage**: Handle 10,000+ entries
- **Code Generation**: Generate 100+ lines per minute
- **Agent Response**: < 1 second for simple queries

## 🔮 Advanced Testing

### **1. Chaos Testing**
- Randomly kill evolution processes
- Simulate network failures
- Test memory corruption scenarios
- Verify system recovery

### **2. Security Testing**
- Test API authentication
- Verify input validation
- Check for injection attacks
- Test privilege escalation

### **3. Integration Testing**
- Test with external systems
- Verify Azure AI integration
- Check database migrations
- Test deployment scenarios

---

> **Comprehensive testing ensures the strange loop system evolves reliably and safely, maintaining the foundation for autonomous software development.**

The foundation is solid and ready for the next phase of testing and evolution! 🎯 