using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Serialization;

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

        public BindTo CloneForBindingExpressionMarkupSerializationObject()
        {
            var b = new BindTo();

            b.Path = ActualPath;
            b.Converter = ActualConverter;
            b.ConverterCulture = ConverterCulture;
            b.ConverterParameter = ConverterParameter;
            b.Mode = Mode;
            b.ElementName = ElementName;
            b.BindsDirectlyToSource = BindsDirectlyToSource;
            b.NotifyOnSourceUpdated = NotifyOnSourceUpdated;
            b.NotifyOnTargetUpdated = NotifyOnTargetUpdated;
            b.NotifyOnValidationError = NotifyOnValidationError;
            b.IsAsync = IsAsync;

            return b;
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
        private RapidBindingPropertyValueProviderConverter _converter;

        public RapidBindingPropertyDelegate(BindTo binding, FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
            : base(binding, frameworkElement)
        {
            DependencyProperty = dependencyProperty;
            BoundMemberName = DependencyProperty.Name;

            SetupBinding();
        }

        public DependencyProperty DependencyProperty { get; protected set; }

        private void SetupBinding()
        {
            _converter = new RapidBindingPropertyValueProviderConverter(this);
        }
        
        public override object ProvideValue(IServiceProvider provider)
        {
            _converter.SetupBinding();
            return _converter.ProvideValue(provider);
        }

        public override void EnsureSerialization()
        {
            //Don't serialize... we use the bindingexpression converter!
        }
    }

    public class PathIterator
    {
        private int _index;
        private bool _started;
        
        public PathIterator(string path)
        {
            Parts = ParsePathParts(path);
            _index = 0;
        }

        public PathIterator(IEnumerable<PathPart> parts)
        {
            Parts = parts.ToArray();
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
        public bool IsLast { get { return _index >= Parts.Length; } }

        public bool IsStarted { get { return _started; } }

        public bool MoveNext()
        {
            _started = true;
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
}
