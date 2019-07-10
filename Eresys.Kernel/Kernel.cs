using Eresys.Practises.Logging;
using Eresys.Practises.Profiling;
using System;
using System.Windows.Forms;

namespace Eresys
{
    /// <summary>
    /// Dit is zowat de belangerijkste klasse van de engine. Het bundelt namelijk alles. Het is via deze klasse dat de engine
    /// wordt opgestart, en dat alle componenten kunnen worden aangesproken (zoals graphics, controls, scene,...)
    /// </summary>
    public class Kernel
    {
        /// <summary>
        /// Geeft aan of de kernel gestart en geïnitilaseerd is
        /// </summary>
        public bool Running { get; private set; } = false;

        /// <summary>
        /// Geeft aan of de toepassing actief (= heeft focus) is
        /// </summary>
        public bool Active { get; private set; } = false;

        /// <summary>
        /// Het hoofd-logbestand dat door de kernel wordt gebruikt, maar ook door andere componenten mag worden gebruikt
        /// </summary>
        public ILogger Logger { get; set; } = new ConsoleLogger();

        /// <summary>
        /// De settings
        /// </summary>
        public Settings Settings { get; set; } = null;

        /// <summary>
        /// Geeft aan hoelang de applicatie in totaal al actief is, in seconden
        /// </summary>
        public double Time { get { return timer.Time; } }

        /// <summary>
        /// Geeft aan hoelang het laatst gerenderde frame geduurt heeft
        /// </summary>
        public double Interval { get { return timer.Interval; } }

        /// <summary>
        /// De Graphics component
        /// </summary>
        public IGraphics Graphics { get; set; } = null;

        /// <summary>
        /// De Controls component
        /// </summary>
        public IControls Controls { get; set; } = null;

        /// <summary>
        /// De scene
        /// </summary>
        public Scene Scene { get; set; } = null;

        /// <summary>
        /// Het aantal frames per seconde dat op dit moment gehaald wordt
        /// </summary>
        public float FPS { get; private set; } = 0.0f;

        /// <summary>
        /// Het aantal frames gerenderd
        /// </summary>
        public long FramesDrawn { get; private set; } = 0;

        /// <summary>
        /// Het gemiddelde aantal frames per seconde gemeten sinds de engine werd opgestart
        /// </summary>
        public float AverageFPS
        {
            get
            {
                if (FramesDrawn <= fpsMeasDelay)
                {
                    return 0.0f;
                }

                return (float)((FramesDrawn - fpsMeasDelay) / (timer.Time - fpsOfsTime));
            }
        }

        public IProfiler Profiler { get; set; } = null;

        private IApplication _app;

        /// <summary>
        /// Start de kernel
        /// </summary>
        /// <param name="app">De applicatie, een object van een klasse die IApplication implementeerd</param>
        /// <param name="logFileName">logbestand</param>
        /// <param name="iniFileName">inibestand</param>
        /// <returns></returns>
        public int Run(IApplication app, string logFileName)
        {
            _app = app;

            try
            {
                // init Graphics
                Graphics.ContextCreated += Graphics_ContextCreated;
                Graphics.ContextDestroying += Graphics_ContextDestroying;
                Graphics.Update += Graphics_Update;
                Graphics.Render += Graphics_Render;
                Graphics.Activated += Graphics_Activate;
                Graphics.Deactivate += Graphics_Deactivate;
                Graphics.Closed += Graphics_Closed;

                Graphics.Startup();

                while (Running)
                {
                    // process application events
                    Application.DoEvents();
                }

                return 0; // geef nul terug. Dit geeft aan dat de toepassing normaal is beëindigt
            }
            catch (Exception e)
            {
                // als er een exceptie is opgedoken sluit deze code de engine af
                Running = false;
                Cursor.Show();

                Logger.WithException(e)
                    .Log(LogLevels.Error, "Eresys terminated due to fatal exception!");

                return e.GetHashCode(); // geef een waarde terug. Dit geeft aan dat de toepassing is beëindigt door een fout
            }
        }

        private void Graphics_ContextCreated(object sender, EventArgs e)
        {
            // init kernel (=main) timer
            timer = new Timer();

            // init scene
            Scene = new Scene();

            // init profiler
            Eresys.Profiler.IsProfilingEnabled = bool.Parse(Settings["profiler"]);
            Profiler = new Profiler();

            // start application
            _app.Startup();

            // start main loop
            Logger.Log(LogLevels.Info, "Entering Main Loop...");

            Running = true;
            timer.Pause = false;
        }

        private void Graphics_Update(object sender, EventArgs e)
        {
            if (!Active)
            {
                return;
            }

            // upate controls
            using (Profiler.StartSample("update controls"))
            {
                Controls.Update();
            }

            // update gamestate
            using (Profiler.StartSample("update app"))
            {
                _app.Update();
            }

            // update scene
            using (Profiler.StartSample("update scene"))
            {
                Scene.Update((float)Interval);
            }
        }

        private void Graphics_Render(object sender, EventArgs e)
        {
            if (!Active)
            {
                return;
            }

            // render frame
            using (Profiler.StartSample("rendering"))
            {
                using (Profiler.StartSample("begin frame"))
                {
                    Graphics.BeginFrame();
                }

                using (Profiler.StartSample("render scene"))
                {
                    Scene.Render(Graphics);
                }

                using (Profiler.StartSample("render app"))
                {
                    _app.Render();
                }

                using (Profiler.StartSample("end frame"))
                {
                    Graphics.EndFrame();
                }
            }

            // frame timing
            using (Profiler.StartSample("frame timing"))
            {
                timer.Update();
                FPS = 1.0f / (float)timer.Interval;
                if (FramesDrawn == fpsMeasDelay) fpsOfsTime = timer.Time;
                FramesDrawn++;
            }
        }

        private void Graphics_ContextDestroying(object sender, EventArgs e)
        {
            timer.Pause = true;

            // terminate application
            _app.Terminate();

            // write kernel performance report
            Profiler.WriteReport(new ConsoleLogger(), "kernel main loop");

            // some logging...
            Logger.Log(LogLevels.Info, "Eresys normally terminated.");

            Logger.Log(LogLevels.Info, $"Running time:      {timer.Time.ToString("F4")} seconds");
            Logger.Log(LogLevels.Info, $"Frames rendered:   {FramesDrawn}");
            Logger.Log(LogLevels.Info, $"Average FPS:       {AverageFPS.ToString("F4")}");
            Logger.Log(LogLevels.Info, $"Last Measured FPS: {FPS.ToString("F4")}");
        }

        /// <summary>
        /// Beëindigt de engine, en dus ook de toepassing
        /// </summary>
        public void Terminate()
        {
            Running = false;
        }

        private void Graphics_Closed(object sender, EventArgs e)
        {
            Terminate();
        }

        // activeert de kernel
        private void Graphics_Activate(object sender, EventArgs e)
        {
            Active = true;
        }

        // zet de kernel in een soort slaapstand
        private void Graphics_Deactivate(object sender, EventArgs e)
        {
            Active = false;
        }

        private readonly long fpsMeasDelay = 30;
        private Timer timer = null;
        private double fpsOfsTime = 0.0;
    }
}
