-- Ultra-Generic System Database Schema
-- This script creates all tables for the ultra-generic system

-- Enable foreign keys
PRAGMA foreign_keys = ON;

-- Create Agents table
CREATE TABLE IF NOT EXISTS "Agents" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT,
    "Type" TEXT,
    "Configuration" TEXT,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create Skills table
CREATE TABLE IF NOT EXISTS "Skills" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "Language" TEXT DEFAULT 'C#',
    "Dependencies" TEXT,
    "CompilationOutput" TEXT,
    "IsCompiled" INTEGER NOT NULL DEFAULT 0,
    "HasErrors" INTEGER NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create AgentSkills junction table
CREATE TABLE IF NOT EXISTS "AgentSkills" (
    "AgentsId" TEXT NOT NULL,
    "SkillsId" TEXT NOT NULL,
    PRIMARY KEY ("AgentsId", "SkillsId"),
    FOREIGN KEY ("AgentsId") REFERENCES "Agents" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("SkillsId") REFERENCES "Skills" ("Id") ON DELETE CASCADE
);

-- Create AgentThreads table
CREATE TABLE IF NOT EXISTS "AgentThreads" (
    "Id" TEXT PRIMARY KEY,
    "ThreadId" TEXT NOT NULL UNIQUE,
    "AgentId" TEXT NOT NULL,
    "Context" TEXT,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "LastActivity" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExecutionData" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT,
    FOREIGN KEY ("AgentId") REFERENCES "Agents" ("Id") ON DELETE CASCADE
);

-- Create AgentExecutions table
CREATE TABLE IF NOT EXISTS "AgentExecutions" (
    "Id" TEXT PRIMARY KEY,
    "AgentId" TEXT NOT NULL,
    "ThreadId" TEXT NOT NULL,
    "Input" TEXT,
    "Output" TEXT,
    "StartTime" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "EndTime" TEXT,
    "Status" TEXT NOT NULL DEFAULT 'Running',
    "Error" TEXT,
    "ExecutionData" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT,
    FOREIGN KEY ("AgentId") REFERENCES "Agents" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("ThreadId") REFERENCES "AgentThreads" ("Id") ON DELETE CASCADE
);

