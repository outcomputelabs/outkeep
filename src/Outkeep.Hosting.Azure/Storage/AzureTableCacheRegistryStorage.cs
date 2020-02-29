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
    public class AzureTableCacheRegistryStorage : IHostedService, ICacheRegistryStorage
    {
        private readonly string _name;
        private readonly CloudTable _table;

        public AzureTableCacheRegistryStorage(string name, IOptionsSnapshot<AzureTableCacheRegistryStorageOptions> optionsSnapshot)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            if (optionsSnapshot is null) throw new ArgumentNullException(nameof(optionsSnapshot));

            var options = optionsSnapshot.Get(_name);

            if (!CloudStorageAccount.TryParse(options.ConnectionString, out var account))
            {
                throw new OutkeepStorageException(Resources.Exception_InvalidCloudStorageAccountConnectionString);
            }

            var client = account.CreateCloudTableClient();
            client.TableClientConfiguration.UseRestExecutorForCosmosEndpoint = options.UseRestExecutorForCosmosEndpoint;

            _table = client.GetTableReference(options.TableName);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _table.CreateIfNotExistsAsync(cancellationToken);
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