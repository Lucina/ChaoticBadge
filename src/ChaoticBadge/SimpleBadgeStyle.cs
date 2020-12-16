using System.Collections.Generic;
using System.Drawing;
using Svg;
using Svg.Transforms;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a simple badge style with a fixed layout.
    /// </summary>
    public abstract record SimpleBadgeStyle : BadgeStyle
    {
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
        /// Creates an instance of <see cref="SimpleBadgeStyle"/> with the default style.
        /// </summary>
        protected SimpleBadgeStyle()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="SimpleBadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="statusMap">Status mapping for <see cref="Status"/>.</param>
        protected SimpleBadgeStyle(string fontFamily, int fontSizePts, int height,
            IReadOnlyDictionary<Status, string> statusMap) : base(fontFamily, fontSizePts, height, statusMap)
        {
        }

        /// <inheritdoc />
        public sealed override SvgDocument CreateSvg(string name, Status status, string? statusText = null,
            SvgDocument? icon = null)
        {
            statusText ??= StatusMap.TryGetValue(status, out var inStatus) ? inStatus : StandardStatusMap[status];
            var svg = new SvgDocument {Height = Height};
            var group = new SvgGroup();
            group.FontFamily = FontFamily;
            group.FontSize = FontSizeSvgUnits;
            svg.Children.Add(group);

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
                    Transforms = new SvgTransformCollection
                    {
                        new SvgScale(iconScale, iconScale), new SvgTranslate(-bounds.Left, -bounds.Top)
                    }
                };
                foreach (var e in icon.Children) iconGroup.Children.Add(e);
            }

            var leftColor = GetLeftColor(name, status, statusText, iconGroup);
            var rightColor = GetRightColor(name, status, statusText, iconGroup);
            var shadowColor = GetShadowColor(name, status, statusText, iconGroup);
            if (iconGroup != null) iconGroup.Fill = new SvgColourServer(leftColor);

            float leftTextWidth =
                new SvgText(name) {FontFamily = FontFamily, FontSize = FontSizeSvgUnits}
                    .Bounds
                    .Width;
            var labelTextElement =
                new SvgText(name)
                {
                    Fill = new SvgColourServer(leftColor),
                    Y = {TextOffsetY},
                    X = {Border},
                    Transforms = new SvgTransformCollection {new SvgTranslate(iconTranslate)}
                };
            var labelShadowTextElement =
                new SvgText(name)
                {
                    Fill = new SvgColourServer(shadowColor),
                    Y = {TextShadowOffsetY},
                    X = {Border},
                    Transforms = new SvgTransformCollection {new SvgTranslate(iconTranslate)}
                };
            float leftWidth = iconTranslate + leftTextWidth + Border * 2;
            var leftBlock = GetLeftBlock(leftWidth, name, status, statusText, iconGroup);
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
                Fill = new SvgColourServer(rightColor), Y = {TextOffsetY}, X = {Border}
            };
            var statusShadowTextElement =
                new SvgText(statusText)
                {
                    Fill = new SvgColourServer(shadowColor), Y = {TextShadowOffsetY}, X = {Border}
                };
            var rightBlock = GetRightBlock(rightTextWidth + Border * 2, name, status, statusText, iconGroup);
            var rightGroup = new SvgGroup {Transforms = new SvgTransformCollection {new SvgTranslate(leftWidth)}};
            group.Children.Add(rightGroup);
            rightGroup.Children.Add(rightBlock);
            rightGroup.Children.Add(statusShadowTextElement);
            rightGroup.Children.Add(statusTextElement);

            #endregion

            return svg;
        }

        /// <summary>
        /// Gets left base color (text and icon).
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Left base color.</returns>
        protected abstract Color GetLeftColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null);

        /// <summary>
        /// Gets right base color (text).
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Right base color.</returns>
        protected abstract Color GetRightColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null);

        /// <summary>
        /// Gets shadow color (text).
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Shadow color.</returns>
        protected abstract Color GetShadowColor(string name, Status status, string? statusText = null,
            SvgGroup? icon = null);

        /// <summary>
        /// Gets left background element.
        /// </summary>
        /// <param name="width">Element width.</param>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Left background element.</returns>
        protected abstract SvgElement GetLeftBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null);

        /// <summary>
        /// Right background element.
        /// </summary>
        /// <param name="width">Element width.</param>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Right background element.</returns>
        protected abstract SvgElement GetRightBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null);
    }
}
