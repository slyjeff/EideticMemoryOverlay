using Emo.Common.Utils;
using System;
using System.Windows.Input;

namespace Emo.Pages.SelectCards {
    public class RightClickOptionCommand : ICommand {
        private readonly ButtonOption _option;
        private readonly Action<ButtonOption> _optionSelectedCallback;

        public RightClickOptionCommand(ButtonOption option, string text, Action<ButtonOption> optionSelectedCallback) {
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
