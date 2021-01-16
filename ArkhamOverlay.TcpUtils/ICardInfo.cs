namespace ArkhamOverlay.TcpUtils {
    public interface ICardInfo {
        CardButtonType CardButtonType { get; set; }
        string Name { get; set; }
        bool IsVisible { get; set; }
        bool ImageAvailable { get; set; }
    }
}
