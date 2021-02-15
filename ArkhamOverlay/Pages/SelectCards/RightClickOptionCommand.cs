using System;
using System.Windows.Input;

namespace ArkhamOverlay.Pages.SelectCards {
    public class RightClickOptionCommand : ICommand {
        private readonly string _option;
        private readonly Action<string> _optionSelectedCallback;

        public RightClickOptionCommand(string option, string text, Action<string> optionSelectedCallback) {
            _option = option;
            Text = text;
            _optionSelectedCallback = optionSelectedCallback;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            _optionSelectedCallback(_option);
        }

        public string Text { get; }
    }
}
