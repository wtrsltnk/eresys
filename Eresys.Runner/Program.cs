﻿using Eresys.Extra;
using Eresys.Graphics.GL;
using Eresys.Graphics.GL.Renderers;
using Eresys.Math;
using Eresys.Practises.Logging;
using System;
using System.IO;

namespace Eresys
{
    public class Application : IApplication
    {
        public static int Main(string[] args)
        {
            var kernel = new Kernel();

            // init some members
            // init settings
            Settings settings = null;
            bspFileName = "";

            // process args
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (arg.StartsWith("-map="))
                {
                    bspFileName = arg.Substring(5);
                    break;
                }
                if (arg.StartsWith("-ini="))
                {
                    // init settings
                    settings = new Settings(arg.Substring(5));
                    break;
                }
            }

            kernel.Settings = settings ?? new Settings("eresys.ini");

            // start application
            return new Application()
                .Run(kernel);
        }

        public ILogger Logger { get; set; } = new ConsoleLogger();

        public int Run(Kernel kernel)
        {
            // init graphics
            switch (kernel.Settings["graphics"])
            {
                case "directx":
                    kernel.Graphics = new DummyGraphics();// DXGraphics();
                    break;
                case "opengl":
                    kernel.Graphics = new GlGraphics(new RendererFactory());
                    break;
                default:
                    throw new Exception("Graphics renderer " + kernel.Settings["graphics"] + " invalid!");
            }

            // init controls
            switch (kernel.Settings["controls"])
            {
                case "directx":
                    kernel.Controls = new DummyControls();// DXControls();
                    break;
                default:
                    throw new Exception("Controls manager " + kernel.Settings["controls"] + " invalid!");
            }

            return kernel.Startup(this, "eresys.log");
        }

        public void Startup(Kernel kernel)
        {
            // init some members
            wireOn = false;
            useCd = true;
            closing = false;

            // init splash and credits screen
            splashIdx = kernel.Graphics.AddTexture(new Texture("eresys-splash.jpg"));
            kernel.Graphics.BeginFrame();
            kernel.Graphics.RenderTexture(splashIdx, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);
            kernel.Graphics.EndFrame();
            creditsIdx = kernel.Graphics.AddTexture(new Texture("eresys-credits.jpg"));

            // init keys
            escape = new Button(Key.Escape, ButtonType.Trigger, kernel.Controls);
            wire = new Button(Key.Z, ButtonType.Trigger, kernel.Controls);
            frustum = new Button(Key.F, ButtonType.Trigger, kernel.Controls);
            forward = new Button(Key.UpArrow, ButtonType.Key, kernel.Controls);
            backward = new Button(Key.DownArrow, ButtonType.Key, kernel.Controls);
            left = new Button(Key.LeftArrow, ButtonType.Key, kernel.Controls);
            right = new Button(Key.RightArrow, ButtonType.Key, kernel.Controls);
            up = new Button(Key.RightShift, ButtonType.Key, kernel.Controls);
            down = new Button(Key.RightControl, ButtonType.Key, kernel.Controls);
            useBsp = new Button(Key.B, ButtonType.Trigger, kernel.Controls);
            f2 = new Button(Key.F2, ButtonType.Trigger, kernel.Controls);
            f3 = new Button(Key.F3, ButtonType.Key, kernel.Controls);
            f4 = new Button(Key.F4, ButtonType.Key, kernel.Controls);
            f5 = new Button(Key.F5, ButtonType.Key, kernel.Controls);
            f6 = new Button(Key.F6, ButtonType.Key, kernel.Controls);
            f7 = new Button(Key.F7, ButtonType.Key, kernel.Controls);
            f8 = new Button(Key.F8, ButtonType.Key, kernel.Controls);
            ss = new Button(Key.SysRq, ButtonType.Trigger, kernel.Controls);
            resetPos = new Button(Key.P, ButtonType.Trigger, kernel.Controls);
            cd = new Button(Key.C, ButtonType.Trigger, kernel.Controls);

            // init scene
            try
            {
                if (string.IsNullOrWhiteSpace(bspFileName))
                {
                    bspFileName = kernel.Settings["map"];
                }

                bsp = new HlBspMap();

                new HlBspMapLoader()
                    .LoadFromFile(bspFileName, bsp, kernel.Graphics);

                kernel.Scene.AddObject(bsp);
            }
            catch (FileNotFoundException fnfe)
            {
                Logger.Log(LogLevels.Warning, $"Couldn't load bsp map \'{fnfe.FileName}\'!");
                bsp = null;
                kernel.Graphics.FrameClearing = true;
            }

            // init player
            player = new Player();
            kernel.Scene.player = player;
            SetPos();

            // init screentext
            font = kernel.Graphics.AddFont("arial", 10, true, false);
            textColor = new Color(120, 180, 240);
            fpsTimer = fpsUpdateInt;
            versionText = "Eresys " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " - map: " + (bsp == null ? "no map loaded!" : bsp.FileName);
            vh = Int32.Parse(kernel.Settings["height"]) - 24;
        }

