using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using RapidPliant.WPF.Controls;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.WPF.Binding
{
    public class BindTo : BindingMarkupExtensionBase
    {
        public BindTo()
        {
        }

        public BindTo(string path)
        {
            Path = new PropertyPath(path);
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            RapidBindingDelegateBase bindingDelegateBase = null;

            FrameworkElement targetFrameworkElement;
            DependencyProperty targetDepProp;
            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepProp))
            {
                //This is a normal property binding... so just return that!
                bindingDelegateBase = new RapidBindingPropertyDelegate(this, targetFrameworkElement, targetDepProp);
            }

            MethodInfo targetDepPropMethod;
            EventInfo targetDepPropEvent;
            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepPropEvent))
            {
                bindingDelegateBase = new RapidBindingEventDelegate(this, targetFrameworkElement, targetDepPropEvent);
            }

            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepPropMethod))
            {
                bindingDelegateBase = new RapidBindingMethodActionDelegate(this, targetFrameworkElement, targetDepPropMethod);
            }

            if (bindingDelegateBase != null)
            {
                bindingDelegateBase.EnsureSerialization();

                return bindingDelegateBase.ProvideValue(provider);
            }

            throw new Exception("Not supported property type for RapidBinding!");
        }

        public string ToSerializationString(RapidBindingDelegateBase bindingDelegate)
        {
            var sb = new StringBuilder();
            sb.Append("{BindTo");

            if (Binding.Path != null)
            {
                sb.AppendFormat(" Path={0}", Binding.Path.Path);
            }

            sb.Append("}");
            return sb.ToString();
        }
    }

    public abstract class RapidBindingDelegateBase
    {
        public RapidBindingDelegateBase(BindTo binding, FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
            Binding = binding;
        }

        public FrameworkElement FrameworkElement { get; protected set; }

        public BindTo Binding { get; protected set; }

        public string BoundMemberName { get; protected set; }

        public virtual void EnsureSerialization()
        {
            RapidBindingSerializationProperty.SetRapidBinding(FrameworkElement, BoundMemberName + "=\"" + Binding.ToSerializationString(this) + "\"");
        }

        public abstract object ProvideValue(IServiceProvider provider);
    }

    public abstract class RapidBindingDelegate : RapidBindingDelegateBase
    {
        public RapidBindingDelegate(BindTo binding, Type delegateType, FrameworkElement frameworkElement)
            : base(binding, frameworkElement)
        {
            DelegateType = delegateType;
            EventArgsType = DelegateType.GetMethod("Invoke").GetParameters()[1].ParameterType;
            Delegate = Delegate.CreateDelegate(delegateType, this, "OnEvent");
        }

        public Type EventArgsType { get; protected set; }

        public Type DelegateType { get; protected set; }

        public Delegate Delegate { get; protected set; }

        protected virtual void OnEvent(object sender, RoutedEventArgs args)
        {
            var frameworkElem = sender as FrameworkElement;
            CallMethodForPath(frameworkElem);
        }

        private void CallMethodForPath(FrameworkElement frameworkElement)
        {
            new ActionMethodWithPath(frameworkElement, Binding.Path.Path).Invoke();
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            return Delegate;
        }
    }

    public class RapidBindingMethodActionDelegate : RapidBindingDelegate
    {
        public RapidBindingMethodActionDelegate(BindTo rapidBinding, FrameworkElement frameworkElement, MethodInfo methodInfo)
            : base(rapidBinding, methodInfo.GetParameters()[1].ParameterType, frameworkElement)
        {
            MethodInfo = methodInfo;
            BoundMemberName = MethodInfo.Name;
        }

        public MethodInfo MethodInfo { get; protected set; }
    }

    public class RapidBindingEventDelegate : RapidBindingDelegate
    {
        public RapidBindingEventDelegate(BindTo rapidBinding, FrameworkElement frameworkElement, EventInfo eventInfo)
            : base(rapidBinding, eventInfo.EventHandlerType, frameworkElement)
        {
            EventInfo = eventInfo;
            BoundMemberName = EventInfo.Name;
        }

        public EventInfo EventInfo { get; protected set; }
    }

    public class RapidBindingPropertyDelegate : RapidBindingDelegateBase
    {
        public RapidBindingPropertyDelegate(BindTo binding, FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
            : base(binding, frameworkElement)
        {
            DependencyProperty = dependencyProperty;
            BoundMemberName = DependencyProperty.Name;
        }

        public DependencyProperty DependencyProperty { get; protected set; }

        public override object ProvideValue(IServiceProvider provider)
        {
            return Binding.Binding.ProvideValue(provider);
        }
    }

    public class PathIterator
    {
        private int _index;

        public PathIterator(string path)
        {
            Parts = ParsePathParts(path);
            _index = 0;
        }

        private PathPart[] ParsePathParts(string path)
        {
            var pathParts = new List<PathPart>();

            var parts = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (var i = 0; i < parts.Length; ++i)
            {
                var pathPart = new PathPart();
                var part = parts[i].Trim();
                
                if (!ParseMethodWithArgs(pathPart, part))
                {
                    pathPart.Name = part;
                }

                pathParts.Add(pathPart);
            }

            return pathParts.ToArray();
        }

        private bool ParseMethodWithArgs(PathPart pathPart, string part)
        {
            var i = part.IndexOf("(");
            if (i == -1)
                return false;

            pathPart.Name = part.Substring(0, i);

            var remainder = part.Substring(i).Trim('(', ')').Trim();
            var argParts = remainder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var argPart in argParts)
            {
                var argPath = new PathPart();
                argPath.Name = argPart.Trim();
                pathPart.AddSubPath(argPath);
            }

            return true;
        }

        public PathPart[] Parts { get; set; }

        public PathPart Current { get; private set; }

        public bool MoveNext()
        {
            if (_index < Parts.Length)
            {
                Current = Parts[_index++];
                return true;
            }
            return false;
        }
    }

    public class PathPart
    {
        public PathPart()
        {
            SubParts = new List<PathPart>();
        }

        public List<PathPart> SubParts { get; set; }

        public string Name { get; set; }

        public void AddSubPath(PathPart pathPart)
        {
            if(pathPart.Name != null)
                SubParts.Add(pathPart);
        }
    }

    public class ActionMethodWithPath
    {
        public ActionMethodWithPath(FrameworkElement frameworkElem, string path)
        {
            var pathIter = new PathIterator(path);

            object rootDataContext;
            object thisDataContext;
            var target = FindDataContexts(frameworkElem, pathIter, out rootDataContext, out thisDataContext);

            EvalActionMethod(rootDataContext, thisDataContext, target, pathIter);
        }
        
        public string Path { get; private set; }

        public object Root { get; private set; }

        public object Target { get; private set; }

        public MethodInfo TargetMethod { get; private set; }

        public object[] TargetMethodArgs { get; private set; }

        private object FindDataContexts(FrameworkElement frameworkElem, PathIterator path, out object rootDataContext, out object thisDataContext)
        {
            rootDataContext = null;
            thisDataContext = null;
            object target = null;

            var parentWithDataContext = frameworkElem.FindParentWithDataContext();
            if (parentWithDataContext != null && parentWithDataContext.DataContext != null)
            {
                thisDataContext = parentWithDataContext.DataContext;
                rootDataContext = thisDataContext;
                target = thisDataContext;
            }

            if (path.MoveNext())
            {
                if (path.Current.Name == "root")
                {
                    var view = frameworkElem.FindParent<RapidView>();
                    rootDataContext = view.ViewModel;
                    target = rootDataContext;
                }
            }

            return target;
        }

        private void EvalActionMethod(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            Root = rootDataContext;
            Target = thisDataContext;
            
            PathPart lastPart = pathIter.Current;
            while(pathIter.MoveNext())
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
