using Microsoft.EntityFrameworkCore;
using UltraGenericSystem.Models;
using UltraGenericSystem.Models.SelfEvolution;
using System.Linq.Expressions;

namespace UltraGenericSystem.Data;

/// <summary>
/// Database context for the Ultra-Generic System
/// </summary>
public class UltraGenericContext : DbContext
{
    public UltraGenericContext(DbContextOptions<UltraGenericContext> options) : base(options)
    {
    }

    // Entity DbSets
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<AgentThread> AgentThreads { get; set; }
    public DbSet<AgentExecution> AgentExecutions { get; set; }
    public DbSet<KnowledgeEntry> KnowledgeEntries { get; set; }
    public DbSet<DynamicSkill> DynamicSkills { get; set; }
    public DbSet<DocumentationEntry> DocumentationEntries { get; set; }

    // Memory DbSets
    public DbSet<MemoryEntry> MemoryEntries { get; set; }
    public DbSet<WhiteboardMemory> WhiteboardMemories { get; set; }
    public DbSet<VectorEmbedding> VectorEmbeddings { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    // Self-Evolution DbSets
    public DbSet<CodeTemplate> CodeTemplates { get; set; }
    public DbSet<DynamicPlugin> DynamicPlugins { get; set; }
    public DbSet<SelfEvolutionConfig> SelfEvolutionConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BaseEntity
        ConfigureBaseEntity(modelBuilder);

        // Configure specific entities
        ConfigureAgent(modelBuilder);
        ConfigureSkill(modelBuilder);
        ConfigureAgentThread(modelBuilder);
        ConfigureAgentExecution(modelBuilder);
        ConfigureKnowledgeEntry(modelBuilder);
        ConfigureDynamicSkill(modelBuilder);
        ConfigureDocumentationEntry(modelBuilder);

        // Configure memory entities
        ConfigureMemoryEntry(modelBuilder);
        ConfigureWhiteboardMemory(modelBuilder);
        ConfigureVectorEmbedding(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureDocumentChunk(modelBuilder);

        // Configure self-evolution entities
        ConfigureCodeTemplate(modelBuilder);
        ConfigureDynamicPlugin(modelBuilder);
        ConfigureSelfEvolutionConfig(modelBuilder);

        // Configure relationships
        ConfigureRelationships(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        // Apply BaseEntity configuration to all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                // Configure Id as primary key
                modelBuilder.Entity(entityType.ClrType)
                    .HasKey("Id");

                // Configure CreatedAt with default value
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure UpdatedAt with default value and update trigger
                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure IsDeleted with default value
                modelBuilder.Entity(entityType.ClrType)
                    .Property("IsDeleted")
                    .HasDefaultValue(false);

                // Configure Version with default value
                modelBuilder.Entity(entityType.ClrType)
                    .Property("Version")
                    .HasDefaultValue(1);

                // Configure Metadata as JSON with ValueConverter and ValueComparer
                var dictConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, object>, string>(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );
                var dictComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );
                modelBuilder.Entity(entityType.ClrType)
                    .Property("Metadata")
                    .HasConversion(dictConverter)
                    .Metadata.SetValueComparer(dictComparer);

                // Configure Tags as JSON with ValueConverter and ValueComparer
                var tagsConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string[], string>(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<string[]>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? Array.Empty<string>()
                );
                var tagsComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<string[]>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => System.Text.Json.JsonSerializer.Deserialize<string[]>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null) ?? Array.Empty<string>()
                );
                modelBuilder.Entity(entityType.ClrType)
                    .Property("Tags")
                    .HasConversion(tagsConverter)
                    .Metadata.SetValueComparer(tagsComparer);

