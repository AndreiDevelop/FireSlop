using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Orientation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OrientationPhoneGraphic : MaskableGraphic
    {
        [SerializeField] private Color32 _portraitOutline = new Color32(197, 201, 205, 255);
        [SerializeField] private Color32 _landscapeFill = new Color32(246, 247, 248, 255);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = rectTransform.rect;
            var scale = Mathf.Min(bounds.width / 600f, bounds.height / 600f);
            var center = bounds.center;
            DrawPhone(mesh, center, scale, new Vector2(0f, 0f), new Vector2(246f, 500f), _portraitOutline);
            DrawPhone(mesh, center, scale, new Vector2(0f, -38f), new Vector2(500f, 222f), _landscapeFill, true);
            DrawPhone(mesh, center, scale, new Vector2(0f, -38f), new Vector2(500f, 222f), color);
            DrawCircle(mesh, center + new Vector2(-225f, -38f) * scale, 7f * scale, color);
            DrawCircle(mesh, center + new Vector2(0f, 224f) * scale, 6f * scale, _portraitOutline);
            var previous = new Vector2(150f, 215f);
            for (var step = 1; step <= 32; step++)
            {
                var angle = Mathf.Lerp(90f, 12f, step / 32f) * Mathf.Deg2Rad;
                var point = new Vector2(150f + Mathf.Cos(angle) * 112f, 103f + Mathf.Sin(angle) * 112f);
                DrawLine(mesh, center + previous * scale, center + point * scale, 13f * scale, color);
                previous = point;
            }

            AddTriangle(mesh, center + new Vector2(238f, 134f) * scale,
                center + new Vector2(285f, 140f) * scale, center + new Vector2(267f, 95f) * scale, color);
        }

        private static void DrawPhone(VertexHelper mesh, Vector2 center, float scale, Vector2 offset,
            Vector2 size, Color tint, bool fill = false)
        {
            const float Radius = 34f;
            var half = size * 0.5f;
            var previous = Vector2.zero;
            var first = Vector2.zero;
            for (var corner = 0; corner < 4; corner++)
            {
                var cornerCenter = new Vector2(
                    corner < 2 ? half.x - Radius : -half.x + Radius,
                    corner == 0 || corner == 3 ? half.y - Radius : -half.y + Radius);
                for (var step = 0; step <= 12; step++)
                {
                    var angle = (90f - corner * 90f - step * 7.5f) * Mathf.Deg2Rad;
                    var rounding = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;
                    var point = center + (offset + cornerCenter + rounding) * scale;
                    if (corner == 0 && step == 0)
                    {
                        first = point;
                    }
                    else if (fill)
                    {
                        AddTriangle(mesh, center + offset * scale, previous, point, tint);
                    }
                    else
                    {
                        DrawLine(mesh, previous, point, 11f * scale, tint);
                    }

                    previous = point;
                }
            }

            if (fill)
            {
                AddTriangle(mesh, center + offset * scale, previous, first, tint);
            }
            else
            {
                DrawLine(mesh, previous, first, 11f * scale, tint);
            }
        }

        private static void DrawLine(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
        {
            var direction = (to - from).normalized;
            var normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
            var index = mesh.currentVertCount;
            mesh.AddVert(from - normal, tint, Vector2.zero);
            mesh.AddVert(from + normal, tint, Vector2.zero);
            mesh.AddVert(to + normal, tint, Vector2.zero);
            mesh.AddVert(to - normal, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }

        private static void DrawCircle(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            for (var step = 0; step < 24; step++)
            {
                var first = step * Mathf.PI * 2f / 24f;
                var second = (step + 1) * Mathf.PI * 2f / 24f;
                AddTriangle(mesh, center, center + new Vector2(Mathf.Cos(first), Mathf.Sin(first)) * radius,
                    center + new Vector2(Mathf.Cos(second), Mathf.Sin(second)) * radius, tint);
            }
        }

        private static void AddTriangle(VertexHelper mesh, Vector2 first, Vector2 second, Vector2 third, Color tint)
        {
            var index = mesh.currentVertCount;
            mesh.AddVert(first, tint, Vector2.zero);
            mesh.AddVert(second, tint, Vector2.zero);
            mesh.AddVert(third, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
        }
    }
}
