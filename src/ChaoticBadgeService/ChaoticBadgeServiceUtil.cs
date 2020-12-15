#nullable enable
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;

namespace ChaoticBadgeService
{
    public static class ChaoticBadgeServiceUtil
    {
        public static FileContentResult Badge(ControllerBase controller, string label, BadgeType badgeType,
            string? status = null, string? customLeftColor = null, string? customRightColor = null,
            IReadOnlyDictionary<BadgeType, (string, Color)>? typeMasp = null)
        {
            var svg = Badges.CreateSvg(label, badgeType, status, customLeftColor, customRightColor, typeMasp);
            var ms = new MemoryStream();
            using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                svg.Write(writer);
            return controller.File(ms.ToArray(), "image/svg+xml; charset=utf-8", null);
        }
    }
}
