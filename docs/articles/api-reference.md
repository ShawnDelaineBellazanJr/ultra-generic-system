# API Reference

## 📚 Complete API Documentation

The Strange Loop Self-Evolution System exposes a comprehensive REST API that enables interaction with all aspects of the autonomous evolution system.

> **A next-generation autonomous architecture that can evolve itself - a true "strange loop" where the system becomes both the observer and the observed, the designer and the designed.**

## 🎯 API Overview

### **Base URL**
```
https://localhost:7001
```

### **Authentication**
Currently, the API uses no authentication. In production, implement proper authentication and authorization.

### **Response Format**
All API responses are in JSON format with the following structure:

```json
{
  "success": true,
  "data": { /* response data */ },
  "message": "Operation completed successfully",
  "timestamp": "2025-06-21T22:00:00Z"
}
```

### **Error Responses**
```json
{
  "success": false,
  "error": "Error description",
  "errorCode": "ERROR_CODE",
  "timestamp": "2025-06-21T22:00:00Z"
}
```

## 🤖 Azure AI Agent API

### **Health Check**
```http
GET /api/azureaiagent/health
```

**Response:**
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

### **Send Message**
```http
POST /api/azureaiagent/message
Content-Type: application/json

{
  "message": "Hello! Can you help me understand how self-evolution works?",
  "conversationId": "conversation-001",
  "parameters": {
    "maxTokens": 1000,
    "temperature": 0.7
  }
}
```

**Response:**
```json
{
  "response": "Hello! I'd be happy to explain how self-evolution works...",
  "conversationId": "conversation-001",
  "timestamp": "2025-06-21T22:00:00Z",
  "tokensUsed": 150,
  "modelUsed": "gpt-4.1",
  "processingTime": "00:00:01.234"
}
```

### **Streaming Message**
```http
POST /api/azureaiagent/message/stream
Content-Type: application/json

{
  "message": "Explain the strange loop concept in detail",
  "conversationId": "conversation-002"
}
```

**Response:** (Server-Sent Events)
```
data: {"chunk": "The", "conversationId": "conversation-002"}
data: {"chunk": " strange", "conversationId": "conversation-002"}
data: {"chunk": " loop", "conversationId": "conversation-002"}
data: {"chunk": " concept", "conversationId": "conversation-002"}
...
```

### **Function Calling**
```http
POST /api/azureaiagent/function
Content-Type: application/json

{
  "functionName": "analyzeSystem",
  "parameters": {
    "targetSystem": "performance",
    "analysisDepth": "detailed"
  },
  "conversationId": "conversation-003"
}
```

## 🔄 Strange Loop Evolution API

### **Get Evolution Statistics**
```http
GET /api/selfevolution/strange-loop/statistics
```

**Response:**
```json
{
  "totalEvolutions": 5,
  "successfulEvolutions": 4,
  "failedEvolutions": 1,
  "averageEvolutionTime": "00:03:45",
  "lastEvolutionDate": "2025-06-21T21:30:00Z",
  "evolutionTypes": {
    "SelfModification": 2,
    "PerformanceOptimization": 2,
    "CapabilityEnhancement": 1
  },
  "systemCapabilities": [
    "BasicAgentOrchestration",
    "AzureAIIntegration",
    "CodeGeneration",
    "DynamicPluginLoading",
    "AdvancedMemory",
    "PerformanceOptimization"
  ],
  "successRate": 0.8,
  "averageImprovement": 0.15
}
```

### **Start Evolution Process**
```http
POST /api/selfevolution/strange-loop
Content-Type: application/json

{
  "agentId": "evolution-agent-001",
  "evolutionType": "SelfModification",
  "parameters": {
    "targetImprovement": "Performance",
    "confidenceThreshold": 0.8,
    "maxCodeGenerationAttempts": 3,
    "enableAdvancedAnalysis": true
  },
  "enableHumanApproval": false,
  "timeout": "00:10:00",
  "maxEvolutionSteps": 7
}
```

**Response:**
```json
{
  "evolutionId": "evol-20250621-001",
  "status": "started",
  "agentId": "evolution-agent-001",
  "evolutionType": "SelfModification",
  "startTime": "2025-06-21T22:00:00Z",
  "estimatedCompletion": "2025-06-21T22:10:00Z",
  "currentStep": "SelfAnalysis",
  "progress": 0.0
}
```

