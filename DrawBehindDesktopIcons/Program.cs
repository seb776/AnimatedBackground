using Khronos;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenGL.Wgl;

namespace DrawBehindDesktopIcons
{
    class toto : Khronos.KhronosApi.ExtensionsCollection
    {
    }
    class Program
    {
        static void Main(string[] args)
        {

            PrintVisibleWindowHandles(2);
            // The output will look something like this. 
            // .....
            // 0x00010190 "" WorkerW
            //   ...
            //   0x000100EE "" SHELLDLL_DefView
            //     0x000100F0 "FolderView" SysListView32
            // 0x000100EC "Program Manager" Progman



            // Fetch the Progman window
            IntPtr progman = W32.FindWindow("Progman", null);

            IntPtr result = IntPtr.Zero;

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            W32.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                   1000,
                                   out result);


            PrintVisibleWindowHandles(2);
            // The output will look something like this
            // .....
            // 0x00010190 "" WorkerW
            //   ...
            //   0x000100EE "" SHELLDLL_DefView
            //     0x000100F0 "FolderView" SysListView32
            // 0x00100B8A "" WorkerW                                   <--- This is the WorkerW instance we are after!
            // 0x000100EC "Program Manager" Progman

            IntPtr workerw = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            // We now have the handle of the WorkerW behind the desktop icons.
            // We can use it to create a directx device to render 3d output to it, 
            // we can use the System.Drawing classes to directly draw onto it, 
            // and of course we can set it as the parent of a windows form.
            //
            // There is only one restriction. The window behind the desktop icons does
            // NOT receive any user input. So if you want to capture mouse movement, 
            // it has to be done the LowLevel way (WH_MOUSE_LL, WH_KEYBOARD_LL).


            // Demo 1: Draw graphics between icons and wallpaper

            // Get the Device Context of the WorkerW
            IntPtr dc = W32.GetDCEx(workerw, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            if (dc != IntPtr.Zero)
            {
                int screenCount = 3;
                var size = new W32.RECT();



                Graphics g = Graphics.FromHdc(dc);
                var hdc = g.GetHdc();

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
                    Khronos.KhronosApi.Log += delegate (object sender, KhronosLogEventArgs e) {
                        Console.WriteLine("GLLog=>"+e.ToString());
                    };
                    Khronos.KhronosApi.LogEnabled = true;
                }


                Gl.Initialize();
                OpenGL.Egl.IsRequired = false;
                //Gl.Init
                //Gl.BindAPI(Gl.QueryContextVersion(), new toto());
                //Gl.QueryContextVersion().
                int WS_CHILD = 0x40000000;
                int WS_VISIBLE = 0x10000000;
                int WS_CLIPSIBLINGS = 0x04000000;
                int WS_CLIPCHILDREN = 0x02000000;


                var cPar = new CreateParams();
                cPar.X = 0;
                cPar.Y = 0;
                cPar.Width = 1920*3;
                cPar.Height = 1080;
                cPar.Style = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
                cPar.Parent = workerw;
                var nativeWin = new NativeWindow();
                nativeWin.CreateHandle(cPar);
                var subWinDC = W32.GetDC(nativeWin.Handle);

                int iPxlFmt = OpenGL.Wgl.ChoosePixelFormat(subWinDC, ref pfd);
                OpenGL.Wgl.SetPixelFormat(subWinDC, iPxlFmt, ref pfd);

                W32.SetParent(nativeWin.Handle, workerw);
                var deviceContext = DeviceContext.Create(IntPtr.Zero, nativeWin.Handle);
                //var pixFmt = new DevicePixelFormat(32);
                //deviceContext.ChoosePixelFormat(pixFmt);
                //deviceContext.SetPixelFormat(pixFmt);
                deviceContext.MakeCurrent(subWinDC);
                IntPtr glContext2 = deviceContext.CreateContext(IntPtr.Zero);
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

                deviceContext.MakeCurrent(glContext2);

                //W32.wglMakeCurrent(dc, glContext2);

                //Gl.BindAPI();

                uint shaderId = Gl.CreateShader(OpenGL.ShaderType.FragmentShader);

                var glslCode = File.ReadAllText("shader2.glsl");
                var shader = new string[] { glslCode };
                Gl.ShaderSource(shaderId, shader);
                Gl.CompileShader(shaderId);
                int compileStatus;
                Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out compileStatus);
                if (compileStatus == 0)
                    throw new InvalidOperationException("unable to compiler vertex shader: " + GetShaderInfoLog(shaderId));




                var program = Gl.CreateProgram();

