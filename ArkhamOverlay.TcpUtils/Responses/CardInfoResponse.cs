namespace ArkhamOverlay.TcpUtils.Responses {
    public class CardInfoResponse : Response {

        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public string ImageSource { get; set; }
    }
}
