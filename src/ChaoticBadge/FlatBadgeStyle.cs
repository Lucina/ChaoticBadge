using System.Collections.Generic;
using System.Drawing;
using Svg;
using Svg.Transforms;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a flat badge style.
    /// </summary>
    public record FlatBadgeStyle : BadgeStyle
    {
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
        /// Left-hand-side color.
        /// </summary>
        public Color LeftColor { get; init; }

        /// <summary>
        /// Override right-hand-side color.
        /// </summary>
        public Color? RightColor { get; init; }

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
        /// <param name="typeMap">Mapping for <see cref="Status"/>.</param>
        /// <param name="leftColor">Left-hand-side color.</param>
        /// <param name="rightColor">Override right-hand-side color.</param>
        public FlatBadgeStyle(string fontFamily, int fontSizePts, int height,
            IReadOnlyDictionary<Status, (string status, Color statusColor)> typeMap, Color leftColor,
            Color? rightColor = null)
            : base(fontFamily, fontSizePts, height)
        {
            TypeMap = typeMap;
            LeftColor = leftColor;
            RightColor = rightColor;
        }

        /// <summary>
        /// Creates an instance of <see cref="FlatBadgeStyle"/> with the default style.
        /// </summary>
        public FlatBadgeStyle()
        {
            TypeMap = StandardMap;
            LeftColor = DefaultLeftColor;
            RightColor = null;
        }

        /// <inheritdoc />
        public override SvgDocument CreateSvg(string name, Status status, string? statusText = null,
            SvgDocument? icon = null)
        {
            bool resAvailable = TypeMap.TryGetValue(status, out var res);
            statusText ??= (resAvailable ? res : StandardMap[Status.Error]).Item1;
            var rightColor = RightColor ?? (resAvailable ? res : StandardMap[Status.Error]).Item2;

            var svg = new SvgDocument {Height = Height};
            var group = new SvgGroup();
            group.FontFamily = FontFamily;
            group.FontSize = FontSizeSvgUnits;
            svg.Children.Add(group);
            var leftBaseColor = IsLight(LeftColor) ? Color.Black : Color.White;
            var rightBaseColor = IsLight(rightColor) ? Color.Black : Color.White;

            #region Left

            float iconTranslate = 0;
            SvgGroup? iconGroup = null;
            if (icon != null)
            {
                var bounds = icon.Bounds;
                float iconScale = Height / bounds.Height;
                iconTranslate = iconScale * bounds.Width + bounds.Left;
                iconGroup = new SvgGroup
                {
                    Fill = new SvgColourServer(leftBaseColor),
                    Transforms = new SvgTransformCollection
                    {
                        new SvgScale(iconScale, iconScale), new SvgTranslate(-bounds.Left, -bounds.Top)
                    }
                };
                foreach (var e in icon.Children) iconGroup.Children.Add(e);
            }

            float leftTextWidth =
                new SvgText(name) {FontFamily = FontFamily, FontSize = FontSizeSvgUnits}
                    .Bounds
                    .Width;
            var labelTextElement =
                new SvgText(name)
                {
                    Fill = new SvgColourServer(leftBaseColor),
                    Y = {TextOffsetY},
                    X = {Border},
                    Transforms = new SvgTransformCollection {new SvgTranslate(iconTranslate)}
                };
            var labelShadowTextElement =
                new SvgText(name)
                {
                    Fill = new SvgColourServer(TextShadowColor),
                    Y = {TextShadowOffsetY},
                    X = {Border},
                    Transforms = new SvgTransformCollection {new SvgTranslate(iconTranslate)}
                };
            var leftBlock = new SvgRectangle
            {
                Width = iconTranslate + leftTextWidth + Border * 2,
                Height = Height,
                Fill = new SvgColourServer(LeftColor),
            };
            var leftGroup = new SvgGroup();
            group.Children.Add(leftGroup);
            leftGroup.Children.Add(leftBlock);
            if (iconGroup != null)
                leftGroup.Children.Add(iconGroup);
            leftGroup.Children.Add(labelShadowTextElement);
            leftGroup.Children.Add(labelTextElement);

            #endregion

            #region Right

            float rightTextWidth =
                new SvgText(statusText) {FontFamily = FontFamily, FontSize = FontSizeSvgUnits}
                    .Bounds.Width;
            var statusTextElement = new SvgText(statusText)
            {
                Fill = new SvgColourServer(rightBaseColor), Y = {TextOffsetY}, X = {Border}
            };
            var statusShadowTextElement =
                new SvgText(statusText)
                {
                    Fill = new SvgColourServer(TextShadowColor), Y = {TextShadowOffsetY}, X = {Border}
                };
            var rightBlock = new SvgRectangle
            {
                Width = rightTextWidth + Border * 2, Height = Height, Fill = new SvgColourServer(rightColor)
            };
            var rightGroup = new SvgGroup {Transforms = new SvgTransformCollection {new SvgTranslate(leftBlock.Width)}};
            group.Children.Add(rightGroup);
            rightGroup.Children.Add(rightBlock);
            rightGroup.Children.Add(statusShadowTextElement);
            rightGroup.Children.Add(statusTextElement);

            #endregion

            return svg;
        }

        private static bool IsLight(Color color) => color.R + color.G + color.B >= (128 + 64) * 3;
    }
}
