using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                      
                                                                     
                                                                           
                                                                              
                                                   
   
                                                                              
                                                                             
              
public sealed class EditorViewportRenderer : IDisposable
{
    private readonly Shader _shader;
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _lineVertexCount;

    public EditorViewportRenderer(string shadersDirectory)
    {
        _shader = new Shader(
            Path.Combine(shadersDirectory, "grid.vert"),
            Path.Combine(shadersDirectory, "grid.frag"));

        var vertices = BuildGridVertices(halfSize: 10, step: 1f);
        _lineVertexCount = vertices.Length / 6;                                  

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

        GL.BindVertexArray(0);
    }

                                                                                 
                                                                                 
    private static float[] BuildGridVertices(int halfSize, float step)
    {
        var verts = new List<float>();
        var gridColor = new Vector3(0.35f, 0.35f, 0.38f);

        for (int i = -halfSize; i <= halfSize; i++)
        {
                                           
            verts.AddRange(new[] { -halfSize * step, 0f, i * step, gridColor.X, gridColor.Y, gridColor.Z });
            verts.AddRange(new[] { halfSize * step, 0f, i * step, gridColor.X, gridColor.Y, gridColor.Z });

                                           
            verts.AddRange(new[] { i * step, 0f, -halfSize * step, gridColor.X, gridColor.Y, gridColor.Z });
            verts.AddRange(new[] { i * step, 0f, halfSize * step, gridColor.X, gridColor.Y, gridColor.Z });
        }

                          
        verts.AddRange(new[] { -halfSize * step, 0.01f, 0f, 0.8f, 0.2f, 0.2f });
        verts.AddRange(new[] { halfSize * step, 0.01f, 0f, 0.8f, 0.2f, 0.2f });
                        
        verts.AddRange(new[] { 0f, 0.01f, -halfSize * step, 0.2f, 0.4f, 0.9f });
        verts.AddRange(new[] { 0f, 0.01f, halfSize * step, 0.2f, 0.4f, 0.9f });

        return verts.ToArray();
    }

                                                                                   
                                                                              
