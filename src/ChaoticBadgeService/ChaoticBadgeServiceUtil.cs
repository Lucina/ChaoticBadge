#nullable enable
using System.IO;
using System.Text;
using System.Xml;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;

namespace ChaoticBadgeService
{
    public static class ChaoticBadgeServiceUtil
    {
        public static readonly FlatBadgeStyle DefaultStyle = FlatBadgeStyle.Default;
        public static readonly FlatBadgeStyle StupidStyle = DefaultStyle with {TypeMap = FlatBadgeStyle.StupidMap};

        public static FileContentResult Badge(BadgeStyle badgeStyle, ControllerBase controller, string label,
            Status status, string? statusText = null, string? customLeftColor = null, string? customRightColor = null)
        {
            var svg = badgeStyle.CreateSvg(label, status, statusText, customLeftColor, customRightColor);
            var ms = new MemoryStream();
            using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                svg.Write(writer);
            return controller.File(ms.ToArray(), "image/svg+xml; charset=utf-8", null);
        }
    }
}
