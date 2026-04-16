using System;
using System.Globalization;

namespace Octokit.Reflection
{
    static class Constants
    {
        public const string Iso8601Format = @"yyyy-MM-dd\THH\:mm\:ss.fffzzz";
        public const string Iso8601FormatZ = @"yyyy-MM-dd\THH\:mm\:ss\Z";
        public static readonly string[] Iso8601Formats = {
            Iso8601Format,
            Iso8601FormatZ,
            @"yyyy-MM-dd\THH\:mm\:ss.fffffffzzz",
            @"yyyy-MM-dd\THH\:mm\:ss.ffffffzzz",
            @"yyyy-MM-dd\THH\:mm\:ss.fffffzzz",
            @"yyyy-MM-dd\THH\:mm\:ss.ffffzzz",
            @"yyyy-MM-dd\THH\:mm\:ss.ffzzz",
            @"yyyy-MM-dd\THH\:mm\:ss.fzzz",
            @"yyyy-MM-dd\THH\:mm\:sszzz",
            @"yyyy-MM-dd\THH\:mm\:ss.fffffff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.ffffff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.fffff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.ffff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.fff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.ff\Z",
            @"yyyy-MM-dd\THH\:mm\:ss.f\Z",
        };
    }

    [Serializable]
    internal class InstanceNotInitializedException : InvalidOperationException
    {
        public InstanceNotInitializedException(object the, string property) :
            base(String.Format(CultureInfo.InvariantCulture, "{0} is not correctly initialized, {1} is null", the?.GetType().Name, property)) { }

        protected InstanceNotInitializedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    internal static class Guard
    {
        public static void NotNull(object the, object value, string propertyName) {
            if(value != null) return;
            throw new InstanceNotInitializedException(the, propertyName);
        }

        public static void ArgumentNotNull(object value, string name) {
            if(value != null) return;
            string message = String.Format(CultureInfo.InvariantCulture, "Failed Null Check on '{0}'", name);
            throw new ArgumentNullException(name, message);
        }
    }
}