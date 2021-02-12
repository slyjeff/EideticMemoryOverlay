using System.Collections.Generic;
using System.Linq;

namespace ArkhamOverlay.Common.Enums {
    public interface IButtonContext {
        CardGroup CardGroup { get; }
        int CardZoneIndex { get; }
        int Index { get; }
    }

    public static class ButtonContextExtensions { 
        public static bool HasSameContext(this IButtonContext a, IButtonContext b) {
            return a.CardGroup == b.CardGroup && a.CardZoneIndex == b.CardZoneIndex && a.Index == b.Index;
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
