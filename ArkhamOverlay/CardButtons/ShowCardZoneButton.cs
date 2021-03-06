﻿using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class ShowCardZoneButton : Button {
        public ShowCardZoneButton(CardZone cardZone)  {
            Text = $"Show {cardZone.Name}";
            CardZone = cardZone;
        }

        public CardZone CardZone { get; }
    }
}