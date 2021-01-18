namespace ArkhamOverlay.TcpUtils.Requests {
    public class UpdateCardInfoRequest : Request, ICardInfo {
        public UpdateCardInfoRequest() : base(AoTcpRequest.UpdateCardInfo) {
        }

        public Deck Deck { get; set; }
        public int Index { get; set; }
        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public bool IsToggled { get; set; }
        public bool ImageAvailable { get; set; }
        public bool IsCardInSet { get; set; }
    }
}
