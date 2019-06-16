﻿using DrawBehindDesktopIcons.Utils;
using Khronos;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static OpenGL.Wgl;

namespace DrawBehindDesktopIcons
{
    public class GlOnBackground
    {
        DeviceContext _deviceContext;
        uint _program;

        public GlOnBackground(Stream shader)
        {
            using (StreamReader sr = new StreamReader(shader))
            {
                _setupContext(sr.ReadToEnd());
            }
        }

        private static string GetShaderInfoLog(uint shader)
        {
            const int MaxLength = 1024;

            StringBuilder infoLog = new StringBuilder(MaxLength);
            int length;

            Gl.GetShaderInfoLog(shader, MaxLength, out length, infoLog);

            return (infoLog.ToString());
        }

        private void _setupContext(string shaderCode)
        {
            int screenCount = 3;
            var size = new W32.RECT();


            IntPtr workerw = Utils.Windows.GetWindowsBackgroundHandle();

            var pfd = new PIXELFORMATDESCRIPTOR
            {
                nVersion = 1,
                dwFlags = (PixelFormatDescriptorFlags)(PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER | PFD_TYPE_RGBA),
                cColorBits = 32,
                cDepthBits = 24,
                dwLayerMask = PFD_MAIN_PLANE
            };

            string envDebug = Environment.GetEnvironmentVariable("DEBUG");
            if (true)//envDebug == "GL")
            {
                Khronos.KhronosApi.Log += delegate (object sender, KhronosLogEventArgs e)
                {
                    Console.WriteLine("GLLog=>" + e.ToString());
                };
                Khronos.KhronosApi.LogEnabled = true;
            }


            Gl.Initialize();
            OpenGL.Egl.IsRequired = false;




            var cPar = new CreateParams();
            cPar.X = 0;
            cPar.Y = 0;
            cPar.Width = 1920 * 3;
            cPar.Height = 1080;
            cPar.Style = Windows.WS_CHILD | Windows.WS_VISIBLE | Windows.WS_CLIPSIBLINGS | Windows.WS_CLIPCHILDREN;
            cPar.Parent = workerw;
            var nativeWin = new NativeWindow();
            nativeWin.CreateHandle(cPar);
            var subWinDC = W32.GetDC(nativeWin.Handle);

            int iPxlFmt = OpenGL.Wgl.ChoosePixelFormat(subWinDC, ref pfd);
            OpenGL.Wgl.SetPixelFormat(subWinDC, iPxlFmt, ref pfd);

            W32.SetParent(nativeWin.Handle, workerw);
            _deviceContext = DeviceContext.Create(IntPtr.Zero, nativeWin.Handle);
            //var pixFmt = new DevicePixelFormat(32);
            //deviceContext.ChoosePixelFormat(pixFmt);
            //deviceContext.SetPixelFormat(pixFmt);
            _deviceContext.MakeCurrent(subWinDC);
            IntPtr glContext2 = _deviceContext.CreateContext(IntPtr.Zero);
            if (glContext2 == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());

            //var glCtx = OpenGL.Wgl.CreateContext(hdc);

            //    int iPxlFmt = OpenGL.Wgl.ChoosePixelFormat(glCtx, ref pfd);
            //OpenGL.Wgl.SetPixelFormat(glCtx, iPxlFmt, ref pfd);

            //Gl.CheckErrors();
            //W32.wglMakeCurrent(hdc, IntPtr.Zero);

            W32.GetWindowRect(workerw, ref size);
            int width = size.Width;// / 3;
            int height = size.Height;

            //var glContext = W32.wglCreateContext(hdc);
            //var errCode = Gl.GetError();

            _deviceContext.MakeCurrent(glContext2);

            //W32.wglMakeCurrent(dc, glContext2);

            //Gl.BindAPI();

            uint shaderId = Gl.CreateShader(OpenGL.ShaderType.FragmentShader);

            var glslCode = shaderCode;
            var shader = new string[] { glslCode };
            Gl.ShaderSource(shaderId, shader);
            Gl.CompileShader(shaderId);
            int compileStatus;
            Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out compileStatus);
            if (compileStatus == 0)
                throw new InvalidOperationException("unable to compiler vertex shader: " + GetShaderInfoLog(shaderId));

            _program = Gl.CreateProgram();

            Gl.AttachShader(_program, shaderId);

            int[] arr = new int[42];
            Gl.GetProgram(0, ProgramProperty.ActiveUniforms, arr);
            Gl.LinkProgram(_program);
            Gl.GetProgram(_program, ProgramProperty.LinkStatus, out compileStatus);
            if (compileStatus == 0)
                throw new InvalidOperationException("unable to link program");


            Gl.UseProgram(_program);
        }

        public void Render(float time, Point resolution)
        {
            var timeLoc = Gl.GetUniformLocation(_program, "time");
            Gl.Uniform1(timeLoc, time);

            var resolutionLoc = Gl.GetUniformLocation(_program, "resolution");
            //Gl.UNIFORM
            Gl.Uniform2(resolutionLoc, (float)resolution.X, (float)resolution.Y);


            OpenGL.Gl.Rect(-1, -1, 1, 1);
            //OpenGL.Wgl.swap
            _deviceContext.SwapBuffers();
        }
    }
}