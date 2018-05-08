﻿using Microsoft.Azure.WebJobs.Host.Config;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Indigo.Functions.Redis
{
    public class RedisExtension : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<RedisAttribute>();

            rule.WhenIsNull(nameof(RedisAttribute.Configuration))
                .BindToInput(ThrowValidationError);

            rule.WhenIsNotNull(nameof(RedisAttribute.Configuration))
                .BindToInput(BuildConnectionFromAttribute);
            rule.WhenIsNotNull(nameof(RedisAttribute.Configuration))
                .BindToInput(BuildConnectionFromAttributeAsync);

            // string input
            rule.WhenIsNotNull(nameof(RedisAttribute.Key))
                .BindToInput(GetStringValueFromAttributeAsync);
            rule.WhenIsNotNull(nameof(RedisAttribute.Key))
                .BindToInput(GetStringValueFromAttribute);

            // generic output
            rule.WhenIsNotNull(nameof(RedisAttribute.Key))
                .BindToCollector(attribute => new RedisAsyncCollector(attribute));

            // generic converters
            rule.AddOpenConverter<PocoOpenType, string>(typeof(StringConverter<>));
            rule.AddOpenConverter<string, PocoOpenType>(typeof(PocoConverter<>));
        }

        private static IConnectionMultiplexer ThrowValidationError(RedisAttribute attribute)
        {
            throw new ArgumentException("RedisAttribute.Configuration parameter cannot be null", nameof(attribute));
        }

        private static IConnectionMultiplexer BuildConnectionFromAttribute(RedisAttribute attribute)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(attribute.Configuration);

            return connectionMultiplexer;
        }

        private static async Task<IConnectionMultiplexer> BuildConnectionFromAttributeAsync(RedisAttribute attribute)
        {
            var connectionMultiplexer = await ConnectionMultiplexer
                .ConnectAsync(attribute.Configuration)
                .ConfigureAwait(false);

            return connectionMultiplexer;
        }

        private static async Task<string> GetStringValueFromAttributeAsync(RedisAttribute attribute)
        {
            var connectionMultiplexer = await BuildConnectionFromAttributeAsync(attribute).ConfigureAwait(false);
            return await connectionMultiplexer.GetDatabase().StringGetAsync(attribute.Key).ConfigureAwait(false);
        }

        private static string GetStringValueFromAttribute(RedisAttribute attribute)
        {
            var connectionMultiplexer = BuildConnectionFromAttribute(attribute);
            return connectionMultiplexer.GetDatabase().StringGet(attribute.Key);
        }
    }
}
