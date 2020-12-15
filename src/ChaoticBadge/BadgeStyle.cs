using System.Drawing;
using Svg;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a set of common configuration values for badge styling.
    /// </summary>
    public abstract record BadgeStyle
    {
        /// <summary>
        /// Font family.
        /// </summary>
        public string FontFamily { get; init; }

        /// <summary>
        /// Font size in points.
        /// </summary>
        public int FontSizePts { get; init; }

        /// <summary>
        /// Height.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Border size.
        /// </summary>
        public int Border => (int)((Height - FontSizePts) / 2.0f);

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
        protected BadgeStyle()
        {
            FontFamily = "Verdana,Helvetica,sans-serif";
            FontSizePts = 9;
            Height = 20;
        }

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        protected BadgeStyle(string fontFamily, int fontSizePts, int height)
        {
            FontFamily = fontFamily;
            FontSizePts = fontSizePts;
            Height = height;
        }

        /// <summary>
        /// Creates an SVG badge.
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="leftColor">Custom left-hand-side color.</param>
        /// <param name="rightColor">Custom right-hand-side color.</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Generated SVG.</returns>
        public abstract SvgDocument CreateSvg(string name, Status status, string? statusText = null,
            Color? leftColor = null, Color? rightColor = null, SvgDocument? icon = null);
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
