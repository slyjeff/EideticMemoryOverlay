using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Display Card", "arkhamoverlay.displaycard")]
    public class DisplayCardAction : StreamDeckAction<Card> {
        private int GetCardIndex(Coordinates coordinates) {
            //assume 16 cards per row, as this app only makes since on the large streamdeck
            //subtract one for the "Return" location, since this will be in a folder
            return (coordinates.Row * 16 + coordinates.Column - 1);
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
            var cardIndex = GetCardIndex(args.Payload.Coordinates);

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEP = new IPEndPoint(ipAddress, TcpInfo.Port);

            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                sender.Connect(remoteEP);

                var request = new GetCardInfoRequest { Deck = "Player1", Index = cardIndex };
                var payload = Encoding.ASCII.GetBytes(request.ToString());

                int bytesSent = sender.Send(payload);

                var bytes = new byte[1024];
                int bytesRec = sender.Receive(bytes);
                var responseData = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                var response = JsonConvert.DeserializeObject<GetCardInfoReponse>(responseData.Substring(0, responseData.IndexOf("<EOF>")));

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

                return SetTitleAsync(WrapTitle(response.Name));
            } catch {
                return SetTitleAsync("");
            }
        }

        //protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            //var card = args.Payload.GetSettings<Card>();
            //card.Name = "I Had\r\n A Plan";

            //// save the settings, and set the title
            //SetSettingsAsync(card);
            //return SetTitleAsync(card.Name);
        //}

        //protected override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, Card settings) {
        //    if (args != null) {

        //    }
        //    return base.OnDidReceiveSettings(args, settings);
        //}
    }
}