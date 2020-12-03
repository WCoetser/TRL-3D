using Microsoft.Extensions.Logging;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk
{
    /// <summary>
    /// Reference: https://dreamstatecoding.blogspot.com/p/opengl4-with-opentk-tutorials.html
    /// </summary>
    public class RenderWindow : GameWindow, IRenderWindow
    {
        private ILogger _logger;

        int _program;
        private int _vertexArray;
        private double _time;
        const string vertexShaderCode =
@"
#version 450 core

layout (location = 0) in float time;
layout (location = 1) in vec4 position;
out vec4 frag_color;

void main(void)
{
 gl_Position = position;
 frag_color = vec4(sin(time) * 0.5 + 0.5, cos(time) * 0.5 + 0.5, 0.0, 0.0);
}
";

        const string fragmentShaderCode =
@"
#version 450 core

in vec4 frag_color;
out vec4 color;

void main(void)
{
 color = frag_color;
}
";

        public RenderWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Load += MainWindowLoad;
            Resize += MainWindowResize;
            RenderFrame += MainWindowRenderFrame;
            UpdateFrame += MainWindowUpdateFrame;
        }

        private int CompileShaders()
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderCode);
            GL.CompileShader(vertexShader);

            var info = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogWarning($"GL.CompileShader had info log: {info}");

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderCode);
            GL.CompileShader(fragmentShader);

            info = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogWarning($"GL.CompileShader had info log: {info}");

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            info = GL.GetProgramInfoLog(program);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogWarning($"GL.LinkProgram had info log: {info}");

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        private void MainWindowUpdateFrame(FrameEventArgs obj)
        {
            //_logger.LogInformation($"Game logic rate = {1.0 / obj.Time} updates per second");
            
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        private void MainWindowRenderFrame(FrameEventArgs e)
        {
            _time += e.Time;

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.1f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            // Render point
            GL.UseProgram(_program);

            // add shader attributes here
            GL.VertexAttrib1(0, (float)_time);
            Vector4 position;
            position.X = (float)Math.Sin(_time) * 0.5f;
            position.Y = (float)Math.Cos(_time) * 0.5f;
            position.Z = 0.0f;
            position.W = 1.0f;
            GL.VertexAttrib4(1, position);

            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            //_logger.LogInformation($"Render rate = {1.0 / e.Time} fps");

            SwapBuffers();
        }

        private void MainWindowResize(ResizeEventArgs obj)
        {
            GL.Viewport(0, 0, obj.Width, obj.Height);
        }

        private void MainWindowClosed()
        {
            GL.DeleteProgram(_program);

            GL.DeleteVertexArrays(1, ref _vertexArray);
            GL.DeleteProgram(_program);
        }

        private void MainWindowLoad()
        {
            _logger.LogInformation($"Open GL version: {GL.GetString(StringName.Version)}");
            _program = CompileShaders();

            GL.GenVertexArrays(1, out _vertexArray);
            GL.BindVertexArray(_vertexArray);

            Closed += MainWindowClosed;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}
