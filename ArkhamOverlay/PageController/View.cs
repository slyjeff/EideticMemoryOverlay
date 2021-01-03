using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PageController {
    public class View : UserControl {
        public static DependencyProperty DefaultFocusProperty = DependencyProperty.Register("DefaultFocus",
                                                                                    typeof(Control),
                                                                                    typeof(View),
                                                                                    new PropertyMetadata(null));

        public Control DefaultFocus {
            get { return (Control) GetValue(DefaultFocusProperty); }
            set { SetValue(DefaultFocusProperty, value); }
        }

        public static DependencyProperty DefaultFocusNameProperty = DependencyProperty.Register("DefaultFocusName",
                                                                                    typeof(string),
                                                                                    typeof(View),
                                                                                    new PropertyMetadata(null));

        public string DefaultFocusName {
            get { return (string) GetValue(DefaultFocusNameProperty); }
            set { SetValue(DefaultFocusNameProperty, value); }
        }

        public void ResetFocus() {
            var control = FindDefaultFocusControl();
            if (control != null) {
                Focus(control);
            }
        }

        protected void Focus(IInputElement control) {
            control.Focus();
            FocusManager.SetFocusedElement(this, control);
        }

        protected IInputElement FindDefaultFocusControl() {
            return DefaultFocus ?? FindName(DefaultFocusName) as IInputElement;
        }
    }
}
