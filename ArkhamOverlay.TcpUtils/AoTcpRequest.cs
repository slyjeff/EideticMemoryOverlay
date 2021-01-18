namespace ArkhamOverlay.TcpUtils {
    public enum AoTcpRequest {
        Unknown,
        GetCardInfo,
        GetButtonImage,
        ClickCardButton, 
        ClearAll, 
        UpdateCardInfo, 
        RegisterForUpdates,
        ToggleActAgendaBarRequest,
        ActAgendaBarStatusRequest,
        ShowDeckList
    };

    public static class AoTcpRequestExtensions {
        public static string AsString(this AoTcpRequest request) {
            switch (request) {
                case AoTcpRequest.GetCardInfo:
                    return "Info";
                case AoTcpRequest.GetButtonImage:
                    return "Image";
                case AoTcpRequest.ClickCardButton:
                    return "ClickCardButton";
                case AoTcpRequest.ClearAll:
                    return "ClearAll";
                case AoTcpRequest.UpdateCardInfo:
                    return "UpdateCardInfo";
                case AoTcpRequest.RegisterForUpdates:
                    return "RegisterForUpdates";
                case AoTcpRequest.ToggleActAgendaBarRequest:
                    return "ToggleActAgendaBarRequest";
                case AoTcpRequest.ActAgendaBarStatusRequest:
                    return "ActAgendaBarStatusRequest";
                case AoTcpRequest.ShowDeckList:
                    return "ShowDeckList";
                default:
                    return "Unkown";
            }
        }

        public static AoTcpRequest AsAoTcpRequest(this string request) {
            switch (request) {
                case "Info":
                    return AoTcpRequest.GetCardInfo;
                case "Image":
                    return AoTcpRequest.GetButtonImage;
                case "ClickCardButton":
                    return AoTcpRequest.ClickCardButton;
                case "ClearAll":
                    return AoTcpRequest.ClearAll;
                case "UpdateCardInfo":
                    return AoTcpRequest.UpdateCardInfo;
                case "RegisterForUpdates":
                    return AoTcpRequest.RegisterForUpdates;
                case "ToggleActAgendaBarRequest":
                    return AoTcpRequest.ToggleActAgendaBarRequest;
                case "ActAgendaBarStatusRequest":
                    return AoTcpRequest.ActAgendaBarStatusRequest;
                case "ShowDeckList":
                    return AoTcpRequest.ShowDeckList;
                default:
                    return AoTcpRequest.Unknown;
            }
        }
    }

}
