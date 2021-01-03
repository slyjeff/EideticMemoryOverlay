using ArkhamOverlay.Data;
using PageController;
using System.Collections.ObjectModel;

namespace ArkhamOverlay.Pages.Overlay {
    public class OverlayViewModel : ViewModel {
        public OverlayViewModel() {
            ActAgendaCards = new ObservableCollection<OverlayCardViewModel>();
            EncounterCards = new ObservableCollection<OverlayCardViewModel>();
            PlayerCards = new ObservableCollection<OverlayCardViewModel>();
        }
        public virtual bool ShowActAgendaBar { get; set; }

        public virtual Configuration Configuration { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> ActAgendaCards { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> EncounterCards { get; set; }
        public virtual ObservableCollection<OverlayCardViewModel> PlayerCards { get; set; }
    }
}