        private void SetPos()
        {
            if (bsp == null) return;
            try
            {
                // try to read player position from bsp entity script ...
                string spos = bsp.EntityScript["info_player_start"][0]["origin"];
                Point3D pos;
                string[] apos = spos.Split(' ');
                pos.x = Single.Parse(apos[0]);
                pos.y = Single.Parse(apos[2]) + 20.0f;
                pos.z = Single.Parse(apos[1]);
                player.Position = pos;
            }
            catch (Exception)
            {
                // ... if that fails, we use this default position
                player.Position = new Point3D(0.0f, 60.0f, 0.0f);
            }
            try
            {
                // try to read player direction from bsp entity script ...
                string sdir = bsp.EntityScript["info_player_start"][0]["angle"];
                player.Direction = new Point3D(0.0f, (Int32.Parse(sdir) + 90) * (float)System.Math.PI / 180.0f, 0.0f);
            }
            catch (Exception)
            {
                // ... again, if that fails, we use this default position
                player.Direction = new Point3D(0.0f, 0.0f, 0.0f);
            }
        }

        public void Terminate()
        {
            // no need to do anything special here
        }

        public void Pause(bool pause)
        {
            // ...
        }

        private Color GetFpsColor(float fps)
        {
            float rs, gs; // red and green scale factors

            gs = fps / 15.0f;
            if (gs > 1.0f) gs = 1.0f;
            if (gs < 0.0f) gs = 0.0f;

            rs = (30 - fps) / 15.0f;
            if (rs > 1.0f) rs = 1.0f;
            if (rs < 0.0f) rs = 0.0f;

            return new Color((byte)(rs * 255), (byte)(gs * 255), 0);
        }

        public void Update(Kernel kernel)
        {
            if (closing)
            {
                if ((kernel.Time - closingTime) >= creditsTime) kernel.Terminate();
                else if (escape.Active()) kernel.Terminate();
                return;
            }

            // check for escape key
            if (escape.Active())
            {
                closing = true;
                closingTime = kernel.Time;
                return;
            }

            // update text
            fpsTimer += kernel.Interval;
            if (fpsTimer >= fpsUpdateInt)
            {
                while (fpsTimer >= fpsUpdateInt) fpsTimer -= fpsUpdateInt;
                fps = kernel.FPS;
                avgFps = kernel.AverageFPS;
                fpsColor = GetFpsColor(fps);
                avgFpsColor = GetFpsColor(avgFps);
                fpsText = String.Format("{0,4}", (int)System.Math.Round(fps));
                fpsText = fpsText.Replace(' ', '0') + " FPS";
                avgFpsText = String.Format("{0,4}", (int)System.Math.Round(avgFps));
                avgFpsText = avgFpsText.Replace(' ', '0') + " AVERAGE FPS";
            }

            // return if no map is loaded
            if (bsp == null) return;

            // update other keys
            if (wire.Active()) wireOn = !wireOn;
            if (useBsp.Active()) bsp.BSPRendering = !bsp.BSPRendering;
            if (frustum.Active()) bsp.FrustumCulling = !bsp.FrustumCulling;
            if (f2.Active())
            {
                kernel.Graphics.Brightness = 0.5f;
                kernel.Graphics.Contrast = 0.5f;
                kernel.Graphics.Gamma = 1.0f;
            }
            float gpi = 0.01f; // Gamma Parameters Interval = amount of brightness/contrast/gamma added/removed per keypress
            if (f3.Active()) kernel.Graphics.Brightness -= gpi;
            if (f4.Active()) kernel.Graphics.Brightness += gpi;
            if (f5.Active()) kernel.Graphics.Contrast -= gpi;
            if (f6.Active()) kernel.Graphics.Contrast += gpi;
            if (f7.Active()) kernel.Graphics.Gamma -= gpi;
            if (f8.Active()) kernel.Graphics.Gamma += gpi;
            if (resetPos.Active()) SetPos();
            if (ss.Active())
            {
                // this is somewhat stupid code, but it works fine so we'll leave it like this for now
                int i0 = 1, i1 = 0, i2 = 0;
                string file;
                bool shoot = true;
                do
                {
                    file = String.Format("{0}{1}{2}.jpg", i2, i1, i0);
                    i0++;
                    if (i0 > 9)
                    {
                        i0 = 0;
                        i1++;
                    }
                    if (i1 > 9)
                    {
                        i1 = 0;
                        i2++;
                    }
                    if (i2 > 9)
                    {
                        shoot = false;
                        break;
                    }
                }
                while (File.Exists(file));
                if (shoot) kernel.Graphics.TakeScreenshot().SaveToFile(file);
            }
            if (cd.Active()) useCd = !useCd;

            // rotate player
            Point3D dir = player.Direction;
            dir.x += ((float)System.Math.PI * 1.5f * kernel.Controls.GetMousePosition().y) / (2.0f * Single.Parse(kernel.Settings["height"]));
            dir.y += ((float)System.Math.PI * 1.5f * kernel.Controls.GetMousePosition().x) / (2.0f * Single.Parse(kernel.Settings["width"]));
            if (dir.x > 1.5f) dir.x = 1.5f;
            if (dir.x < -1.5f) dir.x = -1.5f;
            player.Direction = dir;

            // move player
            Vector vec = new Vector(0.0f, 0.0f, 0.0f);
            if (forward.Active()) vec.z += 1.0f;
            if (backward.Active()) vec.z -= 1.0f;
            if (left.Active()) vec.x -= 1.0f;
            if (right.Active()) vec.x += 1.0f;
            if (up.Active()) vec.y += 1.0f;
            if (down.Active()) vec.y -= 1.0f;
            vec.Normalize();
            Matrix mtrans = Matrix.RotationYawPitchRoll(dir.y, 0.0f, 0.0f);
            vec = Vector.TransformNormal(vec, mtrans);
            vec *= 128.0f;
            player.Speed = vec;

            // collision detection between player and bsp
            kernel.Profiler.StartSample("collision detection");
            if (useCd) player.Speed = bsp.CheckCollision(player.Position, player.Speed * (float)kernel.Interval, 20) / (float)kernel.Interval;
            kernel.Profiler.StopSample();

            // update zoom
            if (kernel.Controls.GetMousePosition().z < 0.0f) player.Camera.Camera.Zoom *= 2.0f;
            if (kernel.Controls.GetMousePosition().z > 0.0f) if (player.Camera.Camera.Zoom > 1.0f) player.Camera.Camera.Zoom /= 2.0f;

            // check if map should be visible
            bsp.Visible = kernel.Time >= splashTime;
        }

