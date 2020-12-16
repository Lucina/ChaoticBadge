#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using ChaoticBadge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Svg;

namespace ChaoticBadgeService
{
    public static class ChaoticBadgeServiceUtil
    {
        public static readonly FlatBadgeStyle DefaultStyle = new ShatterBadgeStyle();
        public static readonly FlatBadgeStyle StupidStyle = DefaultStyle with {StatusMap = BadgeStyle.StupidStatusMap};
        private static readonly Dictionary<string, (Color color, SvgDocument icon)?> _icons = new();

        public static FileContentResult Badge(BadgeServiceControllerBase controller, string label, Status status,
            bool? stupid = null, int? height = null, int? fontSize = null, string? statusText = null,
            string? leftColor = null, string? rightColor = null, string? icon = null)
        {
            BadgeStyle badgeStyle = stupid ?? false ? StupidStyle : DefaultStyle;

            if (height > 4)
                badgeStyle = badgeStyle with {Height = height.Value};
            if (fontSize > 4)
                badgeStyle = badgeStyle with {FontSizePts = fontSize.Value};

            GetColors(leftColor, out var customLeftColor);
            GetColors(rightColor, out var customRightColor);

            (Color color, SvgDocument icon)? iconValue = default;
            if (icon != null)
                _icons.TryGetValue(icon, out iconValue);
            SvgDocument? iconIcon = iconValue?.icon;
            customLeftColor ??= iconValue?.color;

            if (badgeStyle is FlatBadgeStyle flat)
            {
                if (customLeftColor != null)
                    flat = flat with {LeftColor = customLeftColor.Value};
                if (customRightColor != null)
                    flat = flat with {RightColor = customRightColor};
                badgeStyle = flat;
            }

            var svg = badgeStyle.CreateSvg(label, status, statusText, iconIcon);
            var ms = new MemoryStream();
            using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                svg.Write(writer);
            return controller.File(ms.ToArray(), "image/svg+xml; charset=utf-8", null);
        }

        public static void InitIcons(IFileProvider fileProvider)
        {
            Console.WriteLine("Load icon set");
            using var stream = fileProvider.GetFileInfo("assets/simple-icons.json").CreateReadStream();
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var json = new Utf8JsonReader(ms.ToArray());
            json.Read();
            if (json.TokenType != JsonTokenType.StartObject) throw new ApplicationException("Invalid icon meta file");
            while (json.Read())
                if (json.GetString() != "icons") json.Skip();
                else break;
            json.Read();
            if (json.TokenType != JsonTokenType.StartArray) throw new ApplicationException("Invalid icon meta file");
            while (json.Read() && json.TokenType != JsonTokenType.EndArray)
            {
                if (json.TokenType != JsonTokenType.StartObject)
                    throw new ApplicationException("Invalid icon meta file");
                string? title = null;
                string? color = null;
                while (json.Read() && json.TokenType != JsonTokenType.EndObject)
                {
                    string? name = json.GetString();
                    json.Read();
                    switch (name)
                    {
                        case "title":
                            title = json.GetString();
                            break;
                        case "hex":
                            color = json.GetString();
                            break;
                        default:
                            json.Skip();
                            break;
                    }
                }

                if (title == null || color == null) throw new ApplicationException("Invalid icon meta file");
                string icon = NameToFilename(title);
                SvgDocument iconDocument;

                Console.WriteLine(icon);
                var iconInfo = fileProvider.GetFileInfo($"assets/{icon}.svg");
                if (iconInfo.Exists)
                {
                    using var stream2 = iconInfo.CreateReadStream();
                    iconDocument = SvgDocument.Open<SvgDocument>(stream2);
                }
                else continue;

                GetColors(color, out var colorValue);
                if (colorValue == null) continue;
                _icons[icon] = (colorValue.Value, iconDocument);
            }

            Console.WriteLine($"{_icons.Count} icons available");
        }

        //https://github.com/simple-icons/simple-icons/blob/develop/scripts/utils.js
        private static readonly (Regex regex, string replacement)[] _regexes =
        {
            (new Regex(@"\+"), "plus"), (new Regex(@"^\."), "dot-"), (new Regex(@"\.$"), "-dot"),
            (new Regex(@"\."), "-dot-"), (new Regex(@"^&"), "and-"), (new Regex(@"&$"), "-and"),
            (new Regex(@"&"), "-and-"), (new Regex(@"[ !:’']"), ""), (new Regex(@"à|á|â|ã|ä"), "a"),
            (new Regex(@"ç|č|ć"), "c"), (new Regex(@"è|é|ê|ë"), "e"), (new Regex(@"ì|í|î|ï"), "i"),
            (new Regex(@"ñ|ň|ń"), "n"), (new Regex(@"ò|ó|ô|õ|ö"), "o"), (new Regex(@"š|ś"), "s"),
            (new Regex(@"ù|ú|û|ü"), "u"), (new Regex(@"ý|ÿ"), "y"), (new Regex(@"ž|ź"), "z"),
        };

        private static string NameToFilename(string name)
        {
            name = name.ToLowerInvariant();
            foreach (var (regex, replacement) in _regexes)
                name = regex.Replace(name, replacement);
            return name;
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