    public void Render(Camera camera, Vector3 background, bool drawGrid = true)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(background.X, background.Y, background.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (!drawGrid) return;

        _shader.Use();
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix());

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Lines, 0, _lineVertexCount);
        GL.BindVertexArray(0);
    }

                                                                                   
                                                                         
                                                                         
                                                                        
                                                                                
    public void RenderModels(Camera camera, Shader modelShader, IEnumerable<ModelInstance> models)
    {
        modelShader.Use();
        modelShader.SetMatrix4("view", camera.GetViewMatrix());
        modelShader.SetMatrix4("projection", camera.GetProjectionMatrix());

        foreach (var model in models)
            model.Render(modelShader);
    }

                                                                           
                                                                    
                                                                     
                                                                        
                                                                       
                                                                     
                                                                         

    private int _markerVao, _markerVbo;
    private int _markerCapacityFloats;

    private void EnsureMarkerBuffers()
    {
        if (_markerVao != 0) return;
        _markerVao = GL.GenVertexArray();
        _markerVbo = GL.GenBuffer();
        GL.BindVertexArray(_markerVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _markerVbo);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.BindVertexArray(0);
    }

    private static void AddCross(List<float> verts, Vector3 center, float half, Vector3 color)
    {
        void Line(Vector3 a, Vector3 b)
        {
            verts.AddRange(new[] { a.X, a.Y, a.Z, color.X, color.Y, color.Z });
            verts.AddRange(new[] { b.X, b.Y, b.Z, color.X, color.Y, color.Z });
        }

        Line(center - new Vector3(half, 0, 0), center + new Vector3(half, 0, 0));
        Line(center - new Vector3(0, half, 0), center + new Vector3(0, half, 0));
        Line(center - new Vector3(0, 0, half), center + new Vector3(0, 0, half));
    }

    private static void AddCircleXZ(List<float> verts, Vector3 center, float radius, Vector3 color, int segments = 24)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            var p0 = center + new Vector3(MathF.Cos(a0) * radius, 0.02f, MathF.Sin(a0) * radius);
            var p1 = center + new Vector3(MathF.Cos(a1) * radius, 0.02f, MathF.Sin(a1) * radius);
            verts.AddRange(new[] { p0.X, p0.Y, p0.Z, color.X, color.Y, color.Z });
            verts.AddRange(new[] { p1.X, p1.Y, p1.Z, color.X, color.Y, color.Z });
        }
    }

                                                                                      
                                                                                  
                                                                                 
                                                                                 
                                                                            
                                                      
    public readonly record struct ViewportMarker(Vector3 Position, Vector3 Color, bool Selected, float TriggerRadius = 0f);

    public void RenderMarkers(Camera camera, IReadOnlyList<ViewportMarker> markers)
    {
        if (markers.Count == 0) return;
        EnsureMarkerBuffers();

        var verts = new List<float>();
        foreach (var marker in markers)
        {
            float half = marker.Selected ? 0.45f : 0.3f;
            var color = marker.Selected ? Vector3.Lerp(marker.Color, Vector3.One, 0.4f) : marker.Color;
            AddCross(verts, marker.Position, half, color);
            if (marker.TriggerRadius > 0f)
                AddCircleXZ(verts, marker.Position, marker.TriggerRadius, color);
        }

        var data = verts.ToArray();
        GL.BindVertexArray(_markerVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _markerVbo);
        if (data.Length > _markerCapacityFloats)
        {
            _markerCapacityFloats = data.Length;
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
        }

                                                                             
                                                                             
                                                                 
        GL.Disable(EnableCap.DepthTest);
        GL.LineWidth(2f);

        _shader.Use();
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        GL.DrawArrays(PrimitiveType.Lines, 0, data.Length / 6);

        GL.LineWidth(1f);
        GL.Enable(EnableCap.DepthTest);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        if (_markerVbo != 0) GL.DeleteBuffer(_markerVbo);
        if (_markerVao != 0) GL.DeleteVertexArray(_markerVao);
        if (_gizmoVbo != 0) GL.DeleteBuffer(_gizmoVbo);
        if (_gizmoVao != 0) GL.DeleteVertexArray(_gizmoVao);
        _shader.Dispose();
    }

                                                                             
                                                                        
                                                                             
                                                                              
                                                                              
                                                                           
                                                                  

    private int _gizmoVao, _gizmoVbo;
    private int _gizmoCapacityFloats;

    private void EnsureGizmoBuffers()
    {
        if (_gizmoVao != 0) return;
        _gizmoVao = GL.GenVertexArray();
        _gizmoVbo = GL.GenBuffer();
        GL.BindVertexArray(_gizmoVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _gizmoVbo);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.BindVertexArray(0);
    }

                                                                                    
                                                                                    
                                                                               
                                                                                 
                                                                                
                                                                                      
                                                                              
                                                                                  
                                                                           
                               
    public void RenderGizmo(Camera camera, Vector3 origin, float axisLength, GizmoAxis? hoverOrDrag)
    {
        EnsureGizmoBuffers();

        var verts = new List<float>();
        AddGizmoAxis(verts, origin, Vector3.UnitX, axisLength, new Vector3(0.85f, 0.15f, 0.15f), hoverOrDrag == GizmoAxis.X);
        AddGizmoAxis(verts, origin, Vector3.UnitY, axisLength, new Vector3(0.15f, 0.75f, 0.15f), hoverOrDrag == GizmoAxis.Y);
        AddGizmoAxis(verts, origin, Vector3.UnitZ, axisLength, new Vector3(0.15f, 0.35f, 0.9f), hoverOrDrag == GizmoAxis.Z);

        var data = verts.ToArray();
        GL.BindVertexArray(_gizmoVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _gizmoVbo);
        if (data.Length > _gizmoCapacityFloats)
        {
            _gizmoCapacityFloats = data.Length;
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
        }

                                                                                  
                                                                              
                 
        GL.Disable(EnableCap.DepthTest);
        GL.LineWidth(3f);

        _shader.Use();
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        GL.DrawArrays(PrimitiveType.Lines, 0, data.Length / 6);

        GL.LineWidth(1f);
        GL.Enable(EnableCap.DepthTest);
        GL.BindVertexArray(0);
    }

    private static void AddGizmoAxis(List<float> verts, Vector3 origin, Vector3 axis, float length, Vector3 color, bool highlighted)
    {
        var tip = origin + axis * length;
        var drawColor = highlighted ? Vector3.Lerp(color, Vector3.One, 0.5f) : color;

        verts.AddRange(new[] { origin.X, origin.Y, origin.Z, drawColor.X, drawColor.Y, drawColor.Z });
        verts.AddRange(new[] { tip.X, tip.Y, tip.Z, drawColor.X, drawColor.Y, drawColor.Z });

                                                                             
                                                                             
                                                                            
                    
        float tickSize = length * 0.08f;
        Vector3 perpA = axis == Vector3.UnitY ? Vector3.UnitX : Vector3.UnitY;
        Vector3 perpB = Vector3.Cross(axis, perpA);
        var tickBase = origin + axis * (length * 0.85f);

        verts.AddRange(new[] { (tickBase - perpA * tickSize).X, (tickBase - perpA * tickSize).Y, (tickBase - perpA * tickSize).Z, drawColor.X, drawColor.Y, drawColor.Z });
        verts.AddRange(new[] { (tickBase + perpA * tickSize).X, (tickBase + perpA * tickSize).Y, (tickBase + perpA * tickSize).Z, drawColor.X, drawColor.Y, drawColor.Z });
        verts.AddRange(new[] { (tickBase - perpB * tickSize).X, (tickBase - perpB * tickSize).Y, (tickBase - perpB * tickSize).Z, drawColor.X, drawColor.Y, drawColor.Z });
        verts.AddRange(new[] { (tickBase + perpB * tickSize).X, (tickBase + perpB * tickSize).Y, (tickBase + perpB * tickSize).Z, drawColor.X, drawColor.Y, drawColor.Z });
    }

                                                                           
                                                                                  
                                                                                 
                                                                             
                                                                             
                                                                          
                                                                               
                                                                            
                                                                               
    private const int RingSegments = 32;

    public void RenderRotateGizmo(Camera camera, Vector3 origin, float radius, GizmoAxis? hoverOrDrag)
    {
        EnsureGizmoBuffers();

        var verts = new List<float>();
        AddGizmoRing(verts, origin, Vector3.UnitX, radius, new Vector3(0.85f, 0.15f, 0.15f), hoverOrDrag == GizmoAxis.X);
        AddGizmoRing(verts, origin, Vector3.UnitY, radius, new Vector3(0.15f, 0.75f, 0.15f), hoverOrDrag == GizmoAxis.Y);
        AddGizmoRing(verts, origin, Vector3.UnitZ, radius, new Vector3(0.15f, 0.35f, 0.9f), hoverOrDrag == GizmoAxis.Z);

        var data = verts.ToArray();
        GL.BindVertexArray(_gizmoVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _gizmoVbo);
        if (data.Length > _gizmoCapacityFloats)
        {
            _gizmoCapacityFloats = data.Length;
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
        }

        GL.Disable(EnableCap.DepthTest);
        GL.LineWidth(3f);

        _shader.Use();
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        GL.DrawArrays(PrimitiveType.Lines, 0, data.Length / 6);

        GL.LineWidth(1f);
        GL.Enable(EnableCap.DepthTest);
        GL.BindVertexArray(0);
    }

                                                                               
                                                                         
                                                                            
                                                                             
                                                                               
                                                               
    private static void AddGizmoRing(List<float> verts, Vector3 origin, Vector3 axis, float radius, Vector3 color, bool highlighted)
    {
        var drawColor = highlighted ? Vector3.Lerp(color, Vector3.One, 0.5f) : color;
        Vector3 perpA = Vector3.Normalize(axis == Vector3.UnitY ? Vector3.UnitX : Vector3.UnitY);
        perpA = Vector3.Normalize(perpA - axis * Vector3.Dot(perpA, axis));                               
        Vector3 perpB = Vector3.Cross(axis, perpA);

        Vector3 PointAt(float angleRad) => origin + (perpA * MathF.Cos(angleRad) + perpB * MathF.Sin(angleRad)) * radius;

        var prev = PointAt(0f);
        for (int i = 1; i <= RingSegments; i++)
        {
            float angle = i / (float)RingSegments * MathF.Tau;
            var next = PointAt(angle);
            verts.AddRange(new[] { prev.X, prev.Y, prev.Z, drawColor.X, drawColor.Y, drawColor.Z });
            verts.AddRange(new[] { next.X, next.Y, next.Z, drawColor.X, drawColor.Y, drawColor.Z });
            prev = next;
        }
    }
}

                                                                               
                                                                               
public enum GizmoAxis { X, Y, Z }
