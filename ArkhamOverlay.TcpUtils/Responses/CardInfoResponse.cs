namespace ArkhamOverlay.TcpUtils.Responses {
    public class CardInfoResponse : Response, ICardInfo {
        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public bool IsVisible { get; set; }
    }
}
