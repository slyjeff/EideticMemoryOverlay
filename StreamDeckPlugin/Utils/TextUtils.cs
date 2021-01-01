using System.Text;

namespace StreamDeckPlugin.Utils {
    public static class TextUtils {
        public static string WrapTitle(string title) {
            string[] words = title.Split(' ');

            var newSentence = new StringBuilder();
            var line = "";
            foreach (string word in words) {
                if ((line + word).Length > 10) {
                    newSentence.AppendLine(line.Trim());
                    line = "";
                }

                line += string.Format("{0} ", word);
            }

            if (line.Trim().Length > 0)
                newSentence.AppendLine(line.Trim());

            return newSentence.ToString().Trim();
        }
    }
}
