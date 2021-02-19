﻿using System.Threading.Tasks;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Toggle Set", "arkhamoverlay.toggleset")]
    public class ToggleSetAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            //just make the image blank for this button
            SetImageAsync(ImageUtils.BlankImage());

            return Task.CompletedTask;
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishModeToggledEvent();
            return Task.CompletedTask;
        }
    }
}