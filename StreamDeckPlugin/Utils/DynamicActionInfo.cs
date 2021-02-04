using ArkhamOverlay.TcpUtils;
using System;
using System.Collections.Generic;

namespace StreamDeckPlugin.Utils {
    public interface IDynamicActionInfo {
        Deck Deck { get; }
        int Index { get; }
        DynamicActionMode Mode { get; }
        bool IsImageAvailable { get; }
        string ImageId { get; }
        string Text { get; set; }
        bool IsToggled { get; set; }
    }

    public class DynamicActionInfo : IDynamicActionInfo {
        public DynamicActionInfo(Deck deck, int index, DynamicActionMode mode, string imageId, bool isImageAvailable) {
            Deck = deck;
            Index = index;
            Mode = mode;
            ImageId = imageId;
            IsImageAvailable = isImageAvailable;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
        public string ImageId { get; }
        public bool IsImageAvailable { get; }
        public string Text { get; set; }
        public bool IsToggled { get; set; }
    }

    static class DynamicActionInfoExtensions {
        static internal bool CardInfoHasChanged(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            return (dynamicActionInfo.Text != cardInfo.Name) || (dynamicActionInfo.IsToggled != cardInfo.IsToggled);
        }

        static internal void UpdateFromCardInfo(this IDynamicActionInfo dynamicActionInfo, ICardInfo cardInfo) {
            dynamicActionInfo.Text = cardInfo.Name;
            dynamicActionInfo.IsToggled = cardInfo.IsToggled;
        }
    }
}
