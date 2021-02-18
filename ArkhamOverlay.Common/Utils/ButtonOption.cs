namespace ArkhamOverlay.Common.Utils {
    public delegate string ResolvePlaceholderDelegate(string parameterName);

    public class ButtonOption {
        public ButtonOption(string option, string text) {
            Option = option;
            Text = text;
        }

        public string Option { get; }
        public string Text { get; }

        public override bool Equals(object obj) {
            if (obj is ButtonOption buttonOption) {
                return (Option == buttonOption.Option)
                    && (Text == buttonOption.Text);
            }
            return false;
        }

        /// <summary>
        /// Look for placeholders in the text (text in <<xxxx>> format) and perform the callback to replace with actual values
        /// </summary>
        /// <param name="resolvePlaceholders">Callback that accepts a placeholder name and returns a value</param>
        /// <returns>The text with placeholders replaced with values</returns>
        /// <remarks>If a placholder cannot be resolved, the string returns as empty, so consumers know to ignore this option</remarks>
        public string GetTextResolvingPlaceholders(ResolvePlaceholderDelegate resolvePlaceholder) {
            var index = 0;
            var text = string.Empty;
            var startOfPlaceholder = Text.IndexOf("<<");
            while (startOfPlaceholder > 0) {
                var endOfPlaceholder = Text.IndexOf(">>", startOfPlaceholder);
                if (endOfPlaceholder == -1) {
                    break;
                }

                var placeholder = Text.Substring(startOfPlaceholder + 2, endOfPlaceholder - 2 - startOfPlaceholder);
                var resolvedPlaceHolder = resolvePlaceholder(placeholder);
                if (string.IsNullOrEmpty(resolvedPlaceHolder)) {
                    return string.Empty;
                }
                text += Text.Substring(index, startOfPlaceholder - index) + resolvedPlaceHolder;
                index += endOfPlaceholder + 2;
                startOfPlaceholder = Text.IndexOf(">>", index);
            }

            return text + Text.Substring(index); ;
        }
    }
}
