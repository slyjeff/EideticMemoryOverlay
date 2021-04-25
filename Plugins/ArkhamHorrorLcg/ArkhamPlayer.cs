using Emo.Common.Enums;
using EideticMemoryOverlay.PluginApi;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;

namespace ArkhamHorrorLcg {
    public class ArkhamPlayer : Player {
        public ArkhamPlayer(CardGroupId deck, IPlugIn plugIn) : base(deck, plugIn) {
            CardGroup.AddCardZone(new CardZone("Hand", CardZoneLocation.Bottom));
            CardGroup.AddCardZone(new CardZone("Threat Area", CardZoneLocation.Bottom));
            CardGroup.AddCardZone(new CardZone("Tableau", CardZoneLocation.Bottom));

            Health = new Stat(StatType.Health, deck);
            Sanity = new Stat(StatType.Sanity, deck);
            Resources = new Stat(StatType.Resources, deck);
            Clues = new Stat(StatType.Clues, deck);

            Faction = Faction.Other;

            Resources.Value = 5;
        }

        public IDictionary<string, int> Slots { get; set; }

        public ImageSource BaseStateLineImage { get; private set; }
        public ImageSource FullInvestigatorImage { get; private set; }

        protected override void LoadOverlayImages() {
            BaseStateLineImage = ArkhamImageUtils.CropBaseStatLine(Image);
            NotifyPropertyChanged(nameof(BaseStateLineImage));

            FullInvestigatorImage = ArkhamImageUtils.CropFullInvestigator(Image);
            NotifyPropertyChanged(nameof(FullInvestigatorImage));
        }

        public override Point GetCropStartingPoint() {
            return new Point(10, 50);
        }

        public string InvestigatorCode { get; set; }

        public override void OnPlayerChanged() {
            NotifyPropertyChanged(nameof(PlayerNameBrush));
            base.OnPlayerChanged();
        }

        public Stat Health { get; }
        public Stat Sanity { get; }
        public Stat Resources { get; }
        public Stat Clues { get; }

        public Brush PlayerNameBrush {
            get {
                switch (Faction) {
                    case Faction.Guardian: return new SolidColorBrush(Colors.DodgerBlue);
                    case Faction.Seeker: return new SolidColorBrush(Colors.DarkGoldenrod);
                    case Faction.Mystic: return new SolidColorBrush(Colors.MediumPurple);
                    case Faction.Rogue: return new SolidColorBrush(Colors.MediumSpringGreen);
                    case Faction.Survivor: return new SolidColorBrush(Colors.Red);
                    default: return new SolidColorBrush(Colors.Black);
                }
            }
        }

        public Faction Faction { get; set; }
    }
}