                // Add global query filter for soft delete
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateIsDeletedFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression CreateIsDeletedFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(bool) }, parameter, Expression.Constant("IsDeleted"));
        var notDeleted = Expression.Not(property);
        return Expression.Lambda(notDeleted, parameter);
    }

    private void ConfigureAgent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.PromptTemplatePath)
                .HasMaxLength(500);

            entity.Property(e => e.Configuration)
                .HasColumnType("TEXT");

            entity.Property(e => e.Version)
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Idle");

            entity.Property(e => e.LastActive);

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureSkill(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Code)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Language)
                .HasMaxLength(20)
                .HasDefaultValue("C#");

            entity.Property(e => e.Dependencies)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<string[]>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? Array.Empty<string>()
                );

            entity.Property(e => e.CompilationOutput)
                .HasColumnType("TEXT");

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.IsCompiled);
            entity.HasIndex(e => e.HasErrors);
        });
    }

    private void ConfigureAgentThread(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentThread>(entity =>
        {
            entity.ToTable("AgentThreads");

            entity.Property(e => e.ThreadId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Context)
                .HasColumnType("TEXT");

            entity.Property(e => e.LastActivity)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExecutionData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );

            // Indexes
            entity.HasIndex(e => e.ThreadId).IsUnique();
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.LastActivity);
        });
    }

    private void ConfigureAgentExecution(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentExecution>(entity =>
        {
            entity.ToTable("AgentExecutions");

            entity.Property(e => e.Input)
                .HasColumnType("TEXT");

            entity.Property(e => e.Output)
                .HasColumnType("TEXT");

            entity.Property(e => e.StartTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Running");

            entity.Property(e => e.Error)
                .HasColumnType("TEXT");

            entity.Property(e => e.ExecutionData)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );

            // Indexes
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.ThreadId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureKnowledgeEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KnowledgeEntry>(entity =>
        {
            entity.ToTable("KnowledgeEntries");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Content)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.Source)
                .HasMaxLength(500);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.LastUpdated);
        });
    }

    private void ConfigureDynamicSkill(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DynamicSkill>(entity =>
        {
            entity.ToTable("DynamicSkills");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.SourceCode)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.CompiledAssemblyPath)
                .HasMaxLength(500);

            entity.Property(e => e.Dependencies)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<string[]>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? Array.Empty<string>()
                );

            entity.Property(e => e.CompilationResult)
                .HasColumnType("TEXT");

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.HasCompilationErrors);
        });
    }

    private void ConfigureDocumentationEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentationEntry>(entity =>
        {
            entity.ToTable("DocumentationEntries");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Content)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.Format)
                .HasMaxLength(20)
                .HasDefaultValue("Markdown");

            entity.Property(e => e.Author)
                .HasMaxLength(100);

            entity.Property(e => e.PublishedDate);

            // Indexes
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.Author);
        });
    }

    private void ConfigureMemoryEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemoryEntry>(entity =>
        {
            entity.ToTable("MemoryEntries");

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Source)
                .HasMaxLength(100);

            entity.Property(e => e.Collection)
                .HasMaxLength(100);

            entity.Property(e => e.Embedding)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<float>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<float>()
                );

            entity.Property(e => e.SimilarityScore);

            entity.Property(e => e.LastAccessed);

            entity.Property(e => e.AccessCount)
                .HasDefaultValue(0);

            // Indexes
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Collection);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.LastAccessed);
            entity.HasIndex(e => e.AccessCount);
        });
    }

    private void ConfigureWhiteboardMemory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WhiteboardMemory>(entity =>
        {
            entity.ToTable("WhiteboardMemories");

            entity.Property(e => e.SessionId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.AgentId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.MessageType)
                .HasMaxLength(100);

            entity.Property(e => e.Timestamp);

            entity.Property(e => e.IsShared)
                .HasDefaultValue(false);

            // Indexes
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsShared);
        });
    }

    private void ConfigureVectorEmbedding(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorEmbedding>(entity =>
        {
            entity.ToTable("VectorEmbeddings");

            entity.Property(e => e.Text)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.Embedding)
                .IsRequired()
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<float>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<float>()
                );

            entity.Property(e => e.Model)
                .HasMaxLength(100);

            entity.Property(e => e.Collection)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt);

            // Indexes
            entity.HasIndex(e => e.Collection);
            entity.HasIndex(e => e.Model);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.Type)
                .HasMaxLength(100);

            entity.Property(e => e.Source)
                .HasMaxLength(100);

            entity.Property(e => e.Url)
                .HasMaxLength(1000);

            entity.Property(e => e.IsProcessed)
                .HasDefaultValue(false);

            entity.Property(e => e.ProcessedAt);

            // Indexes
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.IsProcessed);
            entity.HasIndex(e => e.ProcessedAt);
        });
    }

    private void ConfigureDocumentChunk(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("DocumentChunks");

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.ChunkIndex);

            entity.Property(e => e.StartPosition);

            entity.Property(e => e.EndPosition);

            entity.Property(e => e.Embedding)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<float>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<float>()
                );

            entity.Property(e => e.DocumentId);

            // Foreign key relationship
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.ChunkIndex);
            entity.HasIndex(e => e.StartPosition);
            entity.HasIndex(e => e.EndPosition);
        });
    }

    private void ConfigureCodeTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CodeTemplate>(entity =>
        {
            entity.ToTable("CodeTemplates");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TemplateType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TemplateContent)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.Version)
                .HasDefaultValue(1);

            entity.Property(e => e.Tags)
                .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.TemplateType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Version);
        });
    }

    private void ConfigureDynamicPlugin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DynamicPlugin>(entity =>
        {
            entity.ToTable("DynamicPlugins");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PluginType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.SourceCode)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.CompiledAssembly)
                .HasColumnType("BLOB");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsLoaded)
                .HasDefaultValue(false);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Version)
                .HasMaxLength(20);

            entity.Property(e => e.Configuration)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );

            entity.Property(e => e.Dependencies)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                );

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.PluginType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsLoaded);
            entity.HasIndex(e => e.Version);
        });
    }

    private void ConfigureSelfEvolutionConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SelfEvolutionConfig>(entity =>
        {
            entity.ToTable("SelfEvolutionConfigs");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EnableCodeGeneration)
                .HasDefaultValue(true);

            entity.Property(e => e.EnableDynamicCompilation)
                .HasDefaultValue(true);

            entity.Property(e => e.EnablePluginRegistration)
                .HasDefaultValue(true);

            entity.Property(e => e.EnableSelfModification)
                .HasDefaultValue(false);

            entity.Property(e => e.MaxGeneratedFiles)
                .HasDefaultValue(1000);

            entity.Property(e => e.MaxCompilationTime)
                .HasDefaultValue(30000);

            entity.Property(e => e.AllowedNamespaces)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.Property(e => e.ForbiddenNamespaces)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                );

            entity.Property(e => e.AdvancedSettings)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.EnableCodeGeneration);
            entity.HasIndex(e => e.EnableDynamicCompilation);
            entity.HasIndex(e => e.EnablePluginRegistration);
        });
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Agent - Skill (Many-to-Many)
        modelBuilder.Entity<Agent>()
            .HasMany(a => a.Skills)
            .WithMany(s => s.Agents)
            .UsingEntity(j => j.ToTable("AgentSkills"));

        // Agent - AgentThread (One-to-Many)
        modelBuilder.Entity<Agent>()
            .HasMany(a => a.Threads)
            .WithOne(t => t.Agent)
            .HasForeignKey(t => t.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Agent - AgentExecution (One-to-Many)
        modelBuilder.Entity<Agent>()
            .HasMany(a => a.Executions)
            .WithOne(e => e.Agent)
            .HasForeignKey(e => e.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        // AgentThread - AgentExecution (One-to-Many)
        modelBuilder.Entity<AgentThread>()
            .HasMany(t => t.Executions)
            .WithOne(e => e.Thread)
            .HasForeignKey(e => e.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Global indexes for performance
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                // Index on CreatedAt for time-based queries
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("CreatedAt");

                // Index on UpdatedAt for change tracking
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("UpdatedAt");

                // Index on CreatedBy for user-based queries
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("CreatedBy");

                // Composite index for common queries
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("IsDeleted", "CreatedAt");
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.Version = 1;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.Version++;
            }
        }
    }
} 