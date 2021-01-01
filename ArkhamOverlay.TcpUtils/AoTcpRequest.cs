namespace ArkhamOverlay.TcpUtils {
    public enum AoTcpRequest { Unknown, GetCardInfo, ClickCardButton, ClearAll };

    public static class AoTcpRequestExtensions {
        public static string AsString(this AoTcpRequest request) {
            switch (request) {
                case AoTcpRequest.GetCardInfo:
                    return "Info";
                case AoTcpRequest.ClickCardButton:
                    return "ClickCardButton";
                case AoTcpRequest.ClearAll:
                    return "ClearAll";
                default:
                    return "Unkown";
            }
        }

        public static AoTcpRequest AsAoTcpRequest(this string request) {
            switch (request) {
                case "Info":
                    return AoTcpRequest.GetCardInfo;
                case "ClickCardButton":
                    return AoTcpRequest.ClickCardButton;
                case "ClearAll":
                    return AoTcpRequest.ClearAll;
                default:
                    return AoTcpRequest.Unknown;
            }
        }
    }

}
