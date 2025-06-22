using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using UltraGenericSystem.Data;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions and repository access
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly UltraGenericContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private string? _transactionId;

    public UnitOfWork(UltraGenericContext context, IServiceProvider serviceProvider, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var repository))
        {
            return (IGenericRepository<T>)repository;
        }

        var newRepository = _serviceProvider.GetRequiredService<IGenericRepository<T>>();
        _repositories[typeof(T)] = newRepository;
        return newRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            _logger.LogWarning("Transaction already exists, cannot begin new transaction");
            return;
        }

        try
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _transactionId = Guid.NewGuid().ToString();
            _logger.LogInformation("Began transaction {TransactionId}", _transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error beginning transaction");
            throw;
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _logger.LogWarning("No active transaction to commit");
            return;
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _logger.LogInformation("Committed transaction {TransactionId}", _transactionId);
            _transaction = null;
            _transactionId = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction {TransactionId}", _transactionId);
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _logger.LogWarning("No active transaction to rollback");
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _logger.LogInformation("Rolled back transaction {TransactionId}", _transactionId);
            _transaction = null;
            _transactionId = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction {TransactionId}", _transactionId);
            throw;
        }
    }

    public bool HasActiveTransaction => _transaction != null;

    public string? CurrentTransactionId => _transactionId;

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
} 