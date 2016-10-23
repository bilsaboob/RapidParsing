using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace RapidPliant.WPF.Binding
{
    public class MemberWithPath
    {
    }

    public class PropertyWithPath : MemberWithPath
    {
        public PropertyWithPath()
        {
        }

        public object GetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter);
            if (targetProperty == null)
                return null;
            
            var value = targetProperty.GetValue(target);
            return value;
        }

        public object SetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter, object value)
        {
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter);
            if (targetProperty == null)
                return null;

            targetProperty.SetValue(target, value);
            return value;
        }

        private PropertyInfo ResolveTargetProperty(object rootDataContext, object thisDataContext, ref object target, PathIterator pathIter)
        {
            if (!pathIter.IsStarted && !pathIter.MoveNext())
            {
                return null;
            }

            if (pathIter.Current.Name == "root")
            {
                target = rootDataContext;
            }
            else if (pathIter.Current.Name == "this")
            {
                target = thisDataContext;
            }

            var lastMemberPart = ResolvePath(ref target, pathIter);
            if (lastMemberPart == null)
                return null;

            return target.GetType().GetProperty(lastMemberPart.Name);
        }

        protected PathPart ResolvePath(ref object target, PathIterator pathIter)
        {
            PathPart lastPart = pathIter.Current;
            while (pathIter.MoveNext())
            {
                var part = pathIter.Current;
                lastPart = part;

                if(pathIter.IsLast)
                    break;

                var memberName = part.Name;
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    break;
                
                target = prop.GetValue(target);
                if (target == null)
                    break;
            }

            return lastPart;
        }
    }

    public class ActionMethodWithPath : MemberWithPath
    {
        public ActionMethodWithPath(FrameworkElement frameworkElem, string path)
        {
            var pathIter = new PathIterator(path);
            object rootDataContext;
            object thisDataContext;
            var target = RapidBindingHelpers.FindDataContexts(frameworkElem, pathIter, out rootDataContext, out thisDataContext);

            EvalActionMethod(rootDataContext, thisDataContext, target, pathIter);
        }
        
        public string Path { get; private set; }

        public object Root { get; private set; }

        public object Target { get; private set; }

        public MethodInfo TargetMethod { get; private set; }

        public object[] TargetMethodArgs { get; private set; }

        protected PathPart ResolvePath(ref object target, PathIterator pathIter)
        {
            PathPart lastPart = pathIter.Current;
            while (pathIter.MoveNext())
            {
                var part = pathIter.Current;
                lastPart = part;

                var memberName = part.Name;
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    break;

                target = prop.GetValue(target);
                if (target == null)
                    break;
            }

            return lastPart;
        }

        private void EvalActionMethod(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            Root = rootDataContext;
            Target = thisDataContext;

            //Get the last part
            var lastPart = ResolvePath(ref target, pathIter);

            //Update the traget
            Target = target;

            if (lastPart != null)
            {
                TargetMethod = target.GetType().GetMethod(lastPart.Name);
                EvalActionMethodArgs(rootDataContext, thisDataContext, target, lastPart, TargetMethod);
            }
        }

        private void EvalActionMethodArgs(object rootDataContext, object thisDataContext, object lastPartDataContext, PathPart lastPart, MethodInfo methodInfo)
        {
            var args = new List<object>();

            var methodArgs = methodInfo.GetParameters();
            var argParts = lastPart.SubParts;

            var len = Math.Min(methodArgs.Length, argParts.Count);
            var i = 0;
            for (i = 0; i < len; ++i)
            {
                if(i >= methodArgs.Length)
                    break;
                
                if(i >= argParts.Count)
                    break;

                var methodArg = methodArgs[i];
                var argPart = argParts[i];

                if (argPart.Name == "this")
                {
                    args.Add(thisDataContext);
                }
                else if (argPart.Name == "root")
                {
                    args.Add(rootDataContext);
                }
                else
                {
                    args.Add(GetValue(methodArg.ParameterType, argPart.Name));
                }
            }

            for (int j = i; j < methodArgs.Length; j++)
            {
                var methodArg = methodArgs[i];
                args.Add(GetDefault(methodArg.ParameterType));
            }

            TargetMethodArgs = args.ToArray();
        }

        private object GetValue(Type valueType, string strVal)
        {
            if (valueType == typeof(string))
                return strVal;

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(strVal);
            }

            converter = TypeDescriptor.GetConverter(typeof(string));
            if (converter.CanConvertTo(valueType))
            {
                return converter.ConvertTo(strVal, valueType);
            }

            return GetDefault(valueType);
        }

        public object GetDefault(Type t)
        {
            return this.GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
        }

        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public void Invoke()
        {
            if (TargetMethod == null)
                return;

            TargetMethod.Invoke(Target, TargetMethodArgs);
        }
    }
}