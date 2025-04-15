using Factory.DB;
using System.Reflection;
using System.Diagnostics;

namespace Factory
{
    internal static class ReflectionFactory
    {
        internal const BindingFlags PublicInstanceBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        internal static IEnumerable<PropertyInfo> GetMappableProperties<T>(Type classType)
        {
            return classType.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(T)));
        }
        internal static IEnumerable<FieldInfo> GetMappableFields(Type t)
        {
            return t.GetTypeInfo().GetFields(PublicInstanceBindingFlags).Where(field => field.IsInitOnly == false);
        }

        internal static IEnumerable<PropertyInfo> GetMappableProperties(Type t)
        {
            return t.GetTypeInfo().GetProperties(PublicInstanceBindingFlags).Where(p => p.CanWrite);
            //return t.GetTypeInfo().GetMembers(PublicInstanceBindingFlags);
        }

        internal static IEnumerable<MemberInfo> GetMappableMember(Type t)
        {
            return t.GetTypeInfo().GetMembers(PublicInstanceBindingFlags);
        }

        internal static string GetTableAttribute(Type t)
        {
            var attribute = (SqlTableAttribute)Attribute.GetCustomAttribute(t, typeof(SqlTableAttribute));
            if (attribute != null) return attribute.PropertyName;
            else return String.Empty;
        }

        internal static string GetTypeName(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            bool isNullableType = nullableType != null;

            if (isNullableType)
                return nullableType.Name;
            else
                return type.Name;
        }

        internal static bool SetPropertyValue(object obj, string propertyName, object propertyValue)
        {
            try
            {
                if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                {
                    return false;
                }

                Type objectType = obj.GetType();

                PropertyInfo propertyDetail = objectType.GetProperty(propertyName);


                if (propertyDetail != null && propertyDetail.CanWrite)
                {
                    Type propertyType = propertyDetail.PropertyType;

                    Type dataType = propertyType;

                    // Check for nullable types
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        // Check for null or empty string value.
                        if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                        {
                            propertyDetail.SetValue(obj, null);
                            return false;
                        }
                        else
                        {
                            dataType = propertyType.GetGenericArguments()[0];
                        }
                    }

                    propertyValue = Convert.ChangeType(propertyValue, propertyType);

                    propertyDetail.SetValue(obj, propertyValue);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new Exception($"{funcName} : {ex.Message}");
            }
        }

        internal static bool SetPropertyValue(object targetObj, PropertyInfo prop, object propertyValue)
        {
            try
            {
                if (targetObj == null)
                {
                    return false;
                }

                if (prop != null && prop.CanWrite)
                {
                    Type propertyType = prop.PropertyType;

                    Type dataType = propertyType;

                    // Check for nullable types
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        // Check for null or empty string value.
                        if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                        {
                            prop.SetValue(targetObj, null);
                            return false;
                        }
                        else
                        {
                            dataType = propertyType.GetGenericArguments()[0];
                        }
                    }

                    propertyValue = Convert.ChangeType(propertyValue, propertyType);

                    prop.SetValue(targetObj, propertyValue);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new Exception($"{funcName} : {ex.Message}");
            }
        }
    }
}
