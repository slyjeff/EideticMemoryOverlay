using System;
using System.Windows.Input;

namespace PageController {
    class Command : ICommand {
        private readonly object _controller;
        private readonly string _methodName;

        public Command(object controller, string methodName) {
            _controller = controller;
            _methodName = methodName;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            var method = _controller.GetType().GetMethod(_methodName);

            var parmeters = (method.GetParameters().Length == 1) ? new[] { parameter } : new object[] { };
            method.Invoke(_controller, parmeters);
        }
    }
}