        public void Render(Kernel kernel)
        {
            if (closing)
            {
                // display the credits screen
                kernel.Graphics.RenderTexture(creditsIdx, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);
                kernel.Graphics.RenderText(font, textColor, new Point2D(8, 8), String.Format("Eresys will close in {0} seconds. Press ESCAPE to quit now.", (int)System.Math.Ceiling(creditsTime - (kernel.Time - closingTime))));
            }
            else if (kernel.Time < splashTime)
            {
                // display the splash screen
                kernel.Graphics.RenderTexture(splashIdx, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);
            }
            else
            {
                // display a normal in-game scene
                // the scene has already been rendered at this point
                // we can now add additional graphics to the frame

                // such as: a wireframe, if it is activated
                if (wireOn)
                {
                    kernel.Graphics.WireFrame = true;
                    kernel.Scene.Render(kernel.Graphics);
                    kernel.Graphics.WireFrame = false;
                }

                // and some info text
                kernel.Graphics.RenderText(font, fpsColor, new Point2D(8, 8), fpsText);
                kernel.Graphics.RenderText(font, avgFpsColor, new Point2D(80, 8), avgFpsText);
                kernel.Graphics.RenderText(font, textColor, new Point2D(220, 8), kernel.FramesDrawn + " FRAMES DRAWN");
                if (bsp == null)
                {
                    kernel.Graphics.RenderText(font, textColor, new Point2D(8, 24), String.Format("\nbrighness: {0:F2}\ncontrast: {1:F2}\ngamma: {2:F2}", kernel.Graphics.Brightness, kernel.Graphics.Contrast, kernel.Graphics.Gamma));
                }
                else
                {
                    kernel.Graphics.RenderText(font, textColor, new Point2D(8, 24), String.Format("\nbsp rendering: {0}\nfrustum culling: {1}\nbrighness: {2:F2}\ncontrast: {3:F2}\ngamma: {4:F2}\n\nx={5,-5} y={6,-5} z={7,-5}", bsp.BSPRendering ? "on" : "off", bsp.FrustumCulling ? "on" : "off", kernel.Graphics.Brightness, kernel.Graphics.Contrast, kernel.Graphics.Gamma, (int)player.Position.x, (int)player.Position.y, (int)player.Position.z));
                }
                kernel.Graphics.RenderText(font, textColor, new Point2D(8, vh), versionText);
            }
        }

        private const double fpsUpdateInt = 0.25; // the fps text at the top left of the screen will be updated 4 times per second

        private static string bspFileName;
        private static readonly double splashTime = 2.0;
        private static readonly double creditsTime = 20.0;

        private Player player;
        private Button escape, wire, frustum, forward, backward, left, right, up, down, useBsp, resetPos, f2, f3, f4, f5, f6, f7, f8, ss, cd;
        private bool wireOn, useCd;
        private HlBspMap bsp;
        private int font;
        private Color textColor, fpsColor, avgFpsColor;
        private float fps, avgFps;
        private double fpsTimer;
        private string versionText, fpsText, avgFpsText;
        private int vh; // the height at which the version text will be drawn
        private int splashIdx, creditsIdx;
        private double closingTime;
        private bool closing;
    }
}
