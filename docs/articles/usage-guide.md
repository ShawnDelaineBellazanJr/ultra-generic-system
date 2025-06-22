# Ultra-Generic AI System Usage Guide

## Overview

The Ultra-Generic, Context-Driven, Self-Evolving AI System is a production-ready enterprise AI platform that integrates Microsoft Semantic Kernel (SK) Agent Framework v1.57+, SK Process Framework, Prompty declarative templates, Microsoft.Extensions.AI, and Ollama local LLM hosting. The system operates as a continuous "strange loop" that plans, executes, checks, and reflects to self-optimize.

## System Architecture

### Core Components

- **SK Agent Framework v1.57+**: Multi-agent orchestration with function calling
- **SK Process Framework**: Long-running workflows and stateful processes
- **Prompty**: Declarative prompt templates for dynamic evolution
- **Microsoft.Extensions.AI**: Unified AI service abstractions
- **Ollama**: Local LLM hosting for privacy and cost control
- **Strange Loop Meta-Cognition**: 4-level recursive learning system

### Meta-Cognitive Hierarchy

```
Level 4: Meta-Meta-Reflection (System Evolution Strategy)
    ↓ observes ↑ influences
Level 3: Meta-Reflection (Learning Pattern Analysis)  
    ↓ observes ↑ influences
Level 2: Reflection (Performance & Outcome Analysis)
    ↓ observes ↑ influences  
Level 1: Execution (Task Performance & Skill Application)
    ↓ feeds back into ↑ higher levels
```

## API Endpoints

### 1. Self-Evolution & Code Generation

#### Generate Code
```http
POST /api/SelfEvolution/generate
Content-Type: application/json

{
  "specification": "Create a C# service that processes customer orders with validation, error handling, and logging",
  "targetType": "service",
  "framework": "aspnetcore"
}
```

#### Generate CRUD Stack
```http
POST /api/SelfEvolution/generate/crud-stack
Content-Type: application/json

{
  "entityName": "Customer",
  "properties": ["Id", "Name", "Email", "Phone"]
}
```

#### Generate Controller
```http
POST /api/SelfEvolution/generate/controller
Content-Type: application/json

{
  "controllerName": "CustomerController",
  "entityType": "Customer"
}
```

#### Compile Generated Code
```http
POST /api/SelfEvolution/compile
Content-Type: application/json

{
  "code": "Generated C# code here",
  "references": ["System", "Microsoft.AspNetCore.Mvc"]
}
```

### 2. Memory & RAG (Retrieval-Augmented Generation)

#### Store Knowledge
```http
POST /api/Memory/store
Content-Type: application/json

{
  "key": "ai_architecture",
  "value": "Ultra-generic context-driven self-evolving AI with strange loop meta-cognition",
  "metadata": {
    "category": "architecture",
    "tags": ["ai", "self-evolving", "strange-loop"]
  }
}
```

#### Query Knowledge
```http
POST /api/Memory/rag/query
Content-Type: application/json

{
  "query": "What is the strange loop architecture?",
  "maxResults": 5
}
```

#### Process Documents
```http
POST /api/Memory/rag/process-document
Content-Type: application/json

{
  "document": "Your document content here",
  "chunkSize": 1000
}
```

#### Get Memory Entry
```http
GET /api/Memory/get/{key}
```

### 3. Ollama Integration (Local LLM)

#### Chat with Local Models
```http
POST /api/Ollama/chat
Content-Type: application/json

{
  "model": "llama3:latest",
  "message": "Explain quantum computing",
  "stream": false
}
```

#### Get Embeddings
```http
POST /api/Ollama/embeddings
Content-Type: application/json

{
  "model": "llama3:latest",
  "text": "Your text to embed"
}
```

#### List Available Models
```http
GET /api/Ollama/models
```

#### Get Model Info
```http
GET /api/Ollama/models/{modelName}/info
```

### 4. Azure AI Integration

#### Generate Content
```http
POST /api/AzureAIAgent/generate
Content-Type: application/json

{
  "prompt": "Create a business plan for a tech startup",
  "maxTokens": 1000
}
```

#### Analyze Text
```http
POST /api/AzureAIAgent/analyze
Content-Type: application/json

{
  "text": "Your text to analyze",
  "analysisType": "sentiment"
}
```

#### Get Embeddings
```http
POST /api/AzureAIAgent/embeddings
Content-Type: application/json

{
  "text": "Your text to embed"
}
```

