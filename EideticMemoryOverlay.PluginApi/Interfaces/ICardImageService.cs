namespace EideticMemoryOverlay.PluginApi.Interfaces {
    /// <summary>
    /// provide ability to load images for buttons
    /// </summary>
    public interface ICardImageService {
        //Load an image and initialize buttons with both the full image and an image cropped for showing on the button
        void LoadImage(IHasImageButton hasImageButton);
    }
}
