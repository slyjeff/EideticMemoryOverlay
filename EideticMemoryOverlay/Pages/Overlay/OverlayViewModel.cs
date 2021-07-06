using EideticMemoryOverlay.PluginApi;
using Emo.Data;
using PageController;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Emo.Pages.Overlay {
    public class OverlayViewModel : ViewModel {
        public OverlayViewModel() {
            TopZoneCards = new ObservableCollection<OverlayCardViewModel>();
            EncounterCardInfos = new ObservableCollection<OverlayCardViewModel>();
            PlayerCardInfos = new ObservableCollection<OverlayCardViewModel>();
            BottomZoneCards = new ObservableCollection<OverlayCardViewModel>();
        }

        public virtual AppData AppData { get; set; }
        public virtual double OverlayFontSize { get; set; }
        public virtual double StatFontSize { get; set; }
        public virtual double DeckListItemWidth { get; set; }
        public virtual double DeckListHeight { get; set; }
        public virtual Thickness DeckListMargin { get; set; }
        public virtual bool ShowDeckList { get; set; }
        public virtual double StatImageSize { get; set; }
        public virtual double InvestigatorImageSize { get; set; }

        public virtual IList<DeckListItem> DeckList { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> TopZoneCards { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> EncounterCardInfos { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> PlayerCardInfos { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> BottomZoneCards { get; set; }

        public IList<ObservableCollection<OverlayCardViewModel>> AllOverlayCards {
            get {
                return new List<ObservableCollection<OverlayCardViewModel>> { TopZoneCards, EncounterCardInfos, PlayerCardInfos, BottomZoneCards };
            }
        }
    }
}
