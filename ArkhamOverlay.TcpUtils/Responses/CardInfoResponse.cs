namespace ArkhamOverlay.TcpUtils.Responses {
    public class CardInfoResponse : Response, ICardInfo {
        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public byte[] ImageBytes { get; set; }
    }
}
