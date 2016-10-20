using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using RapidPliant.WPF.Utils;

namespace RapidPliant.WPF.Mvx
{
    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyEntry _nullPropertyEntry = new NullPropertyEntry();
        private Dictionary<string, PropertyEntry> _propertyEntries;

        public ViewModel()
        {
            _propertyEntries = new Dictionary<string, PropertyEntry>();
        }

        public MvxContext Context { get; private set; }
        
        public bool IsLoaded { get; protected set; }

        protected bool HasView { get; private set; }
        protected RapidView View { get; private set; }

        public void Load()
        {
            IsLoaded = true;
            LoadData();
        }

        public virtual void LoadViewModels()
        {
        }

        protected virtual void LoadData()
        {
        }

        public virtual void Unload()
        {
        }

        public void BindContext(MvxContext context)
        {
            if(Context == context)
                return;

            Context = context;
            RapidView view = null;

            if (Context != null)
            {
                view = Context.View;
            }

            BindView(view);
        }

        public void BindView(RapidView view)
        {
            View = view;
            HasView = view != null;
        }

        protected void set<TValue>(Expression<Func<TValue>> memberExpression, TValue value)
        {
            var prop = GetOrCreateProperyEntry(memberExpression);
            prop.SetValueAndNotify(this, value);
        }
        
        protected TValue get<TValue>(Expression<Func<TValue>> memberExpression)
        {
            var prop = GetOrCreateProperyEntry(memberExpression);
            return prop.GetValue<TValue>();
        }

        private PropertyEntry GetOrCreateProperyEntry<TValue>(Expression<Func<TValue>> memberExpression)
        {
            var propInfo = this.GetPropertyInfo(memberExpression);
            if (propInfo == null)
                return _nullPropertyEntry;

            PropertyEntry propEntry;
            if (!_propertyEntries.TryGetValue(propInfo.Name, out propEntry))
            {
                propEntry = new PropertyEntry(propInfo);
                _propertyEntries[propInfo.Name] = propEntry;
            }
            return propEntry;
        }

        protected T GetOrCreate<T>()
        {
            return default(T);
        }

        class PropertyEntry
        {
            public PropertyEntry(PropertyInfo propInfo)
            {
                PropertyInfo = propInfo;
            }

            public PropertyInfo PropertyInfo { get; set; }

            public virtual string Name { get { return PropertyInfo.Name; } }
            public virtual object Value { get; protected set; }

            public virtual void SetValueAndNotify(ViewModel source, object value)
            {
                if(source == null)
                    return;

                Value = value;

                source.OnPropertyChanged(Name);
            }

            public virtual TValue GetValue<TValue>()
            {
                if (Value == null)
                {
                    return default(TValue);
                }

                return (TValue)Value;
            }
        }

        class NullPropertyEntry : PropertyEntry
        {
            public NullPropertyEntry() : base(null)
            {
            }

            public override string Name { get { return null; } }

            public override object Value { get { return null; } protected set {} }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NullViewModel : ViewModel
    {
    }
}