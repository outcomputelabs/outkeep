using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Storage;
using Outkeep.Hosting.Azure.Properties;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public class AzureTableCacheRegistryStorage : ICacheRegistryStorage, IHostedService
    {
        private readonly AzureTableCacheRegistryStorageOptions _options;
        private readonly CloudTable _table;

        public AzureTableCacheRegistryStorage(IOptions<AzureTableCacheRegistryStorageOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (!CloudStorageAccount.TryParse(_options.ConnectionString, out var account))
            {
                throw new BadProviderConfigException(Resources.Exception_InvalidCloudStorageAccountConnectionString);
            }

            _table = account.CreateCloudTableClient().GetTableReference(_options.TableName);
        }

        public Task<CacheRegistryEntry?> ReadAsync(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return InnerReadAsync(key);
        }

        private async Task<CacheRegistryEntry?> InnerReadAsync(string key)
        {
            var operation = TableOperation.Retrieve<AzureTableCacheRegistryEntity>(_options.MainPartitionKey, key);

            try
            {
                var result = await _table.ExecuteAsync(operation).ConfigureAwait(false);
                if (result.Result is AzureTableCacheRegistryEntity entity)
                {
                    return entity.ToEntry();
                }
                else
                {
                    throw new AzureTableCacheRegistryException(Resources.Exception_RetrieveOperationFailed, _options.TableName, _options.MainPartitionKey, key);
                }
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_RetrieveOperationFailed, _options.TableName, _options.MainPartitionKey, key, exception);
            }
        }

        public Task ClearAsync(CacheRegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerClearAsync(entry);
        }

        private async Task InnerClearAsync(CacheRegistryEntry entry)
        {
            var entity = AzureTableCacheRegistryEntity.FromEntry(_options.MainPartitionKey, entry);
            var operation = TableOperation.Delete(entity);

            try
            {
                await _table.ExecuteAsync(operation).ConfigureAwait(false);
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_DeleteOperationFailed, _options.TableName, _options.MainPartitionKey, entity.RowKey, exception);
            }
        }

        public Task<CacheRegistryEntry> WriteAsync(CacheRegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerWriteAsync(entry);
        }

        private async Task<CacheRegistryEntry> InnerWriteAsync(CacheRegistryEntry entry)
        {
            var entity = AzureTableCacheRegistryEntity.FromEntry(_options.MainPartitionKey, entry);
            var operation = TableOperation.InsertOrReplace(entity);

            try
            {
                var result = await _table.ExecuteAsync(operation).ConfigureAwait(false);
                if (result.Result is AzureTableCacheRegistryEntity inserted)
                {
                    return inserted.ToEntry();
                }
                else
                {
                    throw new AzureTableCacheRegistryException(Resources.Exception_InsertOrReplaceOperationFailed, _options.TableName, _options.MainPartitionKey, entity.RowKey);
                }
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_InsertOrReplaceOperationFailed, _options.TableName, _options.MainPartitionKey, entity.RowKey, exception);
            }
        }

        public IQueryable<CacheRegistryEntry> CreateQuery()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken) => _table.CreateIfNotExistsAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}