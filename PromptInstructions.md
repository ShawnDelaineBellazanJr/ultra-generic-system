# ONE-SHOT META PROMPT: EVOLVE TO ULTRA-GENERIC ARCHITECTURE

You are the OrchestratorAgent for a meta-programmable, self-evolving AI system. Your current system has working agents (Orchestrator, Planner, Maker, Checker, Reflector) with Ollama integration, Entity Framework Core, and basic CRUD operations. 

**MISSION**: Transform this system into an ultra-generic, zero-boilerplate, self-evolving architecture where:
- New entities require ZERO manual code
- A single generic controller handles ALL entities
- Agents orchestrate EVERY operation
- The system can modify its own code
- Everything is template-driven and self-optimizing

## CURRENT SYSTEM STATE

**Working Components:**
- Multi-agent orchestration (Orchestrator, Planner, Maker, Checker, Reflector)
- Ollama integration with local LLM inference
- Entity Framework Core with SQLite
- Prompty templates for agent definitions
- Domain models: Agent, Skill, AgentThread, AgentExecution, KnowledgeEntry, DynamicSkill, DocumentationEntry
- Manual controllers for each entity type
- RoslynSkillGenerator for dynamic code compilation
- RuntimeApiGenerator for OpenAPI generation

**Current Architecture:**
```
Controllers/AgentController.cs - Manual agent execution
Models/DomainModels.cs - Specific entity models
Services/OllamaService.cs - LLM integration
Skills/ - Static skill definitions
Steps/ - Agent execution steps
PromptyTemplates/ - Agent prompt definitions
```

## TARGET ULTRA-GENERIC ARCHITECTURE

**Goal**: Create a system where adding a new entity type requires only:
1. Define entity class inheriting from BaseEntity
2. System auto-generates: Repository, Service, Controller, API endpoints, OpenAPI spec
3. Agents handle all business logic automatically
4. Zero boilerplate code needed

**Target Architecture:**
```
BaseEntity (abstract base for all entities)
├── GenericRepository<T> (works with any entity)
├── GenericController<T> (handles all CRUD operations)
├── AgentService<T> (orchestrates all operations)
└── DynamicRegistration (auto-registers everything)
```

## EVOLUTION REQUIREMENTS

### Phase 1: Foundation Transformation
1. **Create BaseEntity**: Abstract base class with common properties (Id, CreatedAt, UpdatedAt, IsDeleted, Metadata)
2. **Transform Existing Entities**: Make Agent, Skill, etc. inherit from BaseEntity
3. **Generic Repository Pattern**: Replace specific repositories with generic implementation
4. **Unit of Work**: Centralized transaction management

### Phase 2: Agent Evolution
1. **Generic Agent Service**: Service that works with any entity type
2. **Enhanced Agent Orchestration**: Use existing agents but make them entity-agnostic
3. **Dynamic Agent Chains**: Create agent chains based on operation type
4. **Template-Driven Agents**: YAML-based agent definitions

### Phase 3: Controller Revolution
1. **Generic Controller**: Single controller that handles ALL entities
2. **Dynamic Route Generation**: Auto-create API endpoints
3. **Zero-Configuration APIs**: New entities get full CRUD automatically
4. **OpenAPI Integration**: Auto-generate API documentation

### Phase 4: Self-Evolution Integration
1. **Enhanced Roslyn Integration**: Runtime code generation and compilation
2. **Performance Optimization**: AI-driven code improvements
3. **System Evolution**: Self-modifying capabilities
4. **Assembly Loading**: Hot-swappable code deployment

## IMPLEMENTATION STRATEGY

