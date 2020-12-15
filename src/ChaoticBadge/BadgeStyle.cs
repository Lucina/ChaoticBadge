using Svg;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a set of common configuration values for badge styling.
    /// </summary>
    public record BadgeStyle
    {
        /// <summary>
        /// Singleton instance of default style.
        /// </summary>
        public static readonly BadgeStyle Default = new BadgeStyle();

        /// <summary>
        /// Font family.
        /// </summary>
        public string FontFamily { get; }

        /// <summary>
        /// Font size in points.
        /// </summary>
        public int FontSizePts { get; }

        /// <summary>
        /// Height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Border size.
        /// </summary>
        public int Border { get; }

        /// <summary>
        /// Y-offset of text baseline.
        /// </summary>
        public int TextOffsetY => (int)(Height / 2.0f + FontSizePts / 2.0f);

        /// <summary>
        /// Y-offset of text shadow baseline.
        /// </summary>
        public int TextShadowOffsetY => TextOffsetY + 1;

        /// <summary>
        /// Font size in <see cref="SvgUnit"/>.
        /// </summary>
        public SvgUnit FontSizeSvgUnits => new(SvgUnitType.Point, FontSizePts);

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the default style.
        /// </summary>
        public BadgeStyle()
        {
            FontFamily = "Verdana,Helvetica,sans-serif";
            FontSizePts = 9;
            Height = 20;
            Border = 6;
        }

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="border">Border size.</param>
        public BadgeStyle(string fontFamily, int fontSizePts, int height, int border)
        {
            FontFamily = fontFamily;
            FontSizePts = fontSizePts;
            Height = height;
            Border = border;
        }
    }
}
