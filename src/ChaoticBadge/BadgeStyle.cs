using System.Collections.Generic;
using Svg;

namespace ChaoticBadge
{
    /// <summary>
    /// Represents a set of common configuration values for badge styling.
    /// </summary>
    public abstract record BadgeStyle
    {
        /// <summary>
        /// Default status mapping for <see cref="Status"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<Status, string> StandardStatusMap =
            new Dictionary<Status, string>
            {
                [Status.Passing] = "passing",
                [Status.Failing] = "failing",
                [Status.Unknown] = "unknown",
                [Status.Release] = "release",
                [Status.PreRelease] = "prerelease",
                [Status.NotFound] = "not found",
                [Status.Error] = "error"
            };

        /// <summary>
        /// Stupid status mapping for <see cref="Status"/> with text emotes.
        /// </summary>
        public static readonly IReadOnlyDictionary<Status, string> StupidStatusMap =
            new Dictionary<Status, string>
            {
                [Status.Passing] = "^.^",
                [Status.Failing] = ">_<",
                [Status.Unknown] = "@.@",
                [Status.Release] = ";D",
                [Status.PreRelease] = ":⦚",
                [Status.NotFound] = "._.?",
                [Status.Error] = "o.o?"
            };

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
        /// Status mapping for <see cref="Status"/>.
        /// </summary>
        public IReadOnlyDictionary<Status, string> StatusMap { get; init; }

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the default style.
        /// </summary>
        protected BadgeStyle()
        {
            FontFamily = "Verdana,Helvetica,sans-serif";
            FontSizePts = 9;
            Height = 20;
            StatusMap = StandardStatusMap;
        }

        /// <summary>
        /// Creates an instance of <see cref="BadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="statusMap">Status mapping for <see cref="Status"/>.</param>
        protected BadgeStyle(string fontFamily, int fontSizePts, int height,
            IReadOnlyDictionary<Status, string> statusMap)
        {
            FontFamily = fontFamily;
            FontSizePts = fontSizePts;
            Height = height;
            StatusMap = statusMap;
        }

        /// <summary>
        /// Creates an SVG badge.
        /// </summary>
        /// <param name="name">Badge name (left text).</param>
        /// <param name="status">Badge status.</param>
        /// <param name="statusText">Status text (right text).</param>
        /// <param name="icon">Custom icon.</param>
        /// <returns>Generated SVG.</returns>
        public abstract SvgDocument CreateSvg(string name, Status status, string? statusText = null,
            SvgDocument? icon = null);
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
