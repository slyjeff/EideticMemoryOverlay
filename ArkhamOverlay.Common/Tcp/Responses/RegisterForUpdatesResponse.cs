using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;
using System.Collections.Generic;

namespace ArkhamOverlay.Common.Tcp.Responses {
    public class CardGroupInfo : ICardGroupInfo {
        public CardGroupId CardGroupId { get; set; }

        public string Name { get; set; }

        public bool IsImageAvailable { get; set; }

        public string ImageId { get; set; }

        public IList<string> Zones { get; set; }
    }

    /// <summary>
    /// When a connection is established, send all state information
    /// </summary>
    public class RegisterForUpdatesResponse : Response {
        /// <summary>
        /// A list of all card groups
        /// </summary>
        public IList<CardGroupInfo> CardGroupInfo { get; set; }

        /// <summary>
        /// A list of all buttons
        /// </summary>
        public IList<ButtonInfo> Buttons { get; set; }
    }
}