### 5. Advanced Orchestration

#### Multi-Step Workflows
```http
POST /api/AdvancedOrchestration/workflow
Content-Type: application/json

{
  "input": {
    "task": "Analyze and optimize code",
    "code": "public class DataProcessor { ... }"
  },
  "steps": ["analyze", "optimize", "test", "deploy"]
}
```

#### Chain-of-Thought Reasoning
```http
POST /api/AdvancedOrchestration/chain-of-thought
Content-Type: application/json

{
  "problem": "Design a scalable microservice architecture",
  "maxSteps": 5
}
```

#### Multi-Agent Conversation
```http
POST /api/AdvancedOrchestration/conversation-demo
Content-Type: application/json

{
  "task": "Design a machine learning pipeline for customer sentiment analysis",
  "maxIterations": 5,
  "convergenceThreshold": 0.85
}
```

### 6. Strange Loop Self-Evolution

#### Trigger Evolution
```http
POST /api/SelfEvolution/strange-loop
Content-Type: application/json

{
  "agentId": "planner",
  "evolutionType": "meta_cognitive",
  "context": "Current system performance analysis"
}
```

#### Get Evolution Statistics
```http
GET /api/SelfEvolution/statistics
```

#### Get Strange Loop History
```http
GET /api/SelfEvolution/strange-loop/history/{agentId}
```

### 7. Plugin Management

#### Load Dynamic Plugins
```http
POST /api/SelfEvolution/plugins/load
Content-Type: application/json

{
  "pluginName": "CustomDataProcessor",
  "pluginCode": "Your C# plugin code here"
}
```

#### Execute Plugins
```http
POST /api/SelfEvolution/plugins/{pluginName}/execute
Content-Type: application/json

{
  "input": "Your input data",
  "parameters": {"param1": "value1"}
}
```

#### Validate Plugins
```http
POST /api/SelfEvolution/plugins/validate
Content-Type: application/json

{
  "pluginCode": "Your C# plugin code here"
}
```

### 8. Swagger-to-SK Integration

#### Generate SK Plugins from Swagger
```http
POST /api/SwaggerToSK/generate-plugin
Content-Type: application/json

{
  "swaggerUrl": "https://api.example.com/swagger.json",
  "pluginName": "ExternalAPI"
}
```

## Real-World Usage Scenarios

### Scenario 1: Building a New Microservice

1. **Generate the Service**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/generate/service" \
  -H "Content-Type: application/json" \
  -d '{
    "specification": "User authentication service with JWT tokens"
  }'
```

2. **Generate the Controller**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/generate/controller" \
  -H "Content-Type: application/json" \
  -d '{
    "controllerName": "AuthController"
  }'
```

3. **Generate the Repository**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/generate/repository" \
  -H "Content-Type: application/json" \
  -d '{
    "entityName": "User"
  }'
```

4. **Compile and Load**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/compile" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "Generated code here"
  }'
```

### Scenario 2: Knowledge Management

1. **Store Domain Knowledge**
```bash
curl -X POST "http://localhost:5000/api/Memory/store" \
  -H "Content-Type: application/json" \
  -d '{
    "key": "business_rules",
    "value": "Customer orders must be validated before processing"
  }'
```

2. **Query for Relevant Information**
```bash
curl -X POST "http://localhost:5000/api/Memory/rag/query" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "What are the business rules for order processing?"
  }'
```

3. **Use in AI Generation**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "specification": "Order validation service",
    "context": "Use stored business rules"
  }'
```

### Scenario 3: Continuous Learning

1. **Analyze Current Performance**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/analyze" \
  -H "Content-Type: application/json" \
  -d '{
    "target": "system_performance",
    "metrics": ["response_time", "accuracy", "user_satisfaction"]
  }'
```

2. **Trigger Strange Loop Evolution**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/strange-loop" \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "optimizer",
    "evolutionType": "performance_optimization"
  }'
```

3. **Generate Improvements**
```bash
curl -X POST "http://localhost:5000/api/SelfEvolution/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "specification": "Performance optimization based on analysis"
  }'