### **Get Evolution Status**
```http
GET /api/selfevolution/strange-loop/{evolutionId}/status
```

**Response:**
```json
{
  "evolutionId": "evol-20250621-001",
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
  "estimatedCompletion": "2025-06-21T22:10:00Z",
  "currentStepDetails": {
    "stepName": "CodeGeneration",
    "stepDescription": "Generating code for new capabilities",
    "stepProgress": 0.75,
    "estimatedStepCompletion": "2025-06-21T22:05:00Z"
  }
}
```

### **Cancel Evolution**
```http
DELETE /api/selfevolution/strange-loop/{evolutionId}
```

**Response:**
```json
{
  "evolutionId": "evol-20250621-001",
  "status": "cancelled",
  "cancellationTime": "2025-06-21T22:03:00Z",
  "completedSteps": [
    "SelfAnalysis",
    "ImprovementIdentification"
  ],
  "reason": "User requested cancellation"
}
```

### **Get Evolution History**
```http
GET /api/selfevolution/strange-loop/history?limit=10&offset=0
```

**Response:**
```json
{
  "evolutions": [
    {
      "evolutionId": "evol-20250621-001",
      "agentId": "evolution-agent-001",
      "evolutionType": "SelfModification",
      "status": "completed",
      "startTime": "2025-06-21T22:00:00Z",
      "completionTime": "2025-06-21T22:08:30Z",
      "success": true,
      "improvementsGenerated": 2,
      "capabilitiesAdded": 1
    }
  ],
  "totalCount": 5,
  "hasMore": false
}
```

## 💻 Code Generation API

### **Generate Code**
```http
POST /api/selfevolution/generate
Content-Type: application/json

{
  "templateName": "Entity",
  "parameters": {
    "EntityName": "TestEntity",
    "Properties": {
      "Name": "string",
      "Value": "int",
      "CreatedDate": "DateTime",
      "IsActive": "bool"
    },
    "Namespace": "UltraGenericSystem.Models.Generated",
    "InheritFrom": "BaseEntity"
  },
  "outputPath": "Models/Generated/TestEntity.cs",
  "compileImmediately": true,
  "validateCode": true
}
```

**Response:**
```json
{
  "generationId": "gen-20250621-001",
  "status": "completed",
  "templateName": "Entity",
  "outputPath": "Models/Generated/TestEntity.cs",
  "compilationSuccess": true,
  "generatedCode": "using System;\n\nnamespace UltraGenericSystem.Models.Generated\n{\n    public class TestEntity : BaseEntity\n    {\n        public string Name { get; set; } = \"\";\n        public int Value { get; set; }\n        public DateTime CreatedDate { get; set; }\n        public bool IsActive { get; set; }\n    }\n}",
  "compilationResult": {
    "success": true,
    "errors": [],
    "warnings": [],
    "assemblyBytes": "base64-encoded-assembly"
  },
  "generationTime": "00:00:02.345",
  "codeMetrics": {
    "linesOfCode": 12,
    "complexity": 1,
    "maintainabilityIndex": 100
  }
}
```

### **List Available Templates**
```http
GET /api/selfevolution/templates
```

**Response:**
```json
{
  "templates": [
    {
      "name": "Entity",
      "description": "Generate a new entity class",
      "category": "Models",
      "parameters": {
        "EntityName": {
          "type": "string",
          "description": "Name of the entity class",
          "required": true
        },
        "Properties": {
          "type": "object",
          "description": "Dictionary of property names and types",
          "required": true
        },
        "Namespace": {
          "type": "string",
          "description": "Namespace for the generated class",
          "required": false,
          "default": "UltraGenericSystem.Models.Generated"
        }
      }
    },
    {
      "name": "Controller",
      "description": "Generate a new API controller",
      "category": "Controllers",
      "parameters": {
        "ControllerName": {
          "type": "string",
          "description": "Name of the controller class",
          "required": true
        },
        "EntityName": {
          "type": "string",
          "description": "Name of the entity this controller manages",
          "required": true
        }
      }
    }
  ]
}
```

### **Get Generation History**
```http
GET /api/selfevolution/generate/history?limit=10&offset=0
```

**Response:**
```json
{
  "generations": [
    {
      "generationId": "gen-20250621-001",
      "templateName": "Entity",
      "status": "completed",
      "compilationSuccess": true,
      "generationTime": "00:00:02.345",
      "timestamp": "2025-06-21T22:00:00Z"
    }
  ],
  "totalCount": 15,
  "hasMore": true
}
```

