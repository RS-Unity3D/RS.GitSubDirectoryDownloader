using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Octokit.Reflection
{
    internal static class ReflectionUtils
    {
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsTypeGenericeCollectionInterface(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>);
        }

        public static bool IsStringEnumWrapper(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(StringEnum<>);
        }

        public static Type GetGenericListElementType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        public static bool IsAssignableFrom(Type baseType, Type type)
        {
            return baseType.IsAssignableFrom(type);
        }

        public delegate object GetDelegate(object source);

        public delegate void SetDelegate(object source, object value);

        public static GetDelegate GetGetDelegate(PropertyInfo property)
        {
            return source => property.GetValue(source, null);
        }

        public static SetDelegate GetSetDelegate(PropertyInfo property)
        {
            return (source, value) => property.SetValue(source, value, null);
        }

        public static GetDelegate GetGetDelegate(FieldInfo field)
        {
            return source => field.GetValue(source);
        }

        public static SetDelegate GetSetDelegate(FieldInfo field)
        {
            return (source, value) => field.SetValue(source, value);
        }

        public static MethodInfo GetGetterMethodInfo(PropertyInfo property)
        {
            return property.GetMethod;
        }

        public static MethodInfo GetSetterMethodInfo(PropertyInfo property)
        {
            return property.SetMethod;
        }

        public static GetDelegate GetGetMethod(PropertyInfo property)
        {
            return source => property.GetValue(source, null);
        }

        public static SetDelegate GetSetMethod(PropertyInfo property)
        {
            return (source, value) => property.SetValue(source, value, null);
        }

        public static GetDelegate GetGetMethod(FieldInfo field)
        {
            return source => field.GetValue(source);
        }

        public static SetDelegate GetSetMethod(FieldInfo field)
        {
            return (source, value) => field.SetValue(source, value);
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetRuntimeProperties();
        }

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            return type.GetRuntimeFields();
        }

        public static object GetAttribute(MemberInfo member, Type attributeType)
        {
            return member.GetCustomAttribute(attributeType);
        }

        public static TypeInfo GetTypeInfo(Type type)
        {
            return type.GetTypeInfo();
        }
    }
}