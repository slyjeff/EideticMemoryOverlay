using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;
using System.Collections.Generic;

namespace ArkhamOverlay.Events {
    public enum ChangeAction { Add, Update }

    public class ButtonInfoChanged : ICrossAppEvent, IButtonContext, ICardInfo {
        public ButtonInfoChanged(CardGroupId cardGroupId, ButtonMode buttonMode, int index, string name, string code, bool isToggled, bool imageAvailable, ChangeAction action, IList<ButtonOption> buttonOptions) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            Index = index;
            Name = name;
            Code = code;
            IsToggled = isToggled;
            ImageAvailable = imageAvailable;
            Action = action;
            ButtonOptions = buttonOptions;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
        public string Name { get; }
        public string Code { get; }
        public bool IsToggled { get; }
        public bool ImageAvailable { get; }
        public ChangeAction Action { get; }
        public IList<ButtonOption> ButtonOptions { get; }
    }

    public static class ButtonInfoChangedExtensions {
        public static void PublishButtonInfoChanged(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int index, string name, string code, bool isToggled, bool imageAvailable, ChangeAction action, IList<ButtonOption> buttonOptions) {
            eventBus.Publish(new ButtonInfoChanged(cardGroupId, buttonMode, index, name, code, isToggled, imageAvailable, action, buttonOptions));
        }

        public static void SubscribeToButtonInfoChanged(this IEventBus eventBus, Action<ButtonInfoChanged> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromButtonInfoChanged(this IEventBus eventBus, Action<ButtonInfoChanged> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
