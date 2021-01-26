namespace ArkhamOverlay.TcpUtils {
    public enum AoTcpRequest {
        Unknown,
        GetCardInfo,
        GetButtonImage,
        ClickCardButton, 
        ClearAll, 
        UpdateCardInfo,
        UpdateStatInfo,
        UpdateInvestigatorImage,
        RegisterForUpdates,
        ToggleActAgendaBarRequest,
        ActAgendaBarStatusRequest,
        ShowDeckList,
        ChangeStatValue,
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
                case AoTcpRequest.UpdateStatInfo:
                    return "UpdateStatInfo";
                case AoTcpRequest.UpdateInvestigatorImage:
                    return "UpdateInvestigatorImage";
                case AoTcpRequest.RegisterForUpdates:
                    return "RegisterForUpdates";
                case AoTcpRequest.ToggleActAgendaBarRequest:
                    return "ToggleActAgendaBarRequest";
                case AoTcpRequest.ActAgendaBarStatusRequest:
                    return "ActAgendaBarStatusRequest";
                case AoTcpRequest.ShowDeckList:
                    return "ShowDeckList";
                case AoTcpRequest.ChangeStatValue:
                    return "ChangeStatValue";
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
                case "UpdateStatInfo":
                    return AoTcpRequest.UpdateStatInfo;
                case "UpdateInvestigatorImage":
                    return AoTcpRequest.UpdateInvestigatorImage;
                case "RegisterForUpdates":
                    return AoTcpRequest.RegisterForUpdates;
                case "ToggleActAgendaBarRequest":
                    return AoTcpRequest.ToggleActAgendaBarRequest;
                case "ActAgendaBarStatusRequest":
                    return AoTcpRequest.ActAgendaBarStatusRequest;
                case "ShowDeckList":
                    return AoTcpRequest.ShowDeckList;
                case "ChangeStatValue":
                    return AoTcpRequest.ChangeStatValue;
                default:
                    return AoTcpRequest.Unknown;
            }
        }
    }

}
