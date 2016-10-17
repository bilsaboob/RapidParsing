using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using RapidPliant.App.Controls;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.Binding
{
    public class BindToExtension : MarkupExtension
    {
        private Type _eventArgsType;
        private DependencyPropertyListener _listener;

        public BindToExtension()
        {
        }

        public BindToExtension(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            //For binding to "method" => RoutedEventHandler
            var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (pvt == null)
                throw new Exception("No target available!");

            var target = pvt.TargetObject;
            var targetProp = pvt.TargetProperty;
            var targetPropType = targetProp.GetType();

            if (typeof(EventInfo).IsAssignableFrom(targetPropType))
            {
                return ProvideEventValue(target, targetProp);
            }
            else if (typeof(MethodInfo).IsAssignableFrom(targetPropType))
            {
                return ProvideMethodValue(target, targetProp);
            }
            else if (typeof(DependencyProperty).IsAssignableFrom(targetPropType))
            {
                return ProvideDependencyPropertyValue(target, targetProp);
            }

            throw new Exception("Invalid target!");
        }

        #region Property
        private object ProvideDependencyPropertyValue(object target, object targetProp)
        {
            var depProp = targetProp as DependencyProperty;
            if (depProp == null)
                return null;

            var depObj = target as DependencyObject;
            if (depObj == null)
                return null;

            depObj.OnPropertyChange(depProp, (args) => {
                args.BindingExpression.UpdateTarget();
            });

            return depObj.GetValue(depProp);

            /*var dpd = DependencyPropertyDescriptor.FromProperty(depProp, depObj.GetType());
            if (dpd != null)
            {
                dpd.AddValueChanged(depObj, (source, args) => {
                    OnControlPropertyValueChanged(depObj, dpd);
                });
            }

            BindMvxContext(depObj);
            
            /*ownerViewModel.PropertyChanged += (source, args) => {
                OnViewModelPropertyValueChanged(depObj, propertyMember, dpd);
            };*/

            //return depObj.GetValue(depProp);
        }

        private void BindMvxContext(DependencyObject depObj)
        {
            //Ensure that we have an mvx context or that whenever it's set, we then evaluate and get the viewmodel from there!
        }

        private void OnControlPropertyValueChanged(object depObj, DependencyPropertyDescriptor dpd)
        {
            var val = dpd.GetValue(depObj);
            //propMember.SetValue(val);
        }

        private void OnViewModelPropertyValueChanged(object depObj, DependencyPropertyDescriptor dpd)
        {
            /*var propertyMember = GetPropertyMember(MemberPath.Parse(Path));
            var ownerViewModel = propertyMember.Obj as ViewModel;

            var propValue = propMember.GetValue();
            dpd.SetValue(depObj, propValue);*/
        }
        #endregion

        #region Method
        private object ProvideMethodValue(object target, object targetProp)
        {
            var mi = targetProp as MethodInfo;
            if (mi == null)
                return null;

            var delegateType = mi.GetParameters()[1].ParameterType;

            _eventArgsType = delegateType.GetMethod("Invoke").GetParameters()[1].ParameterType;
            var callbackAction = GetType().GetMethod("OnMethodInfo", BindingFlags.NonPublic | BindingFlags.Instance);

            return Delegate.CreateDelegate(delegateType, this, callbackAction);
        }

        private void OnMethodInfo(object sender, object args)
        {
        }
        #endregion

        #region Event
        private object ProvideEventValue(object target, object targetProp)
        {
            var ei = targetProp as EventInfo;
            if (ei == null)
                return null;

            var delegateType = ei.EventHandlerType;

            _eventArgsType = delegateType.GetMethod("Invoke").GetParameters()[1].ParameterType;
            var callbackAction = GetType().GetMethod("OnEventInfo", BindingFlags.NonPublic | BindingFlags.Instance);

            return Delegate.CreateDelegate(delegateType, this, callbackAction);
        }

        private void OnEventInfo(object sender, object args)
        {
        }
        #endregion
    }

    public class MemberPath
    {
        private MemberPath(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public MemberPath Prev { get; private set; }
        public MemberPath Next { get; private set; }

        public static MemberPath Parse(string path)
        {
            var parts = path.Split('.');
            MemberPath first = null;
            MemberPath prev = null;
            MemberPath current = null;
            foreach (var part in parts)
            {
                current = new MemberPath(part);
                current.Prev = prev;
                if (prev != null)
                {
                    prev.Next = current;
                }
                if (first == null)
                {
                    first = current;
                }
            }
            return first;
        }
    }

    public sealed class BindingActionExtension : MarkupExtension
    {
        Type _eventArgsType;

        public BindingActionExtension()
        {
        }

        public override object ProvideValue(IServiceProvider sp)
        {
            var pvt = sp.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (pvt != null)
            {
                var evt = pvt.TargetProperty as EventInfo;
                var doAction = GetType().GetMethod("DoAction", BindingFlags.NonPublic | BindingFlags.Instance);
                Type dlgType = null;
                if (evt != null)
                {
                    dlgType = evt.EventHandlerType;
                }
                var mi = pvt.TargetProperty as MethodInfo;
                if (mi != null)
                {
                    dlgType = mi.GetParameters()[1].ParameterType;
                }
                if (dlgType != null)
                {
                    _eventArgsType = dlgType.GetMethod("Invoke").GetParameters()[1].ParameterType;
                    return Delegate.CreateDelegate(dlgType, this, doAction);
                }
            }
            return null;
        }

        public BindingActionExtension(string bindingCommandPath)
        {
            BindingCommandPath = bindingCommandPath;
        }

        void DoAction(object sender, RoutedEventArgs e)
        {
            var dc = (sender as FrameworkElement).DataContext;

            if (BindingCommandPath != null)
            {
                try
                {
                    Method = GetActionMethodForPath(dc, BindingCommandPath);
                }
                catch (Exception)
                {
                    //Doesn't matter
                    Method = null;
                }
            }

            if (Method != null)
            {
                //Invoke without args!
                Method.Invoke(dc, null);
            }
        }

        public string BindingCommandPath { get; set; }

        public MethodInfo Method { get; set; }

        static MethodInfo GetActionMethodForPath(object target, string path)
        {
            var memberNames = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (memberNames.Length == 0)
                return null;

            var methodMember = memberNames[memberNames.Length - 1];
            for (var i = 0; i < memberNames.Length - 1; ++i)
            {
                var memberName = memberNames[i];
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    return null;

                target = prop.GetValue(target);
                if (target == null)
                    return null;
            }

            return target.GetType().GetMethod(methodMember);
        }

    }
}
