using System.Collections.Generic;
using System.Drawing;
using Svg;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a flat badge style.
    /// </summary>
    public record FlatBadgeStyle : SimpleBadgeStyle
    {
        /// <summary>
        /// Default text shadow color.
        /// </summary>
        public static readonly Color ShadowColor = Color.FromArgb(0x20, 0x20, 0x20);

        /// <summary>
        /// Default left color.
        /// </summary>
        public static readonly Color DefaultLeftColor = Color.FromArgb(0x40, 0x40, 0x40);

        /// <summary>
        /// Default passing color.
        /// </summary>
        public static readonly Color PassingRightColor = Color.FromArgb(0x2d, 0xea, 0x56);

        /// <summary>
        /// Default failing color.
        /// </summary>
        public static readonly Color FailingRightColor = Color.FromArgb(0xf3, 0x3b, 0x27);

        /// <summary>
        /// Default unknown color.
        /// </summary>
        public static readonly Color UnknownRightColor = Color.FromArgb(0xea, 0x83, 0x2d);

        /// <summary>
        /// Default mapping for <see cref="Status"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<Status, Color> StandardColorMap =
            new Dictionary<Status, Color>
            {
                [Status.Passing] = PassingRightColor,
                [Status.Failing] = FailingRightColor,
                [Status.Unknown] = UnknownRightColor,
                [Status.Release] = PassingRightColor,
                [Status.PreRelease] = UnknownRightColor,
                [Status.NotFound] = FailingRightColor,
                [Status.Error] = FailingRightColor
            };

        /// <summary>
        /// Singleton instance of default style.
        /// </summary>
        public static readonly FlatBadgeStyle Default = new FlatBadgeStyle();

        /// <summary>
        /// Left-hand-side background color.
        /// </summary>
        public Color LeftColor { get; init; }

        /// <summary>
        /// Override right-hand-side background color.
        /// </summary>
        public Color? RightColor { get; init; }

        /// <summary>
        /// Color mapping for <see cref="Status"/>.
        /// </summary>
        public IReadOnlyDictionary<Status, Color> ColorMap { get; init; }

        /// <summary>
        /// Creates an instance of <see cref="FlatBadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="statusMap">Status mapping for <see cref="Status"/>.</param>
        /// <param name="colorMap">Color mapping for <see cref="Status"/>.</param>
        /// <param name="leftColor">Left-hand-side color.</param>
        /// <param name="rightColor">Override right-hand-side color.</param>
        public FlatBadgeStyle(string fontFamily, int fontSizePts, int height,
            IReadOnlyDictionary<Status, string> statusMap, IReadOnlyDictionary<Status, Color> colorMap, Color leftColor,
            Color? rightColor = null)
            : base(fontFamily, fontSizePts, height, statusMap)
        {
            ColorMap = colorMap;
            LeftColor = leftColor;
            RightColor = rightColor;
        }

        /// <summary>
        /// Creates an instance of <see cref="FlatBadgeStyle"/> with the default style.
        /// </summary>
        public FlatBadgeStyle()
        {
            ColorMap = StandardColorMap;
            LeftColor = DefaultLeftColor;
            RightColor = null;
        }

        /// <inheritdoc />
        protected override Color GetLeftColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            IsLight(LeftColor) ? Color.Black : Color.White;

        /// <inheritdoc />
        protected override Color GetRightColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            IsLight(GetRightBackgroundColor(name, status, statusText, icon)) ? Color.Black : Color.White;

        /// <inheritdoc />
        protected override Color GetShadowColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            ShadowColor;

        /// <inheritdoc />
        protected override SvgElement GetLeftBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            new SvgRectangle {Width = width, Height = Height, Fill = new SvgColourServer(LeftColor)};

        /// <inheritdoc />
        protected override SvgElement GetRightBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            new SvgRectangle
            {
                Width = width,
                Height = Height,
                Fill = new SvgColourServer(GetRightBackgroundColor(name, status, statusText, icon))
            };

        /// <summary>
        /// Gets right background color.
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Right background color.</returns>
        protected virtual Color GetRightBackgroundColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            RightColor ??
            (ColorMap.TryGetValue(status, out var inColor) ? inColor : StandardColorMap[status]);

        private static bool IsLight(Color color) => color.R + color.G + color.B >= (128 + 64) * 3;
    }
}
