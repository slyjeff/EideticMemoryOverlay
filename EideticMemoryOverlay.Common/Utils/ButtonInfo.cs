using Emo.Common.Enums;
using System.Collections.Generic;

namespace Emo.Common.Utils {
    public class ButtonInfo : IButtonContext, ICardInfo {
        public CardGroupId CardGroupId { get; set; }

        public ButtonMode ButtonMode { get; set; }

        public int ZoneIndex { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsToggled { get; set; }

        public bool ImageAvailable { get; set; }

        public IList<ButtonOption> ButtonOptions { get; set; }
    }

}
