#nullable enable
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Svg;
using Svg.Transforms;

namespace ChaoticBadge
{
    /// <summary>
    /// Utility class containing <see cref="CreateSvg"/>.
    /// </summary>
    public static class Badges
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
        /// Default mapping for <see cref="BadgeType"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<BadgeType, (string, Color)> StandardMap =
            new Dictionary<BadgeType, (string, Color)>
            {
                [BadgeType.Passing] = ("passing", PassingRightColor),
                [BadgeType.Failing] = ("failing", FailingRightColor),
                [BadgeType.Unknown] = ("unknown", UnknownRightColor),
                [BadgeType.Release] = ("release", PassingRightColor),
                [BadgeType.PreRelease] = ("prerelease", UnknownRightColor),
                [BadgeType.NotFound] = ("not found", FailingRightColor),
                [BadgeType.Error] = ("error", FailingRightColor)
            };

        /// <summary>
        /// Stupid mapping for <see cref="BadgeType"/> with text emotes.
        /// </summary>
        public static readonly IReadOnlyDictionary<BadgeType, (string, Color)> StupidMap =
            new Dictionary<BadgeType, (string, Color)>
            {
                [BadgeType.Passing] = ("^.^", PassingRightColor),
                [BadgeType.Failing] = (">_<", FailingRightColor),
                [BadgeType.Unknown] = ("@.@", UnknownRightColor),
                [BadgeType.Release] = (";D", PassingRightColor),
                [BadgeType.PreRelease] = (":⦚", UnknownRightColor),
                [BadgeType.NotFound] = ("._.?", FailingRightColor),
                [BadgeType.Error] = ("o.o?", FailingRightColor)
            };

        /// <summary>
        /// Creates an SVG badge.
        /// </summary>
        /// <param name="label">Text label (left).</param>
        /// <param name="badgeType">Badge type.</param>
        /// <param name="status">Status (default from <paramref name="typeMap"/>).</param>
        /// <param name="customLeftColor">Custom left-hand-side color.</param>
        /// <param name="customRightColor">Custom right-hand-side (default from <paramref name="typeMap"/>).</param>
        /// <param name="typeMap">Mapping for <see cref="BadgeType"/>.</param>
        /// <param name="badgeStyle">Badge style.</param>
        /// <returns>Generated SVG.</returns>
        public static SvgDocument CreateSvg(string label, BadgeType badgeType, string? status = null,
            string? customLeftColor = null, string? customRightColor = null,
            IReadOnlyDictionary<BadgeType, (string, Color)>? typeMap = null, BadgeStyle? badgeStyle = null)
        {
            badgeStyle ??= BadgeStyle.Default;
            GetColors(customLeftColor, customRightColor, out var leftColor, out var rightColor);
            leftColor ??= DefaultLeftColor;
            bool resAvailable = (typeMap ?? StandardMap).TryGetValue(badgeType, out var res);
            status ??= (resAvailable ? res : StandardMap[BadgeType.Error]).Item1;
            rightColor ??= (resAvailable ? res : StandardMap[BadgeType.Error]).Item2;

            var svg = new SvgDocument {Height = badgeStyle.Height};
            var group = new SvgGroup();
            group.FontFamily = badgeStyle.FontFamily;
            group.FontSize = badgeStyle.FontSizeSvgUnits;
            svg.Children.Add(group);

            #region Left

            float leftTextWidth =
                new SvgText(label) {FontFamily = badgeStyle.FontFamily, FontSize = badgeStyle.FontSizeSvgUnits}
                    .Bounds
                    .Width;
            var labelText = new SvgText(label)
            {
                Fill = new SvgColourServer(TextColor), Y = {badgeStyle.TextOffsetY}, X = {badgeStyle.Border}
            };
            var labelShadowText =
                new SvgText(label)
                {
                    Fill = new SvgColourServer(TextShadowColor),
                    Y = {badgeStyle.TextShadowOffsetY},
                    X = {badgeStyle.Border}
                };
            var leftBlock = new SvgRectangle
            {
                Width = leftTextWidth + badgeStyle.Border * 2,
                Height = badgeStyle.Height,
                Fill = new SvgColourServer(leftColor.Value)
            };
            group.Children.Add(leftBlock);
            group.Children.Add(labelShadowText);
            group.Children.Add(labelText);

            #endregion

            #region Right

            float rightTextWidth =
                new SvgText(status) {FontFamily = badgeStyle.FontFamily, FontSize = badgeStyle.FontSizeSvgUnits}
                    .Bounds.Width;
            var statusText = new SvgText(status)
            {
                Fill = new SvgColourServer(TextColor), Y = {badgeStyle.TextOffsetY}, X = {badgeStyle.Border}
            };
            var statusShadowText =
                new SvgText(status)
                {
                    Fill = new SvgColourServer(TextShadowColor),
                    Y = {badgeStyle.TextShadowOffsetY},
                    X = {badgeStyle.Border}
                };
            var rightBlock = new SvgRectangle
            {
                Width = rightTextWidth + badgeStyle.Border * 2,
                Height = badgeStyle.Height,
                Fill = new SvgColourServer(rightColor.Value)
            };
            var rightGroup = new SvgGroup {Transforms = new SvgTransformCollection {new SvgTranslate(leftBlock.Width)}};
            group.Children.Add(rightGroup);
            rightGroup.Children.Add(rightBlock);
            rightGroup.Children.Add(statusShadowText);
            rightGroup.Children.Add(statusText);

            #endregion

            return svg;
        }

        private static void GetColors(string? customLeftColor, string? customRightColor, out Color? leftColor,
            out Color? rightColor)
        {
            if (customLeftColor != null)
                try
                {
                    leftColor = Color.FromArgb(unchecked((int)0xff000000) | int.Parse(customLeftColor,
                        NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
                catch
                {
                    leftColor = null;
                }
            else
                leftColor = null;

            if (customRightColor != null)
                try
                {
                    rightColor = Color.FromArgb(unchecked((int)0xff000000) | int.Parse(customRightColor,
                        NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
                catch
                {
                    rightColor = null;
                }
            else
                rightColor = null;
        }
    }
}
