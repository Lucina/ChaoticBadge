using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Svg;

namespace ChaoticBadge
{
    /// <summary>
    /// Shattered effect badge.
    /// </summary>
    public record ShatterBadgeStyle : FlatBadgeStyle
    {
        /// <summary>
        /// Creates an instance of <see cref="ShatterBadgeStyle"/> with the specified style.
        /// </summary>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSizePts">Font size in points.</param>
        /// <param name="height">Height.</param>
        /// <param name="statusMap">Status mapping for <see cref="Status"/>.</param>
        /// <param name="colorMap">Color mapping for <see cref="Status"/>.</param>
        /// <param name="leftColor">Left-hand-side color.</param>
        /// <param name="rightColor">Override right-hand-side color.</param>
        public ShatterBadgeStyle(string fontFamily, int fontSizePts, int height,
            IReadOnlyDictionary<Status, string> statusMap, IReadOnlyDictionary<Status, Color> colorMap, Color leftColor,
            Color? rightColor = null)
            : base(fontFamily, fontSizePts, height, statusMap, colorMap, leftColor, rightColor)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="ShatterBadgeStyle"/> with the default style.
        /// </summary>
        public ShatterBadgeStyle()
        {
        }

        /// <inheritdoc />
        protected override SvgElement GetLeftBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            GetShatterBlock(width, Height, LeftColor);

        /// <inheritdoc />
        protected override SvgElement GetRightBlock(float width, string name, Status status, string? statusText = null,
            SvgGroup? icon = null) =>
            GetShatterBlock(width, Height, GetRightBackgroundColor(name, status, statusText, icon));

        private static Random Random => _random ??= new Random();
        [ThreadStatic] private static Random? _random;

        private const float _maxScaleWidth = 120;
        private const float _maxScaleHeight = 80;
        private const float _colorChuck = 64;
        private const float _baseSep = 15;

        private static SvgElement GetShatterBlock(float width, float height, Color color)
        {
            float mag = 3 + color.R + color.G + color.B;
            float top = Math.Min(mag + _colorChuck, 256 * 3);
            float bot = Math.Max(mag - _colorChuck, 0);
            float scaleTop = top / mag;
            float scaleBot = bot / mag;

            float fx;
            float fy;
            if (width > height)
            {
                fx = Math.Min(width, _maxScaleWidth);
                fy = Math.Min(height, _maxScaleWidth * height / width);
            }
            else
            {
                fy = Math.Min(height, _maxScaleHeight);
                fx = Math.Min(width, _maxScaleWidth * width / height);
            }

            var (verts, tris) = GenerateTriSet(fx, fy, _baseSep);
            var group = new SvgGroup();
            group.Children.Add(new SvgRectangle {Width = width, Height = height, Fill = new SvgColourServer(color)});
            foreach ((int v1, int v2, int v3) in tris)
            {
                double s = Random.NextDouble() * (scaleTop - scaleBot) + scaleBot;
                int r = (int)Math.Min(255, (color.R + 1) * s);
                int g = (int)Math.Min(255, (color.G + 1) * s);
                int b = (int)Math.Min(255, (color.B + 1) * s);
                var c = Color.FromArgb(r, g, b);
                var points = new SvgPointCollection
                {
                    verts[v1].X * width / fx,
                    verts[v1].Y * height / fy,
                    verts[v2].X * width / fx,
                    verts[v2].Y * height / fy,
                    verts[v3].X * width / fx,
                    verts[v3].Y * height / fy
                };
                group.Children.Add(new SvgPolygon {Points = points, Fill = new SvgColourServer(c)});
            }

            return group;
        }

        private static TriSet GenerateTriSet(float width, float height, float sep)
        {
            var verts = new List<Vertex>();
            /*verts.AddRange(new[] {(0, 0), (width, 0), (0, height), (width, height)});
            tris.AddRange(new[] {(0, 1, 3), (0, 2, 3)});*/
            float sMin = 0.5f * sep, sMax = 1.5f * sep;
            // border
            for (float x = 0; x < width; x += (sMax - sMin) * (float)Random.NextDouble() + sMin)
            {
                verts.Add(new Vertex(x, 0));
                verts.Add(new Vertex(x, height));
            }

            for (float y = 0; y < height; y += (sMax - sMin) * (float)Random.NextDouble() + sMin)
            {
                verts.Add(new Vertex(0, y));
                verts.Add(new Vertex(width, y));
            }

            verts.Add(new Vertex(0, height));
            verts.Add(new Vertex(width, 0));
            verts.Add(new Vertex(width, height));

            int c = (int)Math.Ceiling(width * height / (sep * sep));
            for (int i = 0; i < c; i++)
                verts.Add(new Vertex(width * (float)Random.NextDouble(), height * (float)Random.NextDouble()));

            var tris = BowyerWatson2d(verts);
            return new TriSet(verts, tris);
        }

        private static readonly float _tanPiOver3 = (float)Math.Tan(Math.PI / 3);
        private static readonly float _cosPiOver3 = (float)Math.Cos(Math.PI / 3);