```

## System Capabilities

### ✅ Production-Ready Features

- **Code Generation & Self-Evolution**: Dynamic C# code generation with Roslyn compilation
- **Knowledge Management & RAG**: Vector-based memory with semantic search
- **Multi-Agent Orchestration**: SK Agent Framework with function calling
- **Local & Cloud AI Integration**: Ollama + Azure OpenAI support
- **Dynamic Plugin System**: Runtime plugin loading and execution
- **Strange Loop Meta-Cognition**: 4-level recursive learning system
- **Enterprise-Grade APIs**: RESTful endpoints with proper error handling
- **Real-Time Workflows**: Multi-step orchestration with state management

### 🔄 Strange Loop Learning Process

1. **Level 1 - Execution**: Perform tasks and generate experience data
2. **Level 2 - Reflection**: Analyze performance patterns and outcomes
3. **Level 3 - Meta-Reflection**: Learn about learning patterns and strategies
4. **Level 4 - Meta-Meta-Reflection**: Evolve the evolution strategy itself

### 🧠 Agent Ecosystem

- **PlannerAgent**: Decomposes goals into actionable steps
- **OrchestratorAgent**: Executes planned steps using available skills
- **CheckerAgent**: Validates results and identifies issues
- **ReflectorAgent**: Generates new skills when capabilities are missing
- **PatternMinerAgent**: Extracts reusable patterns from execution history
- **KnowledgeGraphAgent**: Builds and maintains technical knowledge graphs

## Configuration

### Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection=Data Source=ultra_generic.db

# Azure OpenAI
AzureOpenAI__Endpoint=https://your-resource.openai.azure.com/
AzureOpenAI__ApiKey=your-api-key
AzureOpenAI__DeploymentName=gpt-4-turbo

# Ollama
Ollama__BaseUrl=http://localhost:11434
Ollama__DefaultModel=llama3:latest

# Self Evolution
SelfEvolution__EnableCodeGeneration=true
SelfEvolution__EnableDynamicCompilation=true
SelfEvolution__EnableSelfModification=false
SelfEvolution__MaxGeneratedFiles=1000
```

### Kill-Switch Configuration

The system includes safety mechanisms to prevent runaway self-modification:

```json
{
  "SelfEvolution": {
    "EnableSelfModification": false,
    "MaxGeneratedFiles": 1000,
    "AllowedNamespaces": ["System", "Microsoft.AspNetCore"],
    "ForbiddenNamespaces": ["System.IO", "System.Net"]
  }
}
```

## Monitoring & Observability

### Execution Telemetry

```json
{
  "execution_telemetry": {
    "trace_id": "uuid",
    "span_hierarchy": "agent_call_tree",
    "performance_metrics": "latency_p95, memory_usage, cpu_utilization",
    "business_metrics": "value_delivered, developer_productivity_gain"
  },
  "learning_telemetry": {
    "pattern_extraction_rate": "patterns_per_hour",
    "knowledge_graph_growth": "nodes_edges_per_day",
    "skill_evolution_frequency": "new_skills_per_week",
    "strange_loop_depth": "cognitive_recursion_levels"
  }
}
```

## Best Practices

### 1. Code Generation
- Always validate generated code before compilation
- Use the kill-switch configuration to prevent unsafe modifications
- Implement proper error handling in generated code

### 2. Memory Management
- Use meaningful keys for memory storage
- Include relevant metadata for better retrieval
- Regularly clean up old or unused memory entries

### 3. Agent Orchestration
- Design workflows with clear success criteria
- Implement proper error handling and retry logic
- Monitor agent performance and adjust strategies

### 4. Security
- Validate all inputs before processing
- Use the kill-switch for production environments
- Implement proper authentication and authorization

## Troubleshooting

### Common Issues

1. **Memory Store Returns 400**: Check that the request body includes required fields (key, value)
2. **Code Generation Fails**: Ensure the specification is clear and the targetType is supported
3. **Orchestration Timeout**: Increase timeout values or break complex workflows into smaller steps
4. **Plugin Loading Errors**: Validate plugin code and check namespace restrictions

### Debug Mode

Enable debug logging by setting the log level to Debug in appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "UltraGenericSystem": "Debug"
    }
  }
}
```

## Next Steps

1. **Explore the API**: Use the Swagger UI at `/swagger` to explore all available endpoints
2. **Start with Simple Workflows**: Begin with basic memory operations and code generation
3. **Build Complex Systems**: Gradually build more sophisticated multi-agent workflows
4. **Monitor and Evolve**: Use the strange loop capabilities to continuously improve the system

## Support

For issues and questions:
- Check the system logs for detailed error information
- Use the `/api/SelfEvolution/statistics` endpoint to monitor system health
- Review the kill-switch configuration if experiencing unexpected behavior 