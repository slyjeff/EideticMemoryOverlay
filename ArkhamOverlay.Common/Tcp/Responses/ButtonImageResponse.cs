﻿namespace ArkhamOverlay.Common.Tcp.Responses {
    public class ButtonImageResponse : Response {
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
    }
}