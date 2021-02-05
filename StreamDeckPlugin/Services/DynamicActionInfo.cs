using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Services {
    public interface IDynamicActionInfo {
        Deck Deck { get; }
        int Index { get; }
        DynamicActionMode Mode { get; }
        bool IsImageAvailable { get; set; }
        string ImageId { get; set; }
        string Text { get; set; }
        bool IsToggled { get; set; }
    }

    public class DynamicActionInfo : IDynamicActionInfo {
        public DynamicActionInfo(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
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
            dynamicActionInfo.Text = cardInfo.Name;
            dynamicActionInfo.IsToggled = cardInfo.IsToggled;
            dynamicActionInfo.ImageId = cardInfo.Name;
            dynamicActionInfo.IsImageAvailable = cardInfo.ImageAvailable;
        }
    }
}