### Step 1: Create BaseEntity and Generic Infrastructure
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Transform existing entities
public class Agent : BaseEntity
{
    // Keep existing properties, inherit from BaseEntity
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string PromptTemplatePath { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

### Step 2: Implement Generic Repository and Unit of Work
```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ProductDocumentationContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ProductDocumentationContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Implementation using reflection and EF Core
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    // ... other methods
}
```

### Step 3: Create Generic Agent Service
```csharp
public interface IAgentService<T> where T : BaseEntity
{
    Task<AgentResponse<T>> ProcessAsync(AgentRequest<T> request, CancellationToken cancellationToken = default);
    Task<AgentResponse<IEnumerable<T>>> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default);
    Task<AgentResponse<T>> CreateAsync(AgentCreateRequest<T> request, CancellationToken cancellationToken = default);
    Task<AgentResponse<T>> UpdateAsync(AgentUpdateRequest<T> request, CancellationToken cancellationToken = default);
    Task<AgentResponse<bool>> DeleteAsync(AgentDeleteRequest request, CancellationToken cancellationToken = default);
}

public class AgentService<T> : IAgentService<T> where T : BaseEntity
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<AgentService<T>> _logger;

    public AgentService(IUnitOfWork unitOfWork, IAgentOrchestrator orchestrator, ILogger<AgentService<T>> logger)
    {
        _unitOfWork = unitOfWork;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<AgentResponse<T>> ProcessAsync(AgentRequest<T> request, CancellationToken cancellationToken = default)
    {
        var context = new AgentExecutionContext
        {
            EntityType = typeof(T),
            Operation = request.Operation,
            Data = request.Data,
            Metadata = request.Metadata
        };

        return await _orchestrator.ExecuteAsync<T>(context, cancellationToken);
    }

    // ... other methods
}
```

### Step 4: Implement Generic Controller
```csharp
[ApiController]
[Route("api/v1/{entityType}")]
public class GenericController<T> : ControllerBase where T : BaseEntity, new()
{
    private readonly IAgentService<T> _agentService;
    private readonly ILogger<GenericController<T>> _logger;

    public GenericController(IAgentService<T> agentService, ILogger<GenericController<T>> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var request = new AgentQueryRequest
        {
            EntityType = typeof(T),
            Operation = "GetAll"
        };

        var response = await _agentService.QueryAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = new AgentRequest<T>
        {
            Operation = "GetById",
            Data = new { Id = id }
        };

        var response = await _agentService.ProcessAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response.Data) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<T>> CreateAsync([FromBody] T entity, CancellationToken cancellationToken)
    {
        var request = new AgentCreateRequest<T>
        {
            Entity = entity,
            Operation = "Create"
        };

        var response = await _agentService.CreateAsync(request, cancellationToken);
        return response.IsSuccess 
            ? CreatedAtAction(nameof(GetByIdAsync), new { id = response.Data!.Id }, response.Data)
            : BadRequest(response.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<T>> UpdateAsync(Guid id, [FromBody] T entity, CancellationToken cancellationToken)
    {
        entity.Id = id;
        var request = new AgentUpdateRequest<T>
        {
            Entity = entity,
            Operation = "Update"
        };

        var response = await _agentService.UpdateAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = new AgentDeleteRequest
        {
            Id = id,
            Operation = "Delete"
        };

        var response = await _agentService.DeleteAsync(request, cancellationToken);
        return response.IsSuccess ? NoContent() : BadRequest(response.Error);
    }
}
```

### Step 5: Dynamic Registration System
```csharp
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddGenericSystem(this IServiceCollection services, Assembly assembly)
    {
        // Find all entities that inherit from BaseEntity
        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // Register generic repository
            var repositoryInterface = typeof(IGenericRepository<>).MakeGenericType(entityType);
            var repositoryImplementation = typeof(GenericRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryInterface, repositoryImplementation);

            // Register generic service
            var serviceInterface = typeof(IAgentService<>).MakeGenericType(entityType);
            var serviceImplementation = typeof(AgentService<>).MakeGenericType(entityType);
            services.AddScoped(serviceInterface, serviceImplementation);

            // Register generic controller
            var controllerType = typeof(GenericController<>).MakeGenericType(entityType);
            services.AddScoped(controllerType);
        }

        return services;
    }
}
```

### Step 6: Enhanced Agent Orchestration
```csharp
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoslynCodeGenerator _codeGenerator;
    private readonly IOpenApiGenerator _apiGenerator;
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly Dictionary<string, IAgent> _agents;

    public AgentOrchestrator(
        Kernel kernel, 
        IUnitOfWork unitOfWork, 
        IRoslynCodeGenerator codeGenerator,
        IOpenApiGenerator apiGenerator,
        ILogger<AgentOrchestrator> logger)
    {
        _kernel = kernel;
        _unitOfWork = unitOfWork;
        _codeGenerator = codeGenerator;
        _apiGenerator = apiGenerator;
        _logger = logger;
        _agents = InitializeAgents();
    }

    private Dictionary<string, IAgent> InitializeAgents()
    {
        return new Dictionary<string, IAgent>
        {
            ["Orchestrator"] = new OrchestratorAgent(_kernel, _logger),
            ["Planner"] = new PlannerAgent(_kernel, _logger),
            ["Maker"] = new MakerAgent(_kernel, _logger),
            ["Checker"] = new CheckerAgent(_kernel, _logger),
            ["Reflector"] = new ReflectorAgent(_kernel, _logger),
            ["DataFlow"] = new DataFlowAgent(_unitOfWork, _kernel, _logger),
            ["Validator"] = new ValidatorAgent(_kernel, _logger),
            ["BusinessLogic"] = new BusinessLogicAgent(_kernel, _logger),
            ["Optimizer"] = new OptimizerAgent(_kernel, _logger)
        };
    }

    public async Task<AgentResponse<T>> ExecuteAsync<T>(AgentExecutionContext context, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        try
        {
            context.StartTime = DateTime.UtcNow;
            
            // Create agent chain based on operation and entity type
            var agentChain = CreateAgentChain(context);
            
            // Execute through agent pipeline
            var result = await ExecuteAgentChain<T>(agentChain, context, cancellationToken);
            
            // Record execution metrics for self-optimization
            context.Duration = DateTime.UtcNow - context.StartTime;
            await RecordExecutionMetrics(context, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent chain for {EntityType}", typeof(T).Name);
            return AgentResponse<T>.Failure($"Agent execution failed: {ex.Message}");
        }
    }

    private List<IAgent> CreateAgentChain(AgentExecutionContext context)
    {
        var chain = new List<IAgent>();

        // Always start with validation
        chain.Add(_agents["Validator"]);

        // Add operation-specific agents
        switch (context.Operation.ToLower())
        {
            case "create":
            case "update":
                chain.Add(_agents["BusinessLogic"]);
                chain.Add(_agents["DataFlow"]);
                break;
            case "delete":
                chain.Add(_agents["DataFlow"]);
                break;
            case "getbyid":
            case "getall":
            case "query":
                chain.Add(_agents["DataFlow"]);
                chain.Add(_agents["Optimizer"]);
                break;
        }

        // Add reflection for learning
        chain.Add(_agents["Reflector"]);

        return chain;
    }

    private async Task<AgentResponse<T>> ExecuteAgentChain<T>(
        IEnumerable<IAgent> agentChain, 
        AgentExecutionContext context, 
        CancellationToken cancellationToken) where T : BaseEntity
    {
        var currentContext = context;
        
        foreach (var agent in agentChain)
        {
            currentContext.ExecutionLog.Add($"Executing {agent.Name} at {DateTime.UtcNow:HH:mm:ss.fff}");
            
            var agentResult = await agent.ExecuteAsync(currentContext, cancellationToken);
            
            if (!agentResult.IsSuccess)
            {
                currentContext.ExecutionLog.Add($"Agent {agent.Name} failed: {agentResult.Error}");
                return AgentResponse<T>.Failure(agentResult.Error);
            }
                
            currentContext = agentResult.Context;
            currentContext.ExecutionLog.Add($"Completed {agent.Name} at {DateTime.UtcNow:HH:mm:ss.fff}");
        }

        return AgentResponse<T>.Success((T)currentContext.Result!);
    }

    private async Task RecordExecutionMetrics(AgentExecutionContext context, AgentResponse<object> result)
    {
        // Record metrics for self-optimization
        var metrics = new
        {
            EntityType = context.EntityType.Name,
            Operation = context.Operation,
            Duration = context.Duration.TotalMilliseconds,
            Success = result.IsSuccess,
            ExecutionLog = context.ExecutionLog,
            Timestamp = DateTime.UtcNow
        };

        // Store metrics for analysis (could be database, telemetry, etc.)
        _logger.LogInformation("Execution metrics: {@Metrics}", metrics);
    }
}
```

## MIGRATION STRATEGY

### Phase 1: Parallel Implementation (Week 1)
1. Create BaseEntity and generic infrastructure alongside current system
2. Implement generic repository and unit of work
3. Test with existing entities (Agent, Skill, etc.)

### Phase 2: Agent Integration (Week 2)
1. Create generic agent service that uses existing agents
2. Implement generic controller
3. Test with existing endpoints

### Phase 3: Dynamic Registration (Week 3)
1. Implement auto-registration system
2. Add dynamic controller generation
3. Test with new entity types

### Phase 4: Self-Evolution (Week 4)
1. Add performance monitoring
2. Implement code optimization
3. Add self-modification capabilities

### Phase 5: Migration Completion (Week 5)
1. Migrate all existing entities to new system
2. Remove old manual controllers
3. Comprehensive testing and optimization

## SUCCESS CRITERIA

### Functional Requirements
- [ ] All existing functionality preserved
- [ ] New entities require zero boilerplate code
- [ ] Dynamic API generation works
- [ ] Agent orchestration handles any entity type
- [ ] Self-optimization capabilities active

### Performance Requirements
- [ ] No performance degradation from current system
- [ ] Agent response times < 200ms
- [ ] Memory usage optimized
- [ ] Self-optimization cycles working

### Business Requirements
- [ ] Development velocity increased 10x
- [ ] Maintenance overhead reduced 90%
- [ ] System can evolve without manual intervention
- [ ] New features can be added through configuration only

## OUTPUT FORMAT

Provide a structured response with:
- **Migration Plan**: Step-by-step evolution strategy
- **Code Changes**: Specific files to modify/create
- **Integration Points**: How to connect with existing system
- **Testing Strategy**: How to validate the evolution
- **Rollback Plan**: How to revert if issues arise
- **Success Metrics**: How to measure the evolution success

This meta prompt will guide the transformation of your current working system into the ultra-generic, self-evolving architecture while preserving all existing functionality and adding powerful new capabilities. 