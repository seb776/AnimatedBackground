using DrawBehindDesktopIcons.Utils;
using Khronos;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenGL.Wgl;

namespace DrawBehindDesktopIcons
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(out NativeMessage lpMsg, HandleRef hWnd, uint wMsgFilterMin,
   uint wMsgFilterMax, uint wRemoveMsg);

        private static void Worker(object num)
        {
            GlOnBackground backgroundRenderer = new GlOnBackground(File.Open("Shader3.glsl", FileMode.Open, FileAccess.Read));

            bool alive = true;
            var timer = new System.Timers.Timer(10000.0);
            timer.AutoReset = false;
            //timer.Elapsed += (_, __) => { alive = false; };
            timer.Start();
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            while (alive)
            {
                backgroundRenderer.Render((float)stopWatch.Elapsed.TotalSeconds);
                Thread.Sleep((int)(1000.0f / 60.0f));
                NativeMessage nativeMsg = new NativeMessage();
                PeekMessage(out nativeMsg, new HandleRef(), 0, 0, 1);
            }
        }

        static void Main(string[] args)
        {
            Worker(null);
            //Thread t = new Thread(new ParameterizedThreadStart(Worker));
            //t.Start()
            return;
            //}));
            //thread.Start();
            //glShaderSource(shader, 1, (const GLchar*const*)&_shaderCode, &len);

            //glCompileShader(shader);
            //GLint status = 0;
            //glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
            //if (status == GL_FALSE)
            //{
            //    GLint maxLength = 0;
            //    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &maxLength);
            //    char* buffer = (char*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, maxLength);
            //    glGetShaderInfoLog(shader, maxLength, &maxLength, &buffer[0]);
            //    OutputDebugString("compile shader info log");
            //    OutputDebugString((char*)buffer);
            //    glGetProgramInfoLog(shader, maxLength, &maxLength, &buffer[0]);
            //    OutputDebugString("compile program info log");
            //    OutputDebugString((char*)buffer);
            //}

            //GLuint program = glCreateProgram();

            // Attach our shaders to our program
            //glAttachShader(program, shader);
            // Link our program
            //glLinkProgram(program);

            // Note the different functions here: glGetProgram* instead of glGetShader*.
            //GLint isLinked = 0;
            //glGetProgramiv(program, GL_LINK_STATUS, &isLinked);
            //if (isLinked == GL_FALSE)
            //{
            //    GLint maxLength = 0;
            //    glGetProgramiv(program, GL_INFO_LOG_LENGTH, &maxLength);

            //     The maxLength includes the NULL character
            //    std::vector<GLchar> infoLog(maxLength);

            //    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &maxLength);
            //    char* buffer = (char*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, maxLength);

            //    glGetProgramInfoLog(program, maxLength, &maxLength, &buffer[0]);

            //    OutputDebugString("link program info log");
            //    OutputDebugString((char*)buffer);
            //}
            //glUseProgram(program);



            // Create a Graphics instance from the Device Context
            //using (Graphics g = Graphics.FromHdc(dc))
            //{
            //    //g.GetHdc()
            //    // Use the Graphics instance to draw a white rectangle in the upper 
            //    // left corner. In case you have more than one monitor think of the 
            //    // drawing area as a rectangle that spans across all monitors, and 
            //    // the 0,0 coordinate beeing in the upper left corner.
            //    g.FillRectangle(new SolidBrush(Color.White), 0, 0, 500, 500);

            //}
            // make sure to release the device context after use.
            //W32.ReleaseDC(workerw, dc);
            return;

            // Demo 2: Demo 2: Put a Windows Form behind desktop icons
            //int WS_CHILD = 0x40000000;
            //int WS_VISIBLE = 0x10000000;
            //int WS_CLIPSIBLINGS = 0x04000000;
            //int WS_CLIPCHILDREN = 0x02000000;


            //var cParams = new CreateParams();
            //cParams.X = 0;
            //cParams.Y = 0;
            //cParams.Width = 1920;
            //cParams.Height = 1080;
            //cParams.Style = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
            //var nativeWin = new NativeWindow();
            //nativeWin.CreateHandle(cParams);

            //Form form = new Form();
            //form.Text = "Test Window";
            ////var subWinDC = W32.GetDC(nativeWin.Handle);

            ////    W32.SetParent(nativeWin.Handle, workerw);
            //form.Load += new EventHandler((s, e) =>
            //{
            //    // Move the form right next to the in demo 1 drawn rectangle
            //    form.Width = 500;
            //    form.Height = 500;
            //    form.Left = 500;
            //    form.Top = 0;

            //    // Add a randomly moving button to the form
            //    Button button = new Button() { Text = "Catch Me" };
            //    form.Controls.Add(button);
            //    Random rnd = new Random();
            //    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            //    timer.Interval = 100;
            //    timer.Tick += new EventHandler((sender, eventArgs) =>
            //    {
            //        button.Left = rnd.Next(0, form.Width - button.Width);
            //        button.Top = rnd.Next(0, form.Height - button.Height);
            //    });
            //    timer.Start();

            //    // Those two lines make the form a child of the WorkerW, 
            //    // thus putting it behind the desktop icons and out of reach 
            //    // for any user intput. The form will just be rendered, no 
            //    // keyboard or mouse input will reach it. You would have to use 
            //    // WH_KEYBOARD_LL and WH_MOUSE_LL hooks to capture mouse and 
            //    // keyboard input and redirect it to the windows form manually, 
            //    // but thats another story, to be told at a later time.
            //    W32.SetParent(form.Handle, workerw);
            //});

            //// Start the Application Loop for the Form.
            //Application.Run(form);
        }
        private static string GetShaderInfoLog(uint shader)
        {
            const int MaxLength = 1024;

            StringBuilder infoLog = new StringBuilder(MaxLength);
            int length;

            Gl.GetShaderInfoLog(shader, MaxLength, out length, infoLog);

            return (infoLog.ToString());
        }
    }
}
