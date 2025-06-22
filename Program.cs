using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Serilog;
using UltraGenericSystem.Data;
using UltraGenericSystem.Models;
using UltraGenericSystem.Models.SelfEvolution;
using UltraGenericSystem.Repositories;
using UltraGenericSystem.Services;
using UltraGenericSystem.Services.Agents;
using UltraGenericSystem.Services.SelfEvolution;
using UltraGenericSystem.Services.RuntimeCompilation;
using UltraGenericSystem.Services.DynamicAPI;
using UltraGenericSystem.Services.ProcessFramework;

namespace UltraGenericSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/ultra-generic-system-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Add services to the container
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure Entity Framework
        builder.Services.AddDbContext<UltraGenericContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure Semantic Kernel with support for both Azure OpenAI and Ollama
        builder.Services.AddSingleton<Kernel>(provider =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            
            // Add Azure OpenAI chat completion service (if configured)
            var azureEndpoint = builder.Configuration["AzureOpenAI:Endpoint"];
            var azureApiKey = builder.Configuration["AzureOpenAI:ApiKey"];
            var azureDeploymentName = builder.Configuration["AzureOpenAI:DeploymentName"];
            
            if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey))
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName: azureDeploymentName ?? "gpt-4",
                    endpoint: azureEndpoint,
                    apiKey: azureApiKey
                );
            }
            
            // Add Ollama chat completion service (if configured)
            var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"];
            var ollamaDefaultModel = builder.Configuration["Ollama:DefaultModel"];
            
            if (!string.IsNullOrEmpty(ollamaEndpoint))
            {
                // Register Ollama service first
                var ollamaConfig = new OllamaConfig
                {
                    Endpoint = ollamaEndpoint,
                    DefaultModel = ollamaDefaultModel ?? "llama3.2",
                    MaxTokens = int.TryParse(builder.Configuration["Ollama:MaxTokens"], out var maxTokens) ? maxTokens : 4096,
                    Temperature = double.TryParse(builder.Configuration["Ollama:Temperature"], out var temperature) ? temperature : 0.7,
                    TopP = double.TryParse(builder.Configuration["Ollama:TopP"], out var topP) ? topP : 0.9,
                    TopK = int.TryParse(builder.Configuration["Ollama:TopK"], out var topK) ? topK : 40,
                    RepeatPenalty = double.TryParse(builder.Configuration["Ollama:RepeatPenalty"], out var repeatPenalty) ? repeatPenalty : 1.1,
                    EnableStreaming = bool.TryParse(builder.Configuration["Ollama:EnableStreaming"], out var enableStreaming) ? enableStreaming : true,
                    EnableFunctionCalling = bool.TryParse(builder.Configuration["Ollama:EnableFunctionCalling"], out var enableFunctionCalling) ? enableFunctionCalling : true,
                    TimeoutSeconds = int.TryParse(builder.Configuration["Ollama:TimeoutSeconds"], out var timeout) ? timeout : 120,
                    RetryConfig = new OllamaRetryConfig
                    {
                        MaxRetries = int.TryParse(builder.Configuration["Ollama:RetryConfig:MaxRetries"], out var maxRetries) ? maxRetries : 3,
                        RetryDelaySeconds = int.TryParse(builder.Configuration["Ollama:RetryConfig:RetryDelaySeconds"], out var retryDelay) ? retryDelay : 2,
                        BackoffMultiplier = double.TryParse(builder.Configuration["Ollama:RetryConfig:BackoffMultiplier"], out var backoff) ? backoff : 2.0
                    }
                };
                
                var ollamaService = new OllamaService(
                    provider.GetRequiredService<ILogger<OllamaService>>(),
                    ollamaConfig);
                
                // Add Ollama services to the kernel
                kernelBuilder.Services.AddSingleton<IOllamaService>(ollamaService);
                kernelBuilder.Services.AddSingleton(ollamaService.CreateChatCompletionService());
                kernelBuilder.Services.AddSingleton(ollamaService.CreateTextGenerationService());
            }
            
            return kernelBuilder.Build();
        });

        // Register Ollama service (if configured)
        var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"];
        if (!string.IsNullOrEmpty(ollamaEndpoint))
        {
            builder.Services.AddSingleton<IOllamaService>(provider =>
            {
                var ollamaConfig = new OllamaConfig
                {
                    Endpoint = ollamaEndpoint,
                    DefaultModel = builder.Configuration["Ollama:DefaultModel"] ?? "llama3.2",
                    MaxTokens = int.TryParse(builder.Configuration["Ollama:MaxTokens"], out var maxTokens) ? maxTokens : 4096,
                    Temperature = double.TryParse(builder.Configuration["Ollama:Temperature"], out var temperature) ? temperature : 0.7,
                    TopP = double.TryParse(builder.Configuration["Ollama:TopP"], out var topP) ? topP : 0.9,
                    TopK = int.TryParse(builder.Configuration["Ollama:TopK"], out var topK) ? topK : 40,
                    RepeatPenalty = double.TryParse(builder.Configuration["Ollama:RepeatPenalty"], out var repeatPenalty) ? repeatPenalty : 1.1,
                    EnableStreaming = bool.TryParse(builder.Configuration["Ollama:EnableStreaming"], out var enableStreaming) ? enableStreaming : true,
                    EnableFunctionCalling = bool.TryParse(builder.Configuration["Ollama:EnableFunctionCalling"], out var enableFunctionCalling) ? enableFunctionCalling : true,
                    TimeoutSeconds = int.TryParse(builder.Configuration["Ollama:TimeoutSeconds"], out var timeout) ? timeout : 120,
                    RetryConfig = new OllamaRetryConfig
                    {
                        MaxRetries = int.TryParse(builder.Configuration["Ollama:RetryConfig:MaxRetries"], out var maxRetries) ? maxRetries : 3,
                        RetryDelaySeconds = int.TryParse(builder.Configuration["Ollama:RetryConfig:RetryDelaySeconds"], out var retryDelay) ? retryDelay : 2,
                        BackoffMultiplier = double.TryParse(builder.Configuration["Ollama:RetryConfig:BackoffMultiplier"], out var backoff) ? backoff : 2.0
                    }
                };
                
                return new OllamaService(
                    provider.GetRequiredService<ILogger<OllamaService>>(),
                    ollamaConfig);
            });
        }

        // Register repositories and unit of work
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register agent orchestrator
        builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>(sp =>
            new AgentOrchestrator(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<ILogger<AgentOrchestrator>>(),
                sp));
        
        // Register conversational logger for agent chat visibility
        builder.Services.AddScoped<IConversationalLogger, ConversationalLogger>();
        
        // Register SK Agent Factory
        builder.Services.AddScoped<SKAgentFactory>();

        // Register memory service
        builder.Services.AddScoped<IMemoryService, MemoryService>(sp =>
            new MemoryService(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<ILogger<MemoryService>>(),
                new MemoryConfig
                {
                    EnableMem0 = true,
                    EnableWhiteboard = true,
                    EnableRAG = true,
                    MaxMemoryEntries = 10000,
                    MaxWhiteboardEntries = 1000,
                    MaxRAGResults = 10,
                    MinSimilarityThreshold = 0.7,
                    EmbeddingModel = "text-embedding-ada-002",
                    VectorStoreType = "sqlite"
                }));

        // Register Process Framework service
        builder.Services.AddScoped<IKernelProcessService, KernelProcessService>();

        // Register Runtime Compilation service
        builder.Services.AddScoped<IRuntimeCompilationService, RuntimeCompilationService>();

        // Register Dynamic API Generator service
        builder.Services.AddScoped<IDynamicAPIGeneratorService, DynamicOpenAPIGenerator>(sp =>
            new DynamicOpenAPIGenerator(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<DynamicOpenAPIGenerator>>(),
                sp.GetRequiredService<SKAgentFactory>()));

        // Register Azure AI Agent service
        builder.Services.AddScoped<IAzureAIAgentService, AzureAIAgentService>(sp =>
            new AzureAIAgentService(
                sp.GetRequiredService<ILogger<AzureAIAgentService>>(),
                new AzureAIAgentConfig
                {
                    Endpoint = builder.Configuration["AzureOpenAI:Endpoint"] ?? "",
                    ApiKey = builder.Configuration["AzureOpenAI:ApiKey"] ?? "",
                    ModelDeploymentName = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4",
                    MaxTokens = int.TryParse(builder.Configuration["AzureOpenAI:MaxTokens"], out var maxTokens) ? maxTokens : 4000,
                    Temperature = double.TryParse(builder.Configuration["AzureOpenAI:Temperature"], out var temperature) ? temperature : 0.7,
                    EnableFunctionCalling = true,
                    EnableStreaming = true,
                    EnableVision = false,
                    MemoryConfig = new AzureAIMemoryConfig
                    {
                        EnableMemory = true,
                        MaxConversationHistory = 100,
                        EnableEmbeddings = true
                    }
                }));

        // Register self-evolution services
        builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>(sp =>
            new CodeGenerationService(
                sp.GetRequiredService<IGenericRepository<CodeTemplate>>(),
                sp.GetRequiredService<IGenericRepository<DynamicPlugin>>(),
                sp.GetRequiredService<IGenericRepository<SelfEvolutionConfig>>(),
                sp.GetRequiredService<ILogger<CodeGenerationService>>()));

        builder.Services.AddScoped<IDynamicPluginService, DynamicPluginService>(sp =>
            new DynamicPluginService(
                sp.GetRequiredService<IGenericRepository<DynamicPlugin>>(),
                sp.GetRequiredService<ICodeGenerationService>(),
                sp.GetRequiredService<ILogger<DynamicPluginService>>()));

        builder.Services.AddScoped<IStrangeLoopService, StrangeLoopService>(sp =>
            new StrangeLoopService(
                sp.GetRequiredService<ICodeGenerationService>(),
                sp.GetRequiredService<IDynamicPluginService>(),
                sp.GetRequiredService<IGenericRepository<Agent>>(),
                sp.GetRequiredService<IGenericRepository<Skill>>(),
                sp.GetRequiredService<IGenericRepository<SelfEvolutionConfig>>(),
                sp.GetRequiredService<ILogger<StrangeLoopService>>()));

        // Register Swagger-to-SK Plugin service
        builder.Services.AddScoped<ISwaggerToSKPluginService, SwaggerToSKPluginService>(sp =>
            new SwaggerToSKPluginService(
                sp.GetRequiredService<ILogger<SwaggerToSKPluginService>>(),
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<Kernel>()));

        // Register Meta-Agent Self-Evolution service
        builder.Services.AddScoped<IMetaAgentSelfEvolution, MetaAgentSelfEvolution>(sp =>
            new MetaAgentSelfEvolution(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<MetaAgentSelfEvolution>>(),
                sp.GetRequiredService<ICodeGenerationService>(),
                sp.GetRequiredService<IDynamicPluginService>(),
                sp.GetRequiredService<IRuntimeCompilationService>(),
                sp.GetRequiredService<IDynamicAPIGeneratorService>(),
                sp.GetRequiredService<IKernelProcessService>()));

        // Register Runtime Plugin Loader service
        builder.Services.AddScoped<IRuntimePluginLoader, RuntimePluginLoader>(sp =>
            new RuntimePluginLoader(
                sp.GetRequiredService<Kernel>(),
                sp.GetRequiredService<ILogger<RuntimePluginLoader>>(),
                sp.GetRequiredService<IRuntimeCompilationService>()));

        // Register agent services for all entity types
        RegisterGenericServices(builder.Services);

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        // Enable Swagger in all environments for easier API exploration
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ultra Generic System API v1");
            c.RoutePrefix = string.Empty; // Set to empty to serve Swagger UI at root
        });

        if (app.Environment.IsDevelopment())
        {
            // Additional development-specific configurations can go here
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        // Initialize database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UltraGenericContext>();
            context.Database.EnsureCreated();
            
            // Seed initial data if needed
            SeedInitialData(context);
        }

        Log.Information("Ultra-Generic System started successfully");
        app.Run();
    }

    /// <summary>
    /// Registers generic services for all entity types that inherit from BaseEntity
    /// </summary>
    static void RegisterGenericServices(IServiceCollection services)
    {
        // Get all types that inherit from BaseEntity
        var entityTypes = typeof(BaseEntity).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        Log.Information("Found {Count} entity types to register", entityTypes.Count);

        foreach (var entityType in entityTypes)
        {
            Log.Information("Registering services for entity type: {EntityType}", entityType.Name);

            // Register generic repository
            var repositoryInterface = typeof(IGenericRepository<>).MakeGenericType(entityType);
            var repositoryImplementation = typeof(GenericRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryInterface, repositoryImplementation);

            // Register generic service
            var serviceInterface = typeof(IAgentService<>).MakeGenericType(entityType);
            var serviceImplementation = typeof(AgentService<>).MakeGenericType(entityType);
            services.AddScoped(serviceInterface, serviceImplementation);

            // Do NOT register generic controller here (controllers are discovered automatically)

            Log.Information("Successfully registered services for {EntityType}", entityType.Name);
        }
    }

    /// <summary>
    /// Seeds initial data for the system
    /// </summary>
    static void SeedInitialData(UltraGenericContext context)
    {
        try
        {
            // Check if we already have data
            if (context.Agents.Any())
            {
                Log.Information("Database already contains data, skipping seed");
                return;
            }

            Log.Information("Seeding initial data...");

            // Create sample agents
            var orchestratorAgent = new Agent
            {
                Name = "Orchestrator",
                Type = "Orchestrator",
                Description = "Main orchestrator agent for coordinating other agents",
                PromptTemplatePath = "PromptyTemplates/orchestrator.prompt",
                Configuration = "{\"maxConcurrentOperations\": 10}",
                Version = "1.0.0",
                IsActive = true,
                Status = "Active"
            };
            orchestratorAgent.OnCreated("System");

            var plannerAgent = new Agent
            {
                Name = "Planner",
                Type = "Planner",
                Description = "Agent responsible for planning and strategy",
                PromptTemplatePath = "PromptyTemplates/planner.prompt",
                Configuration = "{\"planningDepth\": 5}",
                Version = "1.0.0",
                IsActive = true,
                Status = "Active"
            };
            plannerAgent.OnCreated("System");

            var makerAgent = new Agent
            {
                Name = "Maker",
                Type = "Maker",
                Description = "Agent responsible for execution and implementation",
                PromptTemplatePath = "PromptyTemplates/maker.prompt",
                Configuration = "{\"executionTimeout\": 300}",
                Version = "1.0.0",
                IsActive = true,
                Status = "Active"
            };
            makerAgent.OnCreated("System");

            var checkerAgent = new Agent
            {
                Name = "Checker",
                Type = "Checker",
                Description = "Agent responsible for validation and quality assurance",
                PromptTemplatePath = "PromptyTemplates/checker.prompt",
                Configuration = "{\"validationStrictness\": \"high\"}",
                Version = "1.0.0",
                IsActive = true,
                Status = "Active"
            };
            checkerAgent.OnCreated("System");

            var reflectorAgent = new Agent
            {
                Name = "Reflector",
                Type = "Reflector",
                Description = "Agent responsible for reflection and learning",
                PromptTemplatePath = "PromptyTemplates/reflector.prompt",
                Configuration = "{\"learningRate\": 0.1}",
                Version = "1.0.0",
                IsActive = true,
                Status = "Active"
            };
            reflectorAgent.OnCreated("System");

            // Create sample skills
            var dataFlowSkill = new Skill
            {
                Name = "DataFlow",
                Code = @"
public class DataFlowSkill
{
    public async Task<object> ExecuteAsync(object input)
    {
        // Data flow processing logic
        return await ProcessDataFlow(input);
    }
}",
                Language = "C#",
                Dependencies = new[] { "Microsoft.EntityFrameworkCore", "System.Linq" },
                IsCompiled = true,
                LastCompiled = DateTime.UtcNow,
                CompilationOutput = "Success",
                HasErrors = false
            };
            dataFlowSkill.OnCreated("System");

            var validatorSkill = new Skill
            {
                Name = "Validator",
                Code = @"
public class ValidatorSkill
{
    public async Task<bool> ValidateAsync(object input)
    {
        // Validation logic
        return await ValidateInput(input);
    }
}",
                Language = "C#",
                Dependencies = new[] { "FluentValidation" },
                IsCompiled = true,
                LastCompiled = DateTime.UtcNow,
                CompilationOutput = "Success",
                HasErrors = false
            };
            validatorSkill.OnCreated("System");

            // Create sample knowledge entries
            var systemKnowledge = new KnowledgeEntry
            {
                Title = "Ultra-Generic System Architecture",
                Content = "The Ultra-Generic System is a meta-programmable, self-evolving AI architecture...",
                Category = "Architecture",
                Source = "System Documentation",
                IsPublished = true
            };
            systemKnowledge.OnCreated("System");

            var agentKnowledge = new KnowledgeEntry
            {
                Title = "Agent Orchestration Patterns",
                Content = "Multi-agent orchestration patterns for complex task execution...",
                Category = "Agents",
                Source = "Research Papers",
                IsPublished = true
            };
            agentKnowledge.OnCreated("System");

            // Create sample documentation
            var apiDocs = new DocumentationEntry
            {
                Title = "API Documentation",
                Content = "# Ultra-Generic System API\n\nThis system provides a generic API for all entities...",
                Category = "API",
                Format = "Markdown",
                Author = "System",
                IsPublished = true,
                PublishedDate = DateTime.UtcNow
            };
            apiDocs.OnCreated("System");

            // Add entities to context
            context.Agents.AddRange(orchestratorAgent, plannerAgent, makerAgent, checkerAgent, reflectorAgent);
            context.Skills.AddRange(dataFlowSkill, validatorSkill);
            context.KnowledgeEntries.AddRange(systemKnowledge, agentKnowledge);
            context.DocumentationEntries.Add(apiDocs);

            // Save changes
            context.SaveChanges();

            Log.Information("Successfully seeded initial data");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding initial data");
        }
    }
} 