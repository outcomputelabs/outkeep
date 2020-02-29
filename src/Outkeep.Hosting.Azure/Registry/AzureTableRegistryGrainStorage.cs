using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Outkeep.Hosting.Azure.Properties;
using Outkeep.Registry;
using Outkeep.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Hosting.Azure.Registry
{
    internal class AzureTableRegistryGrainStorage : IRegistryGrainStorage, IHostedService
    {
        private const int MaxDataChunksCount = 15;

        private readonly string _storageName;
        private readonly AzureTableRegistryGrainStorageOptions _options;

        private readonly CloudTable _table;
        private readonly SerializationManager _serializer;
        private readonly ILogger _logger;
        private readonly ISystemClock _clock;
        private readonly ObjectPool<BinaryTokenStreamReader> _readerPool;

        public AzureTableRegistryGrainStorage(string storageName, IOptionsSnapshot<AzureTableRegistryGrainStorageOptions> optionsSnapshot, SerializationManager serializer, ILogger<AzureTableRegistryGrainStorage> logger, ISystemClock clock, ObjectPool<BinaryTokenStreamReader> readerPool)
        {
            _storageName = storageName ?? throw new ArgumentNullException(nameof(storageName));
            _options = optionsSnapshot?.Get(_storageName) ?? throw new ArgumentNullException(nameof(optionsSnapshot));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _readerPool = readerPool ?? throw new ArgumentNullException(nameof(readerPool));

            if (!CloudStorageAccount.TryParse(_options.ConnectionString, out var account))
            {
                throw new OutkeepStorageException(Resources.Exception_InvalidCloudStorageAccountConnectionString);
            }

            var client = account.CreateCloudTableClient();
            client.TableClientConfiguration.UseRestExecutorForCosmosEndpoint = _options.UseRestExecutorForCosmosEndpoint;

            _table = client.GetTableReference(_options.TableName);
        }

        private static string GetPartitionKey(string grainType, GrainReference grainReference)
        {
            var grainKeyString = grainReference.ToKeyString();

            return string.Create(grainType.Length + 1 + grainKeyString.Length, (grainType, grainKeyString), (span, args) =>
            {
                args.grainType.AsSpan().CopyTo(span);
                span = span.Slice(args.grainType.Length);

                span[0] = '|';
                span = span.Slice(1);

                args.grainKeyString.AsSpan().CopyTo(span);
            });
        }

        private sealed class PropertyVisitor
        {
            private readonly Dictionary<string, EntityProperty> _result = new Dictionary<string, EntityProperty>();
            private readonly HashSet<object> _seen = new HashSet<object>();

            private readonly Dictionary<Type, PropertyInfo[]> _properties = new Dictionary<Type, PropertyInfo[]>();
            private readonly Dictionary<PropertyInfo, dynamic> _delegates = new Dictionary<PropertyInfo, dynamic>();
            private readonly Dictionary<(Type, TypeCode), MethodInfo> _genericVisitDelegates = new Dictionary<(Type, TypeCode), MethodInfo>();

            private readonly StringBuilder _keyBuilder = new StringBuilder();

            public void Visit(object obj)
            {
                if (obj is null) return;

                if (_seen.Contains(obj))
                {
                    throw new InvalidOperationException();
                }
                _seen.Add(obj);

                var type = obj.GetType();
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:

                        if (!_properties.TryGetValue(type, out var properties))
                        {
                            _properties[type] = properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        }

                        for (var i = 0; i < properties.Length; i++)
                        {
                            var property = properties[i];
                            switch (Type.GetTypeCode(property.PropertyType))
                            {
                                case TypeCode.Boolean:
                                    if (!_genericVisitDelegates.TryGetValue((property.DeclaringType, TypeCode.Boolean), out var method))
                                    {
                                        typeof(Action<object,>).MakeGenericType(property.DeclaringType)
                                    }

                                    VisitBooleanProperty(obj, property);
                                    break;
                            }
                        }

                        break;
                }
            }

            private void VisitBooleanProperty<T>(T obj, PropertyInfo property)
            {
                var @delegate = (Func<T, bool>)GetOrCreateDelegate(property);

                _keyBuilder.Append(':').Append(property.Name);
                _result.Add(_keyBuilder.ToString(), EntityProperty.GeneratePropertyForBool(@delegate(obj)));
                _keyBuilder.Length -= (property.Name.Length + 1);
            }

            private Delegate GetOrCreateDelegate(PropertyInfo property)
            {
                if (!_delegates.TryGetValue(property, out var @delegate))
                {
                    _delegates[property] = @delegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(new[] { property.DeclaringType, property.PropertyType }), property.GetGetMethod());
                }
                return @delegate;
            }
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default)
        {
            if (grainType is null) throw new ArgumentNullException(nameof(grainType));
            if (grainReference is null) throw new ArgumentNullException(nameof(grainReference));
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerWriteStateAsync(grainType, grainReference, state, cancellationToken);
        }

        private async Task InnerWriteStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default)
        {
            // flatten state properties
            var partitionKey = _options.IsGlobalRegistry ? _options.GlobalRegistryPartitionKey : GetPartitionKey(grainType, grainReference);
            var entity = new DynamicTableEntity(partitionKey, state.Key, state.ETag,

            var entity = new AzureTableRegistryEntity
            {
                PartitionKey = _options.IsGlobalRegistry ? _options.GlobalRegistryPartitionKey : GetPartitionKey(grainType, grainReference),
                RowKey = state.Key,
                Payload = _serializer.SerializeToByteArray(state.State),
                ETag = state.ETag,
                Timestamp = _clock.UtcNow,
            };

            var operation = TableOperation.InsertOrReplace(entity);

            try
            {
                var result = await _table.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);

                // this operation returns no-content when successful
                if (result.HttpStatusCode == (int)HttpStatusCode.NoContent)
                {
                    // write successful
                    state.ETag = result.Etag;
                    return;
                }
                else
                {
                    // write failed
                    throw new InconsistentStateException(Resources.Exception_InsertOrReplaceOperationFailedWithHttpStatusCode_X.Format(result.HttpStatusCode));
                }

                // todo: push the request charge to telemetry around here and maybe log it as debug
            }
            catch (Exception exception)
            {
                throw new InconsistentStateException(Resources.Exception_InsertOrReplaceOperationFailed, exception);
            }
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default)
        {
            if (grainType is null) throw new ArgumentNullException(nameof(grainType));
            if (grainReference is null) throw new ArgumentNullException(nameof(grainReference));
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerClearStateAsync(grainType, grainReference, state, cancellationToken)
        }

        private async Task InnerClearStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default)
        {
            var entity = new AzureTableRegistryEntity
            {
                PartitionKey = _options.IsGlobalRegistry ? _options.GlobalRegistryPartitionKey : GetPartitionKey(grainType, grainReference),
                RowKey = state.Key,
                ETag = state.ETag
            };

            var operation = TableOperation.Delete(entity);

            try
            {
                var result = await _table.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
                if (result.HttpStatusCode == (int)HttpStatusCode.NoContent)
                {
                    // clear successful
                    state.ETag = result.Etag;
                    return;
                }
                else
                {
                    // clear failed
                    throw new InconsistentStateException(Resources.Exception_ClearOperationFailedWithHttpStatusCode_X.Format(result.HttpStatusCode));
                }

                // todo: push the request charge to telemetry around here and maybe log it as debug
            }
            catch (Exception exception)
            {
                throw new InconsistentStateException(Resources.Exception_ClearOperationFailed, exception);
            }
        }

        public Task ReadStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state)
        {
            if (grainType is null) throw new ArgumentNullException(nameof(grainType));
            if (grainReference is null) throw new ArgumentNullException(nameof(grainReference));
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerReadStateAsync(grainType, grainReference, state);
        }

        private async Task InnerReadStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state)
        {
            var partitionKey = _options.IsGlobalRegistry ? _options.GlobalRegistryPartitionKey : GetPartitionKey(grainType, grainReference);
            var operation = TableOperation.Retrieve<AzureTableRegistryEntity>(partitionKey, state.Key);

            try
            {
                var result = await _table.ExecuteAsync(operation).ConfigureAwait(false);
                if (result.Result is AzureTableRegistryEntity entity)
                {
                    state.State = _serializer.Deserialize(state.Type, new BinaryTokenStreamReader(entity.Payload));
                    state.ETag = result.Etag;
                    return;
                }

                if (result.HttpStatusCode == (int)HttpStatusCode.OK)
                {
                    if (!(result.Result is AzureTableRegistryEntity entity))
                    {
                        throw new InconsistentStateException(Resources.Exception_AzureTableEntityIsNotOfTheExpectedType);
                    }

                    var reader = _readerPool.Get();
                    reader.Reset(
                    try
                    {
                        //_serializer.Deserialize(state.Type, new BinaryTokenStreamReader(result.Result))
                    }
                    finally
                    {
                        _readerPool.Return(reader);
                    }
                }
                else if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                throw new InconsistentStateException()
            }
        }

        public IQueryable<IKeyedGrainState> CreateQuery<TOutput>(string grainType, GrainReference grainReference, System.Func<IKeyedGrainState<TState>, TOutput> factory)
        {
            throw new System.NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _table.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(Resources.Log_StartedAzureTableRegistryGrainStorageNamed_X_UsingTable_X, _storageName, _table.Name);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}