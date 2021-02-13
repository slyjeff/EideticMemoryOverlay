using System.Collections.Generic;
using System.Linq;

namespace ArkhamOverlay.Common.Enums {
    public enum ButtonMode { Pool, Zone, Dialog }

    public interface IButtonContext {
        CardGroupId CardGroupId { get; }
        ButtonMode ButtonMode { get; }
        int Index { get; }
    }

    public static class ButtonContextExtensions { 
        public static bool HasSameContext(this IButtonContext a, IButtonContext b) {
            return a.CardGroupId == b.CardGroupId && a.ButtonMode == b.ButtonMode && a.Index == b.Index;
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
