namespace ArkhamOverlay.TcpUtils {
    public enum AoTcpRequest { Unknown, GetCardInfo, ToggleCard, ClearAll };

    public static class AoTcpRequestExtensions {
        public static string AsString(this AoTcpRequest request) {
            switch (request) {
                case AoTcpRequest.GetCardInfo:
                    return "Info";
                case AoTcpRequest.ToggleCard:
                    return "Toggle";
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
                case "Toggle":
                    return AoTcpRequest.ToggleCard;
                case "ClearAll":
                    return AoTcpRequest.ClearAll;
                default:
                    return AoTcpRequest.Unknown;
            }
        }
    }

}