## 🧠 Memory and RAG API

### **Store Memory Entry**
```http
POST /api/memory/store
Content-Type: application/json

{
  "content": "The strange loop system successfully evolved its performance capabilities by implementing advanced caching mechanisms.",
  "metadata": {
    "type": "evolution_result",
    "agentId": "evolution-agent-001",
    "evolutionId": "evol-20250621-001",
    "category": "performance",
    "confidence": 0.95
  },
  "tags": ["evolution", "performance", "caching", "success"],
  "priority": "high",
  "expirationDate": "2025-12-21T22:00:00Z"
}
```

**Response:**
```json
{
  "memoryId": "mem-20250621-001",
  "status": "stored",
  "embeddingGenerated": true,
  "vectorStored": true,
  "timestamp": "2025-06-21T22:00:00Z",
  "similarityScore": 0.0
}
```

### **Search Memory**
```http
POST /api/memory/search
Content-Type: application/json

{
  "query": "performance evolution caching",
  "maxResults": 10,
  "similarityThreshold": 0.7,
  "filters": {
    "tags": ["evolution", "performance"],
    "metadata": {
      "type": "evolution_result"
    }
  },
  "includeMetadata": true,
  "includeEmbeddings": false
}
```

**Response:**
```json
{
  "results": [
    {
      "memoryId": "mem-20250621-001",
      "content": "The strange loop system successfully evolved its performance capabilities by implementing advanced caching mechanisms.",
      "similarityScore": 0.92,
      "metadata": {
        "type": "evolution_result",
        "agentId": "evolution-agent-001",
        "evolutionId": "evol-20250621-001",
        "category": "performance",
        "confidence": 0.95
      },
      "tags": ["evolution", "performance", "caching", "success"],
      "timestamp": "2025-06-21T22:00:00Z"
    }
  ],
  "totalResults": 1,
  "searchTime": "00:00:00.123",
  "queryEmbedding": "base64-encoded-embedding"
}
```

### **Get Memory Statistics**
```http
GET /api/memory/statistics
```

**Response:**
```json
{
  "totalEntries": 150,
  "totalEmbeddings": 150,
  "averageSimilarityScore": 0.75,
  "memoryUsage": "2.5 MB",
  "mostCommonTags": [
    {"tag": "evolution", "count": 45},
    {"tag": "performance", "count": 32},
    {"tag": "success", "count": 28}
  ],
  "recentActivity": {
    "last24Hours": 12,
    "last7Days": 45,
    "last30Days": 150
  }
}
```

### **Delete Memory Entry**
```http
DELETE /api/memory/{memoryId}
```

**Response:**
```json
{
  "memoryId": "mem-20250621-001",
  "status": "deleted",
  "deletionTime": "2025-06-21T22:00:00Z"
}
```

## 🎛️ Agent Orchestration API

### **Get Orchestration Statistics**
```http
GET /api/orchestration/statistics
```

**Response:**
```json
{
  "totalOrchestrations": 25,
  "activeOrchestrations": 3,
  "completedOrchestrations": 22,
  "orchestrationPatterns": {
    "Sequential": 10,
    "Concurrent": 8,
    "GroupChat": 5,
    "Handoff": 2
  },
  "averageOrchestrationTime": "00:02:30",
  "successRate": 0.88,
  "agentUtilization": {
    "PlannerAgent": 0.75,
    "MakerAgent": 0.82,
    "CheckerAgent": 0.68,
    "ReflectorAgent": 0.71
  }
}
```

### **Start Orchestration**
```http
POST /api/orchestration/start
Content-Type: application/json

{
  "pattern": "GroupChat",
  "agents": ["PlannerAgent", "MakerAgent", "CheckerAgent"],
  "task": "Analyze system performance and suggest improvements",
  "parameters": {
    "maxIterations": 5,
    "timeout": "00:05:00",
    "enableStreaming": true
  },
  "callbackUrl": "https://example.com/callback"
}
```

**Response:**
```json
{
  "orchestrationId": "orch-20250621-001",
  "status": "started",
  "pattern": "GroupChat",
  "agents": ["PlannerAgent", "MakerAgent", "CheckerAgent"],
  "startTime": "2025-06-21T22:00:00Z",
  "estimatedCompletion": "2025-06-21T22:05:00Z"
}
```

