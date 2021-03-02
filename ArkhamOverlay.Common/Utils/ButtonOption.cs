using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Utils {
    /// <summary>
    /// Provided to Button Option to resolve the text and images dynamically
    /// </summary>
    public interface IButtonOptionResolver {
        /// <summary>
        /// Used by Button option to find the name of a Card Group based on the ID
        /// </summary>
        /// <param name="cardGroupId">Id of the Card Group</param>
        /// <returns>Name of the Card Group</returns>
        string GetCardGroupName(CardGroupId cardGroupId);

        /// <summary>
        /// Used by Button option to find the name of a Card Group Zone based on ID and Index
        /// </summary>
        /// <param name="cardGroupId">Id of the Card Group</param>
        /// <param name="cardGroupId">Index of the zone within the Card Group</param>
        /// <returns>Name of the Zone</returns>
        string GetCardZoneName(CardGroupId cardGroupId, int zoneIndex);

        /// <summary>
        /// Used by Button option to return an imageId based on ID and index
        /// </summary>
        /// <param name="cardGroupId">Id of the Card Group</param>
        /// <param name="cardGroupId">Index of the zone within the Card Group</param>
        /// <returns>image id corresponding to the card group and zone</returns>
        string GetImageId(CardGroupId cardGroupId, int zoneIndex);
    }

    /// <summary>
    /// Used to determine the text of the button and what information is provided
    /// </summary>
    public enum ButtonOptionOperation { Add, Move, Remove }

    public class ButtonOption {
        /// <summary>
        /// Create a button option with defaults
        /// </summary>
        public ButtonOption() {
            Operation = ButtonOptionOperation.Remove;
            CardGroupId = CardGroupId.Player1;
            ZoneIndex = -1;
        }

        /// <summary>
        /// Create a button option with only the action initialized
        /// </summary>
        /// <param name="operation">What kind of action this button represents</param>
        public ButtonOption(ButtonOptionOperation operation) {
            Operation = operation;
            CardGroupId = CardGroupId.Player1;
            ZoneIndex = -1;
        }

        /// <summary>
        /// Create a button option with initialized values
        /// </summary>
        /// <param name="operation">What kind of action this button represents</param>
        /// <param name="cardGroupId">The destination Card Group of this action</param>
        /// <param name="zoneIndex">The destination zone of this action</param>
        public ButtonOption(ButtonOptionOperation operation, CardGroupId cardGroupId, int zoneIndex) {
            Operation = operation;
            CardGroupId = cardGroupId;
            ZoneIndex = zoneIndex;
        }

        public ButtonOptionOperation Operation { get; set; }
        public CardGroupId CardGroupId { get; set; }
        public int ZoneIndex { get; set; }

        /// <summary>
        /// Get the text, using the resolver to provide names for card groups and zones
        /// </summary>
        /// <param name="resolver">Reslover used to provide dynamic values when displaying option text</param>
        /// <returns>The text with dynamic information replaced with values</returns>
        /// <remarks>If a placholder cannot be resolved, the string returns as empty, so consumers know to ignore this option</remarks>
        public string GetText(IButtonOptionResolver resolver) {
            if (Operation == ButtonOptionOperation.Remove) {
                return "Remove Card";
            }

            var zoneName = resolver.GetCardZoneName(CardGroupId, ZoneIndex);
            if (Operation == ButtonOptionOperation.Move) {
                return $"Move to {zoneName}";
            }

            var cardGroupName = resolver.GetCardGroupName(CardGroupId);
            return $"Add to {zoneName} of {cardGroupName}";
        }


        /// <summary>
        /// Get an image Id using the resolver
        /// </summary>
        /// <param name="resolver">Resolver used to provide a dynamic image value for this button</param>
        /// <returns>Image Id returned by the resolver</returns>
        public string GetImageId(IButtonOptionResolver resolver) {
            if (Operation == ButtonOptionOperation.Remove) {
                return string.Empty;
            }

            return resolver.GetImageId(CardGroupId, ZoneIndex);
        }
    }
}
