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
    internal class AzureTableCacheRegistryStorage : ICacheRegistryStorage, IHostedService
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

        public Task ReadStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerReadStateAsync(state);
        }

        private async Task InnerReadStateAsync(ICacheRegistryEntryState state)
        {
            var operation = TableOperation.Retrieve<AzureTableCacheRegistryEntity>(state.Key, _options.DataRowKey);

            try
            {
                var result = await _table.ExecuteAsync(operation).ConfigureAwait(false);
                if (result.Result is AzureTableCacheRegistryEntity entity)
                {
                    state.Size = entity.Size;
                    state.AbsoluteExpiration = entity.AbsoluteExpiration;
                    state.SlidingExpiration = entity.SlidingExpiration;
                    state.ETag = entity.ETag;
                }
                else
                {
                    throw new AzureTableCacheRegistryException(Resources.Exception_RetrieveOperationFailed, _options.TableName, state.Key, _options.DataRowKey);
                }
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_RetrieveOperationFailed, _options.TableName, state.Key, _options.DataRowKey, exception);
            }
        }

        public Task WriteStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerWriteStateAsync(state);
        }

        private async Task InnerWriteStateAsync(ICacheRegistryEntryState state)
        {
            var entity = new AzureTableCacheRegistryEntity
            {
                PartitionKey = state.Key,
                RowKey = _options.DataRowKey,
                Size = state.Size,
                AbsoluteExpiration = state.AbsoluteExpiration,
                SlidingExpiration = state.SlidingExpiration,
                ETag = state.ETag
            };

            var operation = TableOperation.InsertOrReplace(entity);

            try
            {
                var result = await _table.ExecuteAsync(operation).ConfigureAwait(false);
                if (result.Result is AzureTableCacheRegistryEntity inserted)
                {
                    state.ETag = inserted.ETag;
                }
                else
                {
                    throw new AzureTableCacheRegistryException(Resources.Exception_InsertOrReplaceOperationFailed, _options.TableName, entity.PartitionKey, entity.RowKey);
                }
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_InsertOrReplaceOperationFailed, _options.TableName, entity.PartitionKey, entity.RowKey, exception);
            }
        }

        public Task ClearStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerClearStateAsync(state);
        }

        private async Task InnerClearStateAsync(ICacheRegistryEntryState state)
        {
            var entity = new AzureTableCacheRegistryEntity
            {
                PartitionKey = state.Key,
                RowKey = _options.DataRowKey,
                ETag = state.ETag
            };

            var operation = TableOperation.Delete(entity);

            try
            {
                await _table.ExecuteAsync(operation).ConfigureAwait(false);
            }
            catch (StorageException exception)
            {
                throw new AzureTableCacheRegistryException(Resources.Exception_DeleteOperationFailed, _options.TableName, entity.PartitionKey, entity.RowKey, exception);
            }
        }

        public IQueryable<ICacheRegistryEntryState> CreateQuery()
        {
            return _table.CreateQuery<AzureTableCacheRegistryEntity>();
        }

        public Task StartAsync(CancellationToken cancellationToken) => _table.CreateIfNotExistsAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private class AzureTableCacheRegistryEntity : TableEntity, ICacheRegistryEntryState
        {
            public int? Size { get; set; }
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }

            #region ICacheRegistryEntryState

            string ICacheRegistryEntryState.Key => PartitionKey;
            string? ICacheRegistryEntryState.ETag { get => ETag; set => ETag = value; }

            #endregion ICacheRegistryEntryState
        }
    }
}