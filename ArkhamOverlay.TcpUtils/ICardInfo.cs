namespace ArkhamOverlay.TcpUtils {
    public interface ICardInfo {
        CardButtonType CardButtonType { get; set; }
        string Name { get; set; }
        string ImageSource { get; set; }
        bool IsVisible { get; set; }
    }
}
