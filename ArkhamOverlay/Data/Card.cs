using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Services;
using ArkhamOverlay.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.Data {
    public delegate void CardToggledEvent(ICardButton card);

    public class Card {
        private static readonly Dictionary<string, BitmapImage> CardImageCache = new Dictionary<string, BitmapImage>();

        public Card() {
        }

        public Card(ArkhamDbCard arkhamDbCard, int count, bool isPlayerCard, bool cardBack = false) {
            Code = arkhamDbCard.Code;
            Count = count;
            Name = arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")";
            NameWithoutXp = arkhamDbCard.Name;
            Xp = arkhamDbCard.Xp == null ? 0 : int.Parse(arkhamDbCard.Xp);
            Faction = GetFaction(arkhamDbCard.Faction_Name);
            Type = GetCardType(arkhamDbCard.Type_Code);
            ImageSource = cardBack ? arkhamDbCard.BackImageSrc : arkhamDbCard.ImageSrc;
            IsPlayerCard = isPlayerCard;
            if (cardBack) {
                Name += " (Back)";
            }

            //stometimes if we are closing, this will be null and we can just bail
            if (Application.Current == null) {
                return;
            }

            if (Application.Current.Dispatcher.CheckAccess()) {
                LoadImage();
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    LoadImage();
                }));
            }
        }

        private void LoadImage() {
            if (string.IsNullOrEmpty(ImageSource)) {
                Image = ImageUtils.CreateSolidColorImage(CardColor);
                ButtonImage = Image;
                return;
            }

            if (CardImageCache.ContainsKey(Name)) {
                Image = CardImageCache[Name];
                CropImage();
                return;
            }

            var bitmapImage = new BitmapImage(new Uri("https://arkhamdb.com/" + ImageSource, UriKind.Absolute));
            bitmapImage.DownloadCompleted += (s, e) => {
                CardImageCache[Name] = bitmapImage;
                CropImage();
            };
            Image = bitmapImage;
        }

        private void CropImage() {
            var startingPoint = GetCropStartingPoint();
            ButtonImage = new CroppedBitmap(Image as BitmapImage, new Int32Rect(Convert.ToInt32(startingPoint.X), Convert.ToInt32(startingPoint.Y), 220, 220));

            byte[] bytes = null;
            var bitmapSource = ButtonImage as BitmapSource;

            if (bitmapSource != null) {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream()) {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            ButtonImageAsBytes = bytes;
        }

        private Point GetCropStartingPoint() {
            switch (Type) {
                case CardType.Scenario:
                    return new Point(40, 60);
                case CardType.Agenda:
                    return new Point(10, 40);
                case CardType.Act:
                    return new Point(190, 40);
                case CardType.Location:
                    return new Point(40, 40);
                case CardType.Enemy:
                    return new Point(40, 190);
                case CardType.Treachery:
                    return new Point(40, 0);
                case CardType.Asset:
                    return new Point(40, 40);
                case CardType.Event:
                    return new Point(40, 0);
                case CardType.Skill:
                    return new Point(40, 40);
                default:
                    return new Point(40, 40);
            }
        }

        public string Name { get; }
        public string NameWithoutXp { get; }
        public int Xp { get; }
        public string Code { get; }
        public Faction Faction { get; set; }
        public int Count { get; set; }

        public string ImageSource { get; }
        public ImageSource Image { get; private set; }

        public ImageSource ButtonImage { get; private set; }
        public byte[] ButtonImageAsBytes { get; private set; }

        public CardType Type { get; }

        public Color CardColor {
            get {
                switch (Type) {
                    case CardType.Agenda:
                        return Colors.Chocolate;
                    case CardType.Act:
                        return Colors.BurlyWood;
                    case CardType.Enemy:
                        return Colors.SlateBlue;
                    case CardType.Treachery:
                        return Colors.SlateGray;
                    case CardType.Asset:
                    case CardType.Event:
                    case CardType.Skill:
                        switch (Faction) {
                            case Faction.Guardian:
                                return Colors.DarkBlue;
                            case Faction.Seeker:
                                return Colors.DarkGoldenrod;
                            case Faction.Rogue:
                                return Colors.DarkGreen;
                            case Faction.Survivor:
                                return Colors.DarkRed;
                            case Faction.Mystic:
                                return Colors.Indigo;
                            default:
                                return Colors.DarkGray;
                        }
                    default:
                        return Colors.DarkGray;
                }
            }
        }

        public bool IsHorizontal { get { return Type == CardType.Act || Type == CardType.Agenda; } }

        public bool IsPlayerCard { get; private set; }

        public Card FlipSideCard { get; set; }

        public event Action<bool> IsDisplayedOnOverlayChanged;
        
        private bool _isDisplayedOnOverlay = false;
        public bool IsDisplayedOnOverlay {
            get => _isDisplayedOnOverlay;
            set {
                _isDisplayedOnOverlay = value;
                IsDisplayedOnOverlayChanged?.Invoke(_isDisplayedOnOverlay);
            }
        }

        private CardType GetCardType(string typeCode) {
            if(Enum.TryParse(typeCode, ignoreCase: true, out CardType type)) {
                return type;
            } else {
                return CardType.Other;
            }
        }

        private Faction GetFaction(string factionCode) {
            if (Enum.TryParse(factionCode, ignoreCase: true, out Faction type)) {
                return type;
            }
            else {
                return Faction.Other;
            }
        }
    }
}
