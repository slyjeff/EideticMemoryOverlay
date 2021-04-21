using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PageController {
    public abstract class Controller {
        public object View { get; protected set; }
        public object ViewModel { get; protected set; }
    }

    public abstract class Controller<TView, TViewModel> : Controller {
        #region Create Controller/View/ViewModel

        protected Controller() {
            if (PageControllerConfiguration.ResolveViews) {
                View = PageControllerConfiguration.PageDependencyResolver.Resolve<TView>();
            }
            ViewModel = CreateViewModel();

            if (PageControllerConfiguration.ResolveViews) {
                var dataContexProperty = View.GetType().GetProperty("DataContext");
                if (dataContexProperty != null) {
                    dataContexProperty.SetValue(View, ViewModel, new object[] { });
                }
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var query = from method in GetType().GetMethods()
                        where method.GetCustomAttributes(typeof(PropertyChangedAttribute), true).Any()
                        where e.PropertyName + "Changed" == method.Name
                        select method;

            var propertyChangedMethod = query.FirstOrDefault();
            if (propertyChangedMethod == null) {
                return;
            }

            propertyChangedMethod.Invoke(this, new object[] { });
        }

        private TViewModel CreateViewModel() {
            var viewModelProxyType = DynamicViewModelManager.AddCommandsToViewModelClass(typeof(TViewModel), GetType());

            //ninject seems to bomb calling our inherited constructor, but we can do it manually
            var constructor = viewModelProxyType.GetConstructors()[0];
            var viewModel = (TViewModel)Activator.CreateInstance(viewModelProxyType, constructor.GetParameters().Select(parameter => PageControllerConfiguration.PageDependencyResolver.Resolve(parameter.ParameterType)).ToArray());

            var query = from method in GetType().GetMethods()
                        where method.GetCustomAttributes(typeof(CommandAttribute), true).Any()
                        select method;

            foreach (var method in query.ToList()) {
                var property = viewModelProxyType.GetProperty(method.Name);
                property.SetValue(viewModel, new Command(this, method.Name), new object[] { });
            }

            var notifyPropertyChanged = viewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged != null) {
                notifyPropertyChanged.PropertyChanged += PropertyChanged;
            }

            return viewModel;
        }
        #endregion

        public new TViewModel ViewModel {
            get { return (TViewModel)base.ViewModel; }
            private set { base.ViewModel = value; }
        }

        protected virtual void ViewLoaded() {
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e) {
            ViewLoaded();

            var view = View as View;
            if (view != null) {
                view.ResetFocus();
            }

            var frameworkElement = View as FrameworkElement;
            if (frameworkElement != null) {
                frameworkElement.Loaded -= OnViewLoaded;
            }
        }

        public new TView View {
            get { return (TView)base.View; }
            set {
                base.View = value;

                var frameworkElement = base.View as FrameworkElement;
                if (frameworkElement != null) {
                    frameworkElement.Loaded += OnViewLoaded;
                }
            }
        }
    }
}
