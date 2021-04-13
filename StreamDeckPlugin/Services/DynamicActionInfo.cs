using Emo.Common;
using Emo.Common.Enums;
using Emo.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Services {
    public interface IDynamicActionInfo : IButtonContext {
        bool IsImageAvailable { get; set; }
        string ImageId { get; set; }
        string Text { get; set; }
        bool IsToggled { get; set; }
        IList<ButtonOption> ButtonOptions {get; set;}
    }


    public class DynamicActionInfo : IDynamicActionInfo {
        public DynamicActionInfo(IButtonContext buttonContex) {
            CardGroupId = buttonContex.CardGroupId;
            ButtonMode = buttonContex.ButtonMode;
            ZoneIndex = buttonContex.ZoneIndex;
            Index = buttonContex.Index;
            ButtonOptions = new List<ButtonOption>();
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; set; }
        public int Index { get; set; }
        public string ImageId { get; set; }
        public bool IsImageAvailable { get; set; }
        public string Text { get; set; }
        public bool IsToggled { get; set; }
        public IList<ButtonOption> ButtonOptions { get; set; }
    }

    static class DynamicActionInfoExtensions {
        static internal bool CardInfoHasChanged(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            return dynamicActionInfo.Text != cardInfo.Name
                || dynamicActionInfo.IsToggled != cardInfo.IsToggled
                || dynamicActionInfo.ImageId != cardInfo.Code
                || dynamicActionInfo.IsImageAvailable != cardInfo.ImageAvailable
                || dynamicActionInfo.ButtonOptions.SequenceEqual(cardInfo.ButtonOptions);
        }

        static internal void UpdateFromCardInfo(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            dynamicActionInfo.Text = cardInfo.Name.Replace("Right Click", "Long Press");
            dynamicActionInfo.IsToggled = cardInfo.IsToggled;
            dynamicActionInfo.ImageId = cardInfo.Code;
            dynamicActionInfo.IsImageAvailable = cardInfo.ImageAvailable;
            dynamicActionInfo.ButtonOptions = cardInfo.ButtonOptions;
        }
    }
}
