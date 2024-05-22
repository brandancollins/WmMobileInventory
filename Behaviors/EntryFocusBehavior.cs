using Microsoft.Maui.Controls;

namespace WmMobileInventory.Behaviors
{
    public class EntryFocusBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty IsFocusedProperty = BindableProperty.Create(
            propertyName: nameof(IsFocused),
            returnType: typeof(bool),
            declaringType: typeof(EntryFocusBehavior),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnIsFocusedChanged);

        public bool IsFocused
        {
            get => (bool)GetValue(IsFocusedProperty);
            set => SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = bindable as EntryFocusBehavior;
            var entry = behavior?.AssociatedObject;
            if (entry != null && (bool)newValue)
            {
                entry.Focus();
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            }
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject = bindable;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            AssociatedObject = null;
        }

        public Entry AssociatedObject { get; private set; }
    }
}
