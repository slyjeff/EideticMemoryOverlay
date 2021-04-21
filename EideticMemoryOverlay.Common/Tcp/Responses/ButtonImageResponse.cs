namespace Emo.Common.Tcp.Responses {
    public class ButtonImageResponse : Response {
        public string Code { get; set; }
        public byte[] Bytes { get; set; }
    }
}