        private static HashSet<Triangle> BowyerWatson2d(List<Vertex> points)
        {
            var tris = new HashSet<Triangle>();
            // super-triangle
            float minX = points.Select(v => v.X).Min();
            float maxX = points.Select(v => v.X).Max();
            float minY = points.Select(v => v.Y).Min();
            float maxY = points.Select(v => v.Y).Max();
            float centerX = (maxX - minX) / 2 + minX, centerY = (maxY - minY) / 2 + minY;
            float r = (float)Math.Sqrt((maxX - minX) * (maxX - minX) + (maxY - minY) * (maxY - minY)) / 2;
            float halfBottom = r * _tanPiOver3;
            float top = r / _cosPiOver3;
            points.Insert(0, new Vertex(centerX, centerY + top));
            points.Insert(1, new Vertex(centerX - halfBottom, centerY - r));
            points.Insert(2, new Vertex(centerX + halfBottom, centerY - r));
            tris.Add(new Triangle(0, 1, 2));
            var badTriangles = new HashSet<Triangle>();
            var pol = new List<Edge>();
            for (int c = 3; c < points.Count; c++)
            {
                var point = points[c];
                badTriangles.Clear();
                badTriangles.UnionWith(tris.Where(tri => tri.InCircumcircle(points, point)));
                pol.Clear();
                foreach (var tri in badTriangles)
                foreach (var edge in tri.GetEdges())
                    if (badTriangles.All(tOther => tOther.ValueEquals(tri) || !tOther.HasEdge(edge)))
                        pol.Add(edge);
                tris.ExceptWith(badTriangles);
                foreach ((int a, int b) in pol)
                {
                    var tri = new Triangle(a, b, c);
                    tris.Add(tri.IsCounterClockwise(points) ? tri : new Triangle(b, a, c));
                }
            }

            tris.RemoveWhere(tri => tri.HasSuperTriangleVertex);
            return tris;
        }


        /*
         https://stackoverflow.com/questions/39984709/how-can-i-check-wether-a-point-is-inside-the-circumcircle-of-3-points

         Bowyer-Watson algorithm implementation: O(n^2)

function BowyerWatson (pointList)
   // pointList is a set of coordinates defining the points to be triangulated
   triangulation := empty triangle mesh data structure
   add super-triangle to triangulation // must be large enough to completely contain all the points in pointList
   for each point in pointList do // add all the points one at a time to the triangulation
      badTriangles := empty set
      for each triangle in triangulation do // first find all the triangles that are no longer valid due to the insertion
         if point is inside circumcircle of triangle
            add triangle to badTriangles
      polygon := empty set
      for each triangle in badTriangles do // find the boundary of the polygonal hole
         for each edge in triangle do
            if edge is not shared by any other triangles in badTriangles
               add edge to polygon
      for each triangle in badTriangles do // remove them from the data structure
         remove triangle from triangulation
      for each edge in polygon do // re-triangulate the polygonal hole
         newTri := form a triangle from edge to point
         add newTri to triangulation
   for each triangle in triangulation // done inserting points, now clean up
      if triangle contains a vertex from original super-triangle
         remove triangle from triangulation
   return triangulation
         */

        internal readonly struct Vertex
        {
            internal readonly float X;
            internal readonly float Y;

            internal Vertex(float x, float y)
            {
                X = x;
                Y = y;
            }

            internal void Deconstruct(out float x, out float y)
            {
                x = X;
                y = Y;
            }

            public override string ToString() => $"({X}, {Y})";
        }

        internal readonly struct Triangle
        {
            internal readonly int P1;
            internal readonly int P2;
            internal readonly int P3;

            internal Triangle(int p1, int p2, int p3)
            {
                P1 = p1;
                P2 = p2;
                P3 = p3;
            }

            internal bool ValueEquals(Triangle other) => P1 == other.P1 && P2 == other.P2 && P3 == other.P3;

            internal void Deconstruct(out int p1, out int p2, out int p3)
            {
                p1 = P1;
                p2 = P2;
                p3 = P3;
            }

            internal bool HasEdge(Edge other)
            {
                (int p1, int p2) = other;
                if (p1 == P1 && p2 == P2 || p1 == P2 && p2 == P1) return true;
                if (p1 == P2 && p2 == P3 || p1 == P3 && p2 == P2) return true;
                if (p1 == P3 && p2 == P1 || p1 == P1 && p2 == P3) return true;
                return false;
            }

            internal bool InCircumcircle(IReadOnlyList<Vertex> vertices, Vertex vertex)
            {
                (float x, float y) = vertex;
                float ax = vertices[P1].X - x;
                float ay = vertices[P1].Y - y;
                float bx = vertices[P2].X - x;
                float by = vertices[P2].Y - y;
                float cx = vertices[P3].X - x;
                float cy = vertices[P3].Y - y;
                return (ax * ax + ay * ay) * (bx * cy - cx * by) - (bx * bx + by * by) * (ax * cy - cx * ay) +
                    (cx * cx + cy * cy) * (ax * by - bx * ay) > 0;
            }

            internal bool HasSuperTriangleVertex => P1 < 3 || P2 < 3 || P3 < 3;

            internal bool IsCounterClockwise(List<Vertex> vertices)
            {
                (float ax, float ay) = vertices[P1];
                (float bx, float by) = vertices[P2];
                (float cx, float cy) = vertices[P3];
                return (bx - ax) * (cy - ay) - (cx - ax) * (@by - ay) > 0;
            }

            internal IEnumerable<Edge> GetEdges()
            {
                yield return new Edge(P1, P2);
                yield return new Edge(P2, P3);
                yield return new Edge(P3, P1);
            }

            public override int GetHashCode() => (P1 << 16) ^ (P1 >> 16) ^ (P2 << 24) ^ (P2 >> 8) ^ P3;

            public override string ToString() => $"({P1}, {P2}, {P3})";
        }

        internal readonly struct Edge
        {
            internal readonly int P1;
            internal readonly int P2;

            internal Edge(int p1, int p2)
            {
                P1 = p1;
                P2 = p2;
            }

            internal void Deconstruct(out int p1, out int p2)
            {
                p1 = P1;
                p2 = P2;
            }

            public override int GetHashCode() => (P1 << 16) ^ (P1 >> 16) ^ (P2 << 24) ^ (P2 >> 8);
        }

        internal record TriSet(IReadOnlyList<Vertex> verts, IReadOnlyCollection<Triangle> tris);
    }
}
