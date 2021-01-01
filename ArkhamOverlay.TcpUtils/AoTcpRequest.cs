namespace ArkhamOverlay.TcpUtils {
    public enum AoTcpRequest {
        Unknown,
        GetCardInfo,
        ClickCardButton, 
        ClearAll, 
        UpdateCardInfo, 
        RegisterForUpdates
    };

    public static class AoTcpRequestExtensions {
        public static string AsString(this AoTcpRequest request) {
            switch (request) {
                case AoTcpRequest.GetCardInfo:
                    return "Info";
                case AoTcpRequest.ClickCardButton:
                    return "ClickCardButton";
                case AoTcpRequest.ClearAll:
                    return "ClearAll";
                case AoTcpRequest.UpdateCardInfo:
                    return "UpdateCardInfo";
                case AoTcpRequest.RegisterForUpdates:
                    return "RegisterForUpdates";
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
                case "UpdateCardInfo":
                    return AoTcpRequest.UpdateCardInfo;
                case "RegisterForUpdates":
                    return AoTcpRequest.RegisterForUpdates;
                default:
                    return AoTcpRequest.Unknown;
            }
        }
    }

}
