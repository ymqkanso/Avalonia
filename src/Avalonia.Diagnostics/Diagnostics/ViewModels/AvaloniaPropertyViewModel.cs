using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace Avalonia.Diagnostics.ViewModels
{
    internal class AvaloniaPropertyViewModel : PropertyViewModel
    {
        private readonly AvaloniaObject _target;
        private object _value;
        private string _priority;
        private TypeConverter _converter;
        private string _group;
        private DataGridCollectionView _bindingsView;

        public AvaloniaPropertyViewModel(AvaloniaObject o, AvaloniaProperty property)
        {
            _target = o;
            Property = property;

            Name = property.IsAttached ?
                $"[{property.OwnerType.Name}.{property.Name}]" :
                property.Name;

            if (property.IsDirect)
            {
                _group = "Properties";
                Priority = "Direct";
            }

            Update();
        }

        public AvaloniaProperty Property { get; }
        public override object Key => Property;
        public override string Name { get; }
        public bool IsAttached => Property.IsAttached;

        public string Priority
        {
            get => _priority;
            private set => RaiseAndSetIfChanged(ref _priority, value);
        }

        public override string Value
        {
            get
            {
                if (_value == null)
                {
                    return "(null)";
                }

                return Converter?.CanConvertTo(typeof(string)) == true ?
                    Converter.ConvertToString(_value) :
                    _value.ToString();
            }
            set
            {
                try
                {
                    var convertedValue = Converter?.CanConvertFrom(typeof(string)) == true ?
                        Converter.ConvertFromString(value) :
                        DefaultValueConverter.Instance.ConvertBack(value, Property.PropertyType, null, CultureInfo.CurrentCulture);
                    _target.SetValue(Property, convertedValue);
                }
                catch { }
            }
        }

        public override string Group
        {
            get => _group;
        }

        public DataGridCollectionView BindingsView
        {
            get => _bindingsView;
            private set => RaiseAndSetIfChanged(ref _bindingsView, value);
        }

        private TypeConverter Converter
        {
            get
            {
                if (_converter == null)
                {
                    _converter = TypeDescriptor.GetConverter(_value.GetType());
                }

                return _converter;
            }
        }
        
        public override void Update()
        {
            if (Property.IsDirect)
            {
                RaiseAndSetIfChanged(ref _value, _target.GetValue(Property), nameof(Value));
            }
            else
            {
                var val = _target.GetDiagnostic(Property);

                RaiseAndSetIfChanged(ref _value, val?.Value, nameof(Value));

                if (val != null)
                {
                    SetGroup(IsAttached ? "Attached Properties" : "Properties");
                    Priority = val.ValuePriority.ToString();

                    var bindings = val.Levels.SelectMany(x => x.Bindings, (level, binding) => (level, binding))
                        .Select(x => new BindingDetailsViewModel(x.binding, x.level, val.ValuePriority))
                        .ToList();

                    BindingsView = new DataGridCollectionView(bindings)
                    {
                        GroupDescriptions = { new DataGridPathGroupDescription(nameof(BindingDetailsViewModel.Priority)) }
                    };
                }
                else
                {
                    SetGroup(Priority = "Unset");
                    BindingsView = null;
                }
            }
        }

        private void SetGroup(string group)
        {
            RaiseAndSetIfChanged(ref _group, group, nameof(Group));
        }
    }
}
