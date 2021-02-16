using ArkhamOverlay.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace ArkhamOverlay.Common.Utils {
    public enum ButtonMode { Pool, Zone }

    /// <summary>
    /// Identify a button in three parts (eventually four)
    /// </summary>
    /// <remarks>Eventually will need to add a Card Zone Index when we develop for multiple card zones</remarks>
    public interface IButtonContext {
        CardGroupId CardGroupId { get; }
        ButtonMode ButtonMode { get; }
        int Index { get; }
    }

    public static class ButtonContextExtensions {
        public static bool HasSameContext(this IButtonContext a, IButtonContext b) {
            return a.CardGroupId == b.CardGroupId && a.ButtonMode == b.ButtonMode && a.Index == b.Index;
        }

        public static bool IsAfter(this IButtonContext a, IButtonContext b) {
            return a.CardGroupId == b.CardGroupId && a.ButtonMode == b.ButtonMode && a.Index > b.Index;
        }

        public static bool IsAtSameIndexOrAfter(this IButtonContext a, IButtonContext b) {
            return a.CardGroupId == b.CardGroupId && a.ButtonMode == b.ButtonMode && a.Index >= b.Index;
        }


        public static IEnumerable<T> FindAllWithContext<T>(this IEnumerable<T> list, IButtonContext context) where T : IButtonContext {
            return from potentialContext in list
                   where potentialContext.HasSameContext(context)
                   select potentialContext;
        }
        public static T FirstOrDefaultWithContext<T>(this IEnumerable<T> list, IButtonContext context) where T : IButtonContext {
            return list.FirstOrDefault(x => x.HasSameContext(context));
        }
    }
}