### **Get Orchestration Status**
```http
GET /api/orchestration/{orchestrationId}/status
```

**Response:**
```json
{
  "orchestrationId": "orch-20250621-001",
  "status": "in_progress",
  "pattern": "GroupChat",
  "currentIteration": 2,
  "maxIterations": 5,
  "agentResponses": {
    "PlannerAgent": "Analyzing current performance metrics...",
    "MakerAgent": "Generating improvement suggestions...",
    "CheckerAgent": "Validating proposed changes..."
  },
  "progress": 0.4,
  "startTime": "2025-06-21T22:00:00Z",
  "estimatedCompletion": "2025-06-21T22:05:00Z"
}
```

## 🗄️ Generic Entity API

### **Health Check**
```http
GET /api/generic/health
```

**Response:**
```json
{
  "status": "healthy",
  "databaseConnection": "connected",
  "entityCount": 25,
  "lastMigration": "2025-06-21T22:00:00Z",
  "databaseSize": "1.2 MB"
}
```

### **Get All Entities**
```http
GET /api/generic/entities?page=1&pageSize=10&sortBy=createdDate&sortOrder=desc
```

**Response:**
```json
{
  "entities": [
    {
      "id": "entity-001",
      "type": "TestEntity",
      "data": {
        "name": "Test Entity 1",
        "value": 42,
        "createdDate": "2025-06-21T22:00:00Z"
      },
      "createdDate": "2025-06-21T22:00:00Z",
      "updatedDate": "2025-06-21T22:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### **Get Entity by ID**
```http
GET /api/generic/entities/{entityId}
```

**Response:**
```json
{
  "id": "entity-001",
  "type": "TestEntity",
  "data": {
    "name": "Test Entity 1",
    "value": 42,
    "createdDate": "2025-06-21T22:00:00Z"
  },
  "createdDate": "2025-06-21T22:00:00Z",
  "updatedDate": "2025-06-21T22:00:00Z"
}
```

### **Create Entity**
```http
POST /api/generic/entities
Content-Type: application/json

{
  "type": "TestEntity",
  "data": {
    "name": "New Test Entity",
    "value": 100,
    "createdDate": "2025-06-21T22:00:00Z"
  }
}
```

**Response:**
```json
{
  "id": "entity-002",
  "type": "TestEntity",
  "data": {
    "name": "New Test Entity",
    "value": 100,
    "createdDate": "2025-06-21T22:00:00Z"
  },
  "createdDate": "2025-06-21T22:00:00Z",
  "updatedDate": "2025-06-21T22:00:00Z"
}
```

### **Update Entity**
```http
PUT /api/generic/entities/{entityId}
Content-Type: application/json

{
  "data": {
    "name": "Updated Test Entity",
    "value": 200,
    "createdDate": "2025-06-21T22:00:00Z"
  }
}
```

### **Delete Entity**
```http
DELETE /api/generic/entities/{entityId}
```

## 🔧 Error Codes

### **Common Error Codes**
- `EVOLUTION_NOT_FOUND`: Evolution process not found
- `EVOLUTION_ALREADY_RUNNING`: Evolution already in progress
- `INVALID_EVOLUTION_PARAMETERS`: Invalid evolution parameters
- `CODE_GENERATION_FAILED`: Code generation failed
- `COMPILATION_FAILED`: Code compilation failed
- `MEMORY_STORAGE_FAILED`: Memory storage failed
- `AZURE_AI_ERROR`: Azure AI service error
- `DATABASE_ERROR`: Database operation failed
- `VALIDATION_ERROR`: Input validation failed
- `TIMEOUT_ERROR`: Operation timed out

### **HTTP Status Codes**
- `200 OK`: Operation successful
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict
- `422 Unprocessable Entity`: Validation error
- `500 Internal Server Error`: Server error
- `503 Service Unavailable`: Service temporarily unavailable

## 📊 Rate Limiting

### **Default Limits**
- **Azure AI API**: 100 requests per minute
- **Evolution API**: 10 requests per minute
- **Code Generation**: 20 requests per minute
- **Memory API**: 50 requests per minute
- **Generic API**: 200 requests per minute

### **Rate Limit Headers**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1642723200
```

---

> **This comprehensive API enables full interaction with the strange loop system, providing the foundation for autonomous software evolution.**

The foundation is solid and ready for the next phase of testing and evolution! 🎯 