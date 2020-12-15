using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Svg;
using Svg.Transforms;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a flat-styled badge.
    /// </summary>
    public record FlatBadgeStyle : BadgeStyle
    {
        /// <summary>
        /// Default text color.
        /// </summary>
        public static readonly Color TextColor = Color.FromArgb(0xff, 0xff, 0xff);

        /// <summary>
        /// Default text shadow color.
        /// </summary>
        public static readonly Color TextShadowColor = Color.FromArgb(0x20, 0x20, 0x20);

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
        public static readonly IReadOnlyDictionary<Status, (string status, Color statusColor)> StandardMap =
            new Dictionary<Status, (string status, Color statusColor)>
            {
                [Status.Passing] = ("passing", PassingRightColor),
                [Status.Failing] = ("failing", FailingRightColor),
                [Status.Unknown] = ("unknown", UnknownRightColor),
                [Status.Release] = ("release", PassingRightColor),
                [Status.PreRelease] = ("prerelease", UnknownRightColor),
                [Status.NotFound] = ("not found", FailingRightColor),
                [Status.Error] = ("error", FailingRightColor)
            };

        /// <summary>
        /// Stupid mapping for <see cref="Status"/> with text emotes.
        /// </summary>
        public static readonly IReadOnlyDictionary<Status, (string status, Color statusColor)> StupidMap =
            new Dictionary<Status, (string status, Color statusColor)>
            {
                [Status.Passing] = ("^.^", PassingRightColor),
                [Status.Failing] = (">_<", FailingRightColor),
                [Status.Unknown] = ("@.@", UnknownRightColor),
                [Status.Release] = (";D", PassingRightColor),
                [Status.PreRelease] = (":⦚", UnknownRightColor),
                [Status.NotFound] = ("._.?", FailingRightColor),
                [Status.Error] = ("o.o?", FailingRightColor)
            };

        /// <summary>
        /// Singleton instance of default style.
        /// </summary>
        public static readonly FlatBadgeStyle Default = new FlatBadgeStyle();

        /// <summary>
        /// Mapping for <see cref="Status"/>.
        /// </summary>
        public IReadOnlyDictionary<Status, (string status, Color statusColor)> TypeMap { get; init; }

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="border">Border size.</param>
        /// <param name="typeMap">Mapping for <see cref="Status"/>.</param>
        public FlatBadgeStyle(string fontFamily, int fontSizePts, int height, int border,
            IReadOnlyDictionary<Status, (string status, Color statusColor)> typeMap)
            : base(fontFamily, fontSizePts, height, border)
        {
            TypeMap = typeMap;
        }

        /// <summary>
        /// Creates an instance of <see cref="FlatBadgeStyle"/> with the default style.
        /// </summary>
        public FlatBadgeStyle()
        {
            TypeMap = StandardMap;
        }

        /// <inheritdoc />
        public override SvgDocument CreateSvg(string name, Status status, string? statusText = null,
            string? customLeftColor = null, string? customRightColor = null)
        {
            GetColors(customLeftColor, out var leftColor);
            GetColors(customRightColor, out var rightColor);
            leftColor ??= DefaultLeftColor;
            bool resAvailable = TypeMap.TryGetValue(status, out var res);
            statusText ??= (resAvailable ? res : StandardMap[Status.Error]).Item1;
            rightColor ??= (resAvailable ? res : StandardMap[Status.Error]).Item2;

            var svg = new SvgDocument {Height = Height};
            var group = new SvgGroup();
            group.FontFamily = FontFamily;
            group.FontSize = FontSizeSvgUnits;
            svg.Children.Add(group);

            #region Left

            float leftTextWidth =
                new SvgText(name) {FontFamily = FontFamily, FontSize = FontSizeSvgUnits}
                    .Bounds
                    .Width;
            var labelTextElement =
                new SvgText(name) {Fill = new SvgColourServer(TextColor), Y = {TextOffsetY}, X = {Border}};
            var labelShadowTextElement =
                new SvgText(name) {Fill = new SvgColourServer(TextShadowColor), Y = {TextShadowOffsetY}, X = {Border}};
            var leftBlock = new SvgRectangle
            {
                Width = leftTextWidth + Border * 2, Height = Height, Fill = new SvgColourServer(leftColor.Value)
            };
            group.Children.Add(leftBlock);
            group.Children.Add(labelShadowTextElement);
            group.Children.Add(labelTextElement);

            #endregion

            #region Right

            float rightTextWidth =
                new SvgText(statusText) {FontFamily = FontFamily, FontSize = FontSizeSvgUnits}
                    .Bounds.Width;
            var statusTextElement = new SvgText(statusText)
            {
                Fill = new SvgColourServer(TextColor), Y = {TextOffsetY}, X = {Border}
            };
            var statusShadowTextElement =
                new SvgText(statusText)
                {
                    Fill = new SvgColourServer(TextShadowColor), Y = {TextShadowOffsetY}, X = {Border}
                };
            var rightBlock = new SvgRectangle
            {
                Width = rightTextWidth + Border * 2, Height = Height, Fill = new SvgColourServer(rightColor.Value)
            };
            var rightGroup = new SvgGroup {Transforms = new SvgTransformCollection {new SvgTranslate(leftBlock.Width)}};
            group.Children.Add(rightGroup);
            rightGroup.Children.Add(rightBlock);
            rightGroup.Children.Add(statusShadowTextElement);
            rightGroup.Children.Add(statusTextElement);

            #endregion

            return svg;
        }

        private static void GetColors(string? customColor, out Color? color)
        {
            if (customColor != null)
                try
                {
                    color = Color.FromArgb(unchecked((int)0xff000000) | int.Parse(customColor, NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture));
                }
                catch
                {
                    color = null;
                }
            else
                color = null;
        }
    }
}