-- Create KnowledgeEntries table
CREATE TABLE IF NOT EXISTS "KnowledgeEntries" (
    "Id" TEXT PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Category" TEXT,
    "Source" TEXT,
    "IsPublished" INTEGER NOT NULL DEFAULT 0,
    "LastUpdated" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create DynamicSkills table
CREATE TABLE IF NOT EXISTS "DynamicSkills" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "SourceCode" TEXT NOT NULL,
    "CompiledAssemblyPath" TEXT,
    "Dependencies" TEXT,
    "CompilationResult" TEXT,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "HasCompilationErrors" INTEGER NOT NULL DEFAULT 0,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create DocumentationEntries table
CREATE TABLE IF NOT EXISTS "DocumentationEntries" (
    "Id" TEXT PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Category" TEXT,
    "Format" TEXT DEFAULT 'Markdown',
    "Author" TEXT,
    "IsPublished" INTEGER NOT NULL DEFAULT 0,
    "PublishedDate" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create MemoryEntries table
CREATE TABLE IF NOT EXISTS "MemoryEntries" (
    "Id" TEXT PRIMARY KEY,
    "Key" TEXT NOT NULL UNIQUE,
    "Content" TEXT NOT NULL,
    "Description" TEXT,
    "Source" TEXT,
    "Collection" TEXT,
    "Embedding" TEXT,
    "SimilarityScore" REAL,
    "LastAccessed" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AccessCount" INTEGER NOT NULL DEFAULT 0,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create WhiteboardMemories table
CREATE TABLE IF NOT EXISTS "WhiteboardMemories" (
    "Id" TEXT PRIMARY KEY,
    "SessionId" TEXT NOT NULL,
    "AgentId" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "MessageType" TEXT,
    "Context" TEXT,
    "Timestamp" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsShared" INTEGER NOT NULL DEFAULT 0,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create VectorEmbeddings table
CREATE TABLE IF NOT EXISTS "VectorEmbeddings" (
    "Id" TEXT PRIMARY KEY,
    "Text" TEXT NOT NULL,
    "Embedding" TEXT NOT NULL,
    "Model" TEXT,
    "Collection" TEXT,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create Documents table
CREATE TABLE IF NOT EXISTS "Documents" (
    "Id" TEXT PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Type" TEXT,
    "Source" TEXT,
    "Url" TEXT,
    "IsProcessed" INTEGER NOT NULL DEFAULT 0,
    "ProcessedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create DocumentChunks table
CREATE TABLE IF NOT EXISTS "DocumentChunks" (
    "Id" TEXT PRIMARY KEY,
    "Content" TEXT NOT NULL,
    "ChunkIndex" INTEGER NOT NULL,
    "StartPosition" INTEGER NOT NULL,
    "EndPosition" INTEGER NOT NULL,
    "Embedding" TEXT,
    "DocumentId" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT,
    FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE CASCADE
);

-- Create CodeTemplates table
CREATE TABLE IF NOT EXISTS "CodeTemplates" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "TemplateType" TEXT NOT NULL,
    "TemplateContent" TEXT NOT NULL,
    "Description" TEXT,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Version" INTEGER NOT NULL DEFAULT 1,
    "Tags" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create DynamicPlugins table
CREATE TABLE IF NOT EXISTS "DynamicPlugins" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "PluginType" TEXT NOT NULL,
    "SourceCode" TEXT NOT NULL,
    "CompiledAssembly" BLOB,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsLoaded" INTEGER NOT NULL DEFAULT 0,
    "Description" TEXT,
    "Version" TEXT,
    "Configuration" TEXT,
    "Dependencies" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create SelfEvolutionConfigs table
CREATE TABLE IF NOT EXISTS "SelfEvolutionConfigs" (
    "Id" TEXT PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "EnableCodeGeneration" INTEGER NOT NULL DEFAULT 1,
    "EnableDynamicCompilation" INTEGER NOT NULL DEFAULT 1,
    "EnablePluginRegistration" INTEGER NOT NULL DEFAULT 1,
    "EnableSelfModification" INTEGER NOT NULL DEFAULT 0,
    "MaxGeneratedFiles" INTEGER NOT NULL DEFAULT 1000,
    "MaxCompilationTime" INTEGER NOT NULL DEFAULT 30000,
    "AllowedNamespaces" TEXT,
    "ForbiddenNamespaces" TEXT,
    "AdvancedSettings" TEXT,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT,
    "Metadata" TEXT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_Agents_Name" ON "Agents" ("Name");
CREATE INDEX IF NOT EXISTS "IX_Agents_IsActive" ON "Agents" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Agents_IsDeleted" ON "Agents" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_Skills_Name" ON "Skills" ("Name");
CREATE INDEX IF NOT EXISTS "IX_Skills_Language" ON "Skills" ("Language");
CREATE INDEX IF NOT EXISTS "IX_Skills_IsCompiled" ON "Skills" ("IsCompiled");
CREATE INDEX IF NOT EXISTS "IX_Skills_HasErrors" ON "Skills" ("HasErrors");
CREATE INDEX IF NOT EXISTS "IX_Skills_IsDeleted" ON "Skills" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_AgentThreads_ThreadId" ON "AgentThreads" ("ThreadId");
CREATE INDEX IF NOT EXISTS "IX_AgentThreads_AgentId" ON "AgentThreads" ("AgentId");
CREATE INDEX IF NOT EXISTS "IX_AgentThreads_IsActive" ON "AgentThreads" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_AgentThreads_LastActivity" ON "AgentThreads" ("LastActivity");
CREATE INDEX IF NOT EXISTS "IX_AgentThreads_IsDeleted" ON "AgentThreads" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_AgentExecutions_AgentId" ON "AgentExecutions" ("AgentId");
CREATE INDEX IF NOT EXISTS "IX_AgentExecutions_ThreadId" ON "AgentExecutions" ("ThreadId");
CREATE INDEX IF NOT EXISTS "IX_AgentExecutions_StartTime" ON "AgentExecutions" ("StartTime");
CREATE INDEX IF NOT EXISTS "IX_AgentExecutions_Status" ON "AgentExecutions" ("Status");
CREATE INDEX IF NOT EXISTS "IX_AgentExecutions_IsDeleted" ON "AgentExecutions" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_KnowledgeEntries_Title" ON "KnowledgeEntries" ("Title");
CREATE INDEX IF NOT EXISTS "IX_KnowledgeEntries_Category" ON "KnowledgeEntries" ("Category");
CREATE INDEX IF NOT EXISTS "IX_KnowledgeEntries_IsPublished" ON "KnowledgeEntries" ("IsPublished");
CREATE INDEX IF NOT EXISTS "IX_KnowledgeEntries_LastUpdated" ON "KnowledgeEntries" ("LastUpdated");
CREATE INDEX IF NOT EXISTS "IX_KnowledgeEntries_IsDeleted" ON "KnowledgeEntries" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_DynamicSkills_Name" ON "DynamicSkills" ("Name");
CREATE INDEX IF NOT EXISTS "IX_DynamicSkills_IsActive" ON "DynamicSkills" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_DynamicSkills_HasCompilationErrors" ON "DynamicSkills" ("HasCompilationErrors");
CREATE INDEX IF NOT EXISTS "IX_DynamicSkills_IsDeleted" ON "DynamicSkills" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_DocumentationEntries_Title" ON "DocumentationEntries" ("Title");
CREATE INDEX IF NOT EXISTS "IX_DocumentationEntries_Category" ON "DocumentationEntries" ("Category");
CREATE INDEX IF NOT EXISTS "IX_DocumentationEntries_IsPublished" ON "DocumentationEntries" ("IsPublished");
CREATE INDEX IF NOT EXISTS "IX_DocumentationEntries_Author" ON "DocumentationEntries" ("Author");
CREATE INDEX IF NOT EXISTS "IX_DocumentationEntries_IsDeleted" ON "DocumentationEntries" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_Key" ON "MemoryEntries" ("Key");
CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_Collection" ON "MemoryEntries" ("Collection");
CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_Source" ON "MemoryEntries" ("Source");
CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_LastAccessed" ON "MemoryEntries" ("LastAccessed");
CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_AccessCount" ON "MemoryEntries" ("AccessCount");
CREATE INDEX IF NOT EXISTS "IX_MemoryEntries_IsDeleted" ON "MemoryEntries" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_WhiteboardMemories_SessionId" ON "WhiteboardMemories" ("SessionId");
CREATE INDEX IF NOT EXISTS "IX_WhiteboardMemories_AgentId" ON "WhiteboardMemories" ("AgentId");
CREATE INDEX IF NOT EXISTS "IX_WhiteboardMemories_Timestamp" ON "WhiteboardMemories" ("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_WhiteboardMemories_IsShared" ON "WhiteboardMemories" ("IsShared");
CREATE INDEX IF NOT EXISTS "IX_WhiteboardMemories_IsDeleted" ON "WhiteboardMemories" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_VectorEmbeddings_Collection" ON "VectorEmbeddings" ("Collection");
CREATE INDEX IF NOT EXISTS "IX_VectorEmbeddings_Model" ON "VectorEmbeddings" ("Model");
CREATE INDEX IF NOT EXISTS "IX_VectorEmbeddings_CreatedAt" ON "VectorEmbeddings" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_VectorEmbeddings_IsDeleted" ON "VectorEmbeddings" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_Documents_Title" ON "Documents" ("Title");
CREATE INDEX IF NOT EXISTS "IX_Documents_Type" ON "Documents" ("Type");
CREATE INDEX IF NOT EXISTS "IX_Documents_Source" ON "Documents" ("Source");
CREATE INDEX IF NOT EXISTS "IX_Documents_IsProcessed" ON "Documents" ("IsProcessed");
CREATE INDEX IF NOT EXISTS "IX_Documents_ProcessedAt" ON "Documents" ("ProcessedAt");
CREATE INDEX IF NOT EXISTS "IX_Documents_IsDeleted" ON "Documents" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_DocumentId" ON "DocumentChunks" ("DocumentId");
CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_ChunkIndex" ON "DocumentChunks" ("ChunkIndex");
CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_StartPosition" ON "DocumentChunks" ("StartPosition");
CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_EndPosition" ON "DocumentChunks" ("EndPosition");
CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_IsDeleted" ON "DocumentChunks" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_CodeTemplates_Name" ON "CodeTemplates" ("Name");
CREATE INDEX IF NOT EXISTS "IX_CodeTemplates_TemplateType" ON "CodeTemplates" ("TemplateType");
CREATE INDEX IF NOT EXISTS "IX_CodeTemplates_IsActive" ON "CodeTemplates" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_CodeTemplates_Version" ON "CodeTemplates" ("Version");
CREATE INDEX IF NOT EXISTS "IX_CodeTemplates_IsDeleted" ON "CodeTemplates" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_Name" ON "DynamicPlugins" ("Name");
CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_PluginType" ON "DynamicPlugins" ("PluginType");
CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_IsActive" ON "DynamicPlugins" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_IsLoaded" ON "DynamicPlugins" ("IsLoaded");
CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_Version" ON "DynamicPlugins" ("Version");
CREATE INDEX IF NOT EXISTS "IX_DynamicPlugins_IsDeleted" ON "DynamicPlugins" ("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_SelfEvolutionConfigs_Name" ON "SelfEvolutionConfigs" ("Name");
CREATE INDEX IF NOT EXISTS "IX_SelfEvolutionConfigs_EnableCodeGeneration" ON "SelfEvolutionConfigs" ("EnableCodeGeneration");
CREATE INDEX IF NOT EXISTS "IX_SelfEvolutionConfigs_EnableDynamicCompilation" ON "SelfEvolutionConfigs" ("EnableDynamicCompilation");
CREATE INDEX IF NOT EXISTS "IX_SelfEvolutionConfigs_EnablePluginRegistration" ON "SelfEvolutionConfigs" ("EnablePluginRegistration");
CREATE INDEX IF NOT EXISTS "IX_SelfEvolutionConfigs_IsDeleted" ON "SelfEvolutionConfigs" ("IsDeleted");

-- Insert some initial data
INSERT OR IGNORE INTO "CodeTemplates" ("Id", "Name", "TemplateType", "TemplateContent", "Description", "IsActive", "Version", "Tags") 
VALUES (
    'template-1', 
    'Basic Entity Template', 
    'Entity', 
    'public class {{EntityName}} : BaseEntity
{
    {{#each Properties}}
    public {{Type}} {{Name}} { get; set; }
    {{/each}}
}', 
    'Basic entity template for creating new entities', 
    1, 
    1, 
    'entity, basic'
);

INSERT OR IGNORE INTO "SelfEvolutionConfigs" ("Id", "Name", "EnableCodeGeneration", "EnableDynamicCompilation", "EnablePluginRegistration", "EnableSelfModification", "MaxGeneratedFiles", "MaxCompilationTime", "AllowedNamespaces", "ForbiddenNamespaces", "AdvancedSettings") 
VALUES (
    'config-1', 
    'Default Configuration', 
    1, 
    1, 
    1, 
    0, 
    1000, 
    30000, 
    '["System", "System.Collections.Generic", "System.Linq", "System.Threading.Tasks", "Microsoft.EntityFrameworkCore", "UltraGenericSystem.Models"]', 
    '["System.Reflection.Emit", "System.Runtime.Remoting"]', 
    '{"EnableLogging": true, "EnableMetrics": true, "MaxRecursionDepth": 5}'
);

-- Create EF Core migration history table
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

-- Insert a dummy migration to prevent EF from trying to create migrations
INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('InitialCreate', '9.0.6'); 