using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Outkeep.Caching;
using Outkeep.Hosting.Azure.Properties;
using Outkeep.Hosting.Azure.Storage.Models;
using Outkeep.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting.Azure.Storage
{
    internal class AzureTableCacheRegistryStorage : IHostedService, ICacheRegistryStorage
    {
        private readonly AzureTableCacheRegistryStorageOptions _options;
        private readonly CloudStorageAccount _account;
        private readonly CloudTableClient _client;
        private readonly CloudTable _table;

        public AzureTableCacheRegistryStorage(IOptions<AzureTableCacheRegistryStorageOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (!CloudStorageAccount.TryParse(_options.ConnectionString, out _account))
            {
                throw new OutkeepStorageException(Resources.Exception_InvalidCloudStorageAccountConnectionString);
            }

            _client = _account.CreateCloudTableClient();
            _client.TableClientConfiguration.UseRestExecutorForCosmosEndpoint = _options.UseRestExecutorForCosmosEndpoint;

            _table = _client.GetTableReference(_options.TableName);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _table.CreateIfNotExistsAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task WriteAsync(string key, int size, CancellationToken cancellationToken = default)
        {
            var entity = new CacheRegistryEntity
            {
                Key = key,
                Size = size
            };

            var operation = TableOperation.InsertOrMerge(entity);

            return _table.ExecuteAsync(operation, cancellationToken);
        }

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            var entity = new CacheRegistryEntity
            {
                Key = key
            };

            var operation = TableOperation.Delete(entity);

            return _table.ExecuteAsync(operation);
        }

        public async IAsyncEnumerable<CacheRegistryEntry> EnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var query = _table.CreateQuery<CacheRegistryEntity>();

            TableContinuationToken? token = null;
            do
            {
                var segment = await query.ExecuteSegmentedAsync(token, cancellationToken).ConfigureAwait(false);
                token = segment.ContinuationToken;

                var results = segment.Results;
                for (var i = 0; i < results.Count; i++)
                {
                    var entity = results[i];
                    yield return new CacheRegistryEntry(entity.Key, entity.Size);
                }
            }
            while (token != null);
        }
    }
}