using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UltraGenericSystem.Models;

/// <summary>
/// Abstract base class for all entities in the ultra-generic system.
/// Provides common properties and functionality that all entities share.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created the entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Flexible metadata storage for entity-specific data
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// The type name of the entity
    /// </summary>
    public string EntityType => GetType().Name;

    /// <summary>
    /// Version for optimistic concurrency
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Tags for categorization and search
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Description of the entity
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Audit method for entity creation
    /// </summary>
    public virtual void OnCreated(string createdBy = "System")
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        Version = 1;
    }

    /// <summary>
    /// Audit method for entity updates
    /// </summary>
    public virtual void OnUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Audit method for entity deletion
    /// </summary>
    public virtual void OnDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp
    /// </summary>
    public virtual void UpdateTimestamp()
    {
        OnUpdated();
    }

    /// <summary>
    /// Marks the entity as deleted (soft delete)
    /// </summary>
    public virtual void MarkAsDeleted()
    {
        OnDeleted();
    }

    /// <summary>
    /// Restores the entity from soft delete
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Adds metadata to the entity
    /// </summary>
    public virtual void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
        UpdateTimestamp();
    }

    /// <summary>
    /// Gets metadata value with type conversion
    /// </summary>
    public virtual T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Adds tags to the entity
    /// </summary>
    public virtual void AddTags(params string[] tags)
    {
        var existingTags = new HashSet<string>(Tags);
        foreach (var tag in tags)
        {
            existingTags.Add(tag);
        }
        Tags = existingTags.ToArray();
        UpdateTimestamp();
    }

    /// <summary>
    /// Removes tags from the entity
    /// </summary>
    public virtual void RemoveTags(params string[] tags)
    {
        var existingTags = new HashSet<string>(Tags);
        foreach (var tag in tags)
        {
            existingTags.Remove(tag);
        }
        Tags = existingTags.ToArray();
        UpdateTimestamp();
    }
} 