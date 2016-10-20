using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RapidPliant.WPF.Utils
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TProperty>(this object source, Expression<Func<TProperty>> propertyLambda)
        {
            var type = source.GetType();

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.",propertyLambda.ToString()));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.",propertyLambda.ToString()));

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format("Expresion '{0}' refers to a property that is not from type {1}.",propertyLambda.ToString(),type));

            return propInfo;
        }

        public static int GetTypeDistanceTo(this Type type, Type toType)
        {
            var minDistance = GetTypeDistanceTo_(type, toType, 0);
            return minDistance;
        }

        private static int GetTypeDistanceTo_(Type type, Type toType, int distance)
        {
            if (type == toType)
                return distance;

            var hasNewMinistance = false;
            var minSubDistance = int.MaxValue;
            
            //Check the base type!
            if (type.BaseType != null && !type.BaseType.IsPrimitive && type.BaseType != typeof(object))
            {
                var baseTypeDistance = GetTypeDistanceTo_(type.BaseType, toType, distance + 1);
                if (minSubDistance > baseTypeDistance)
                {
                    hasNewMinistance = true;
                    minSubDistance = baseTypeDistance;
                }
            }
            
            if (hasNewMinistance)
                return minSubDistance;
            
            //If it's an interface type we are looking for, we can also check the interfaces!
            if (toType.IsInterface)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces != null && interfaces.Length > 0)
                {
                    foreach (var interfaceType in interfaces)
                    {
                        var interfaceTypeDistance = GetTypeDistanceTo_(interfaceType, toType, distance +1);
                        if (minSubDistance > interfaceTypeDistance)
                        {
                            hasNewMinistance = true;
                            minSubDistance = interfaceTypeDistance;
                        }
                    }
                }
            }

            return minSubDistance;
        }
    }
}