                Gl.AttachShader(program, shaderId);

                int[] arr = new int[42];
                Gl.GetProgram(0, ProgramProperty.ActiveUniforms, arr);
                Gl.LinkProgram(program);
                Gl.GetProgram(program, ProgramProperty.LinkStatus, out compileStatus);
                if (compileStatus == 0)
                    throw new InvalidOperationException("unable to link program");


                Gl.UseProgram(program);

                //var thread = new Thread(new ParameterizedThreadStart((o) =>
                //{
                //    W32.wglMakeCurrent(dc, glContext2);

                    bool alive = true;
                    var timer = new System.Timers.Timer(10000.0);
                    timer.AutoReset = false;
                    timer.Elapsed += (_, __) => { alive = false; };
                    timer.Start();
                    var stopWatch = new Stopwatch();

                    stopWatch.Start();

                    while (alive)
                    {
                        var timeLoc = Gl.GetUniformLocation(program, "time");
                        Gl.Uniform1(timeLoc, (float)stopWatch.Elapsed.TotalSeconds);

                    var resolutionLoc = Gl.GetUniformLocation(program, "resolution");
                    //Gl.UNIFORM
                        Gl.Uniform2(resolutionLoc, (float)width, (float)height);


                        OpenGL.Gl.Rect(-1, -1, 1, 1);
                    //OpenGL.Wgl.swap
                    deviceContext.SwapBuffers();
                        //W32.SwapBuffers(dc);
                        Thread.Sleep((int)(1000.0f/60.0f));

                    }
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
                W32.ReleaseDC(workerw, dc);
            }
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

            Form form = new Form();
            form.Text = "Test Window";
            //var subWinDC = W32.GetDC(nativeWin.Handle);
            
            //    W32.SetParent(nativeWin.Handle, workerw);
            form.Load += new EventHandler((s, e) =>
            {
                // Move the form right next to the in demo 1 drawn rectangle
                form.Width = 500;
                form.Height = 500;
                form.Left = 500;
                form.Top = 0;

                // Add a randomly moving button to the form
                Button button = new Button() { Text = "Catch Me" };
                form.Controls.Add(button);
                Random rnd = new Random();
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 100;
                timer.Tick += new EventHandler((sender, eventArgs) =>
                {
                    button.Left = rnd.Next(0, form.Width - button.Width);
                    button.Top = rnd.Next(0, form.Height - button.Height);
                });
                timer.Start();

                // Those two lines make the form a child of the WorkerW, 
                // thus putting it behind the desktop icons and out of reach 
                // for any user intput. The form will just be rendered, no 
                // keyboard or mouse input will reach it. You would have to use 
                // WH_KEYBOARD_LL and WH_MOUSE_LL hooks to capture mouse and 
                // keyboard input and redirect it to the windows form manually, 
                // but thats another story, to be told at a later time.
                W32.SetParent(form.Handle, workerw);
            });

            // Start the Application Loop for the Form.
            Application.Run(form);
        }
        private static string GetShaderInfoLog(uint shader)
        {
            const int MaxLength = 1024;

            StringBuilder infoLog = new StringBuilder(MaxLength);
            int length;

            Gl.GetShaderInfoLog(shader, MaxLength, out length, infoLog);

            return (infoLog.ToString());
        }
        static void PrintVisibleWindowHandles(IntPtr hwnd, int maxLevel=-1, int level=0)
        {
            bool isVisible = W32.IsWindowVisible(hwnd);

            if (isVisible && (maxLevel==-1||level<=maxLevel))
            {
                StringBuilder className = new StringBuilder(256);
                W32.GetClassName(hwnd, className, className.Capacity);

                StringBuilder windowTitle = new StringBuilder(256);
                W32.GetWindowText(hwnd, windowTitle, className.Capacity);
//W32.SetPi
                Console.WriteLine("".PadLeft(level*2)+"0x{0:X8} \"{1}\" {2}", hwnd.ToInt64(), windowTitle, className);

                level++;

                // Enumerates all child windows of the current window
                W32.EnumChildWindows(hwnd, new W32.EnumWindowsProc((childhandle, childparamhandle) =>
                {
                    PrintVisibleWindowHandles(childhandle, maxLevel, level);
                    return true;
                }), IntPtr.Zero);
            }            
        }
        static void PrintVisibleWindowHandles(int maxLevel=-1)
        {
            // Enumerates all existing top window handles. This includes open and visible windows, as well as invisible windows.
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                PrintVisibleWindowHandles(tophandle, maxLevel);
                return true;
            }), IntPtr.Zero);
        }
               
    }
}
