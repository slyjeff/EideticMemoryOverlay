namespace Emo.Common.Tcp {
    public enum AoTcpRequest {
        Unknown,
        GetButtonInfo,
        GetButtonImage,
        ConnectionIsAlive,
        UpdateStatInfo,
        RegisterForUpdates,
        StatValue,
        ChangeStatValue,
        EventBus,
    };
}
