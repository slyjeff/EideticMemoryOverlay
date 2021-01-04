using ArkhamOverlay.Data;
using PageController;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsViewModel : ViewModel {
        public virtual ISelectableCards SelectableCards { get; set; }
    }
}
