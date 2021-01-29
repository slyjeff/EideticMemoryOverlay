namespace ArkhamOverlay.Pages.LocalImages {
    public class PackManifest {
        public PackManifest() {
        }
        public PackManifest(LocalPack pack) {
            Name = pack.Name;
        }

        public string Name { get; set; }
    }
}
