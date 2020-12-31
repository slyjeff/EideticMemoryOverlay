using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Card Button", "arkhamoverlay.cardbutton")]
    public class CardButtonAction : StreamDeckAction<Card> {
        private int GetCardButtonIndex(Coordinates coordinates) {
            //assume 8 cards per row, as this app only makes since on the large streamdeck
            //subtract one for the "Return" location, since this will be in a folder
            return (coordinates.Row * 8 + coordinates.Column - 1);
        }

        private string WrapTitle(string title) {
            string[] words = title.Split(' ');

            var newSentence = new StringBuilder(); 
            var line = "";
            foreach (string word in words) {
                if ((line + word).Length > 10) {
                    newSentence.AppendLine(line.Trim());
                    line = "";
                }

                line += string.Format("{0} ", word);
            }

            if (line.Trim().Length > 0)
                newSentence.AppendLine(line.Trim());

            return newSentence.ToString().Trim();
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            var cardIndex = GetCardButtonIndex(args.Payload.Coordinates);
            try {
                var request = new GetCardInfoRequest { Deck = "Player1", Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoReponse>(request);

                SetImageAsync(response.CardButtonType.AsImage());
                return SetTitleAsync(WrapTitle(response.Name));
            } catch (Exception e) {
                return SetTitleAsync("");
            }
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            var cardIndex = GetCardButtonIndex(args.Payload.Coordinates);
            try {
                var request = new ClickCardButtonRequest { Deck = "Player1", Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoReponse>(request);

                SetImageAsync(response.CardButtonType.AsImage());
                return SetTitleAsync(WrapTitle(response.Name));
            } catch {
                return SetTitleAsync("");
            }
        }

        //protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, Card settings) {
        //    if (args != null) {

        //    }
        //    return base.OnDidReceiveSettings(args, settings);
        //}
    }
}