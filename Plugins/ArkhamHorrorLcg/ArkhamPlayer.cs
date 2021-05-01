using Emo.Common.Enums;
using EideticMemoryOverlay.PluginApi;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using EideticMemoryOverlay.PluginApi.Buttons;

namespace ArkhamHorrorLcg {
    public class ArkhamPlayer : Player {
        public ArkhamPlayer(ICardGroup cardGroup) : base(cardGroup) {
            CardGroup.AddCardZone(new CardZone("Hand", CardZoneLocation.Bottom));
            CardGroup.AddCardZone(new CardZone("Threat Area", CardZoneLocation.Bottom));
            CardGroup.AddCardZone(new CardZone("Tableau", CardZoneLocation.Bottom));

            Health = CreateStat(StatType.Health);
            Sanity = CreateStat(StatType.Sanity);
            Resources = CreateStat(StatType.Resources);
            Clues = CreateStat(StatType.Clues);

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

        public Stat Health { get; }
        public Stat Sanity { get; }
        public Stat Resources { get; }
        public Stat Clues { get; }

        public Faction Faction { get; set; }

        public override IList<DeckListItem> GetDeckList() {
            var cards = from cardButton in CardGroup.CardButtons.OfType<CardInfoButton>()
                        select cardButton.CardInfo as ArkhamCardInfo;

            var deckList = new List<DeckListItem>();
            foreach (var card in cards.Where(c => !c.IsBonded)) {
                deckList.Add(new DeckListItem(card));
            }

            var bondedCards = cards.Where(c => c.IsBonded);
            if (bondedCards.Any()) {
                deckList.Add(new DeckListItem("Bonded Cards:"));

                foreach (var card in bondedCards) {
                    deckList.Add(new DeckListItem(card));
                }
            }

            return deckList;
        }
    }
}