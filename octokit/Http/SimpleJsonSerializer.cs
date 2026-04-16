using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Octokit.Reflection;

namespace Octokit.Internal
{
    public class SimpleJsonSerializer : IJsonSerializer
    {
        private readonly NewtonsoftJsonSerializer _newtonsoftJsonSerializer = new NewtonsoftJsonSerializer();

        public string Serialize(object item)
        {
            return _newtonsoftJsonSerializer.Serialize(item);
        }

        public T Deserialize<T>(string json)
        {
            return _newtonsoftJsonSerializer.Deserialize<T>(json);
        }

        internal static string SerializeEnum(Enum value)
        {
            return NewtonsoftJsonSerializer.SerializeEnum(value);
        }

        internal static object DeserializeEnum(string value, Type type)
        {
            return NewtonsoftJsonSerializer.DeserializeEnum(value, type);
        }
    }
}
