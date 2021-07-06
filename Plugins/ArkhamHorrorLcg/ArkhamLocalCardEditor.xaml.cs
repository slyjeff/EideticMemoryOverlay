using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ArkhamHorrorLcg {
    public partial class ArkhamLocalCardEditor : UserControl, ILocalCardEditor {
        public ArkhamLocalCardEditor() {
            InitializeComponent();
        }

        public void SetCard(EditableLocalCard card) {
            DataContext = new ArkhamLocalCardEditorViewModel(card as ArkhamEditableLocalCard);
        }

    }

    internal class ArkhamLocalCardEditorViewModel {
        public ArkhamLocalCardEditorViewModel(ArkhamEditableLocalCard card) {
            CardTypes = new List<string> { "Asset", "Event", "Skill", "Scenario", "Agenda", "Act", "Enemy", "Treachery", "Location", "Investigator" };
            Card = card;
        }

        public IList<string> CardTypes { get; }
        public ArkhamEditableLocalCard Card { get; }
    }
}
