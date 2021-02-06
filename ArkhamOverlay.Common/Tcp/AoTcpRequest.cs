namespace ArkhamOverlay.Common.Tcp {
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
        ShowDeckList,
        StatValue,
        ChangeStatValue,
        Snapshot,
        GetInvestigatorImage,
        EventBus,
    };
}
