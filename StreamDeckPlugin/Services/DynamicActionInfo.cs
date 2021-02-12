using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;

namespace StreamDeckPlugin.Services {
    public interface IDynamicActionInfo : IButtonContext {
        bool IsImageAvailable { get; set; }
        string ImageId { get; set; }
        string Text { get; set; }
        bool IsToggled { get; set; }
    }


    public class DynamicActionInfo : IDynamicActionInfo {
        public DynamicActionInfo(IButtonContext buttonContex) {
            CardGroup = buttonContex.CardGroup;
            CardZoneIndex = buttonContex.CardZoneIndex;
            Index = buttonContex.Index;
        }

        public CardGroup CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
        public string ImageId { get; set; }
        public bool IsImageAvailable { get; set; }
        public string Text { get; set; }
        public bool IsToggled { get; set; }
    }

    static class DynamicActionInfoExtensions {
        static internal bool CardInfoHasChanged(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            return dynamicActionInfo.Text != cardInfo.Name
                || dynamicActionInfo.IsToggled != cardInfo.IsToggled
                || dynamicActionInfo.ImageId != cardInfo.Name
                || dynamicActionInfo.IsImageAvailable != cardInfo.ImageAvailable;
        }

        static internal void UpdateFromCardInfo(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            dynamicActionInfo.Text = cardInfo.Name.Replace("Right Click", "Long Press");
            dynamicActionInfo.IsToggled = cardInfo.IsToggled;
            dynamicActionInfo.ImageId = cardInfo.Name;
            dynamicActionInfo.IsImageAvailable = cardInfo.ImageAvailable;
        }
    }
}
