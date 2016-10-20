using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RapidPliant.App.Controls;

namespace RapidPliant.App.Binding
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
            var parentWithDataContext = frameworkElem.FindParentWithDataContext();
            if (parentWithDataContext != null && parentWithDataContext.DataContext != null)
            {
                var dataContext = parentWithDataContext.DataContext;
                CallMethodForPath(dataContext);
            }
        }

        private void CallMethodForPath(object dataContext)
        {
            new ActionMethodWithPath(dataContext, Binding.Path.Path).Invoke();
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

    public class ActionMethodWithPath
    {
        public ActionMethodWithPath(object target, string path)
        {
            EvalActionMethod(target, path);
        }

        public string Path { get; private set; }

        public object Root { get; private set; }

        public object Target { get; private set; }

        public MethodInfo TargetMethod { get; private set; }

        private void EvalActionMethod(object target, string path)
        {
            Root = target;
            Target = Root;

            var memberNames = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (memberNames.Length == 0)
                return;

            var methodMember = memberNames[memberNames.Length - 1];
            for (var i = 0; i < memberNames.Length - 1; ++i)
            {
                var memberName = memberNames[i];
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    return;

                target = prop.GetValue(target);
                if (target == null)
                    return;
            }

            Target = target;
            TargetMethod = target.GetType().GetMethod(methodMember);
        }

        public void Invoke()
        {
            if (TargetMethod == null)
                return;

            TargetMethod.Invoke(Target, null);
        }
    }
}
