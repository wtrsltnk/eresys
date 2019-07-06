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
        public static bool Running { get; private set; } = false;

        /// <summary>
        /// Geeft aan of de toepassing actief (= heeft focus) is
        /// </summary>
        public static bool Active { get; private set; } = false;

        /// <summary>
        /// Het hoofd-logbestand dat door de kernel wordt gebruikt, maar ook door andere componenten mag worden gebruikt
        /// </summary>
        public static ILogger Logger { get; set; } = new ConsoleLogger();

        /// <summary>
        /// Het hoofdvenster
        /// </summary>
        public static Form Form { get; private set; } = null;

        /// <summary>
        /// De settings
        /// </summary>
        public static Settings Settings { get; private set; } = null;

        /// <summary>
        /// Geeft aan hoelang de applicatie in totaal al actief is, in seconden
        /// </summary>
        public static double Time { get { return timer.Time; } }

        /// <summary>
        /// Geeft aan hoelang het laatst gerenderde frame geduurt heeft
        /// </summary>
        public static double Interval { get { return timer.Interval; } }

        /// <summary>
        /// De Graphics component
        /// </summary>
        public static IGraphics Graphics { get; } = null;

        /// <summary>
        /// De Controls component
        /// </summary>
        public static IControls Controls { get; } = null;

        /// <summary>
        /// De scene
        /// </summary>
        public static Scene Scene { get; private set; } = null;

        /// <summary>
        /// Het aantal frames per seconde dat op dit moment gehaald wordt
        /// </summary>
        public static float FPS { get; private set; } = 0.0f;

        public static long FramesDrawn { get; private set; } = 0;

        /// <summary>
        /// Het gemiddelde aantal frames per seconde gemeten sinds de engine werd opgestart
        /// </summary>
        public static float AverageFPS
        {
            get
            {
                float res = 0.0f;
                if (FramesDrawn > fpsMeasDelay) res = (float)((double)(FramesDrawn - fpsMeasDelay) / (timer.Time - fpsOfsTime));
                return res;
            }
        }

        public static IProfiler Profiler { get; private set; } = null;

        /// <summary>
        /// Start de kernel
        /// </summary>
        /// <param name="app">De applicatie, een object van een klasse die IApplication implementeerd</param>
        /// <param name="logFileName">logbestand</param>
        /// <param name="iniFileName">inibestand</param>
        /// <returns></returns>
        public static int Startup(IApplication app, string logFileName, string iniFileName)
        {
            try
            {
                // init form
                Form = new Form();
                Form.Activated += new EventHandler(Activate);
                Form.Deactivate += new EventHandler(Deactivate);

                // init settings
                Settings = new Settings(iniFileName);

                // init kernel (=main) timer
                timer = new Timer();

                // init graphics
                switch (Settings["graphics"])
                {
                    case "directx":
                        //graphics = new DXGraphics();
                        break;
                    default:
                        throw new InvalidOperationException("Graphics renderer " + Settings["graphics"] + " invalid!");
                }

                // init controls
                switch (Settings["controls"])
                {
                    case "directx":
                        //controls = new DXControls();
                        break;
                    default:
                        throw new InvalidOperationException("Controls manager " + Settings["controls"] + " invalid!");
                }

                // init scene
                Scene = new Scene();

                // init profiler
                Eresys.Profiler.IsProfilingEnabled = bool.Parse(Settings["profiler"]);
                Profiler = new Profiler();

                // start application
                app.Startup();

                // start main loop
                Logger.Log(LogLevels.Info, "Entering Main Loop...");

                Running = true;
                timer.Pause = false;
                while (Running)
                {
                    Profiler.StartSample("MAIN");
                    if (Active)
                    {
                        // render frame
                        Profiler.StartSample("rendering");

                        Profiler.StartSample("begin frame");
                        Graphics.BeginFrame();
                        Profiler.StopSample();

                        Profiler.StartSample("render scene");
                        Scene.Render(Graphics);
                        Profiler.StopSample();

                        Profiler.StartSample("render app");
                        app.Render();
                        Profiler.StopSample();

                        Profiler.StartSample("end frame");
                        Graphics.EndFrame();
                        Profiler.StopSample();

                        Profiler.StopSample();

                        // frame timing
                        Profiler.StartSample("frame timing");
                        timer.Update();
                        FPS = 1.0f / (float)timer.Interval;
                        if (FramesDrawn == fpsMeasDelay) fpsOfsTime = timer.Time;
                        FramesDrawn++;
                        Profiler.StopSample();
                    }

                    // process application events
                    System.Windows.Forms.Application.DoEvents();

                    if (Active)
                    {
                        // upate controls
                        Profiler.StartSample("update controls");
                        Controls.Update();
                        Profiler.StopSample();

                        // update gamestate
                        Profiler.StartSample("update app");
                        app.Update();
                        Profiler.StopSample();

                        // update scene
                        Profiler.StartSample("update scene");
                        Scene.Update((float)Interval);
                        Profiler.StopSample();
                    }
                    Profiler.StopSample();
                }
                timer.Pause = true;

                // terminate application
                app.Terminate();

                // finish some business
                Graphics.Dispose();
                Form.Dispose();

                // write kernel performance report
                Profiler.WriteReport(new ConsoleLogger(), "kernel main loop");

                // some logging...
                Logger.Log(LogLevels.Info, "Eresys normally terminated.");

                Logger.Log(LogLevels.Info, $"Running time:      {timer.Time.ToString("F4")} seconds");
                Logger.Log(LogLevels.Info, $"Frames rendered:   {FramesDrawn}");
                Logger.Log(LogLevels.Info, $"Average FPS:       {AverageFPS.ToString("F4")}");
                Logger.Log(LogLevels.Info, $"Last Measured FPS: {FPS.ToString("F4")}");
            }
            catch (Exception e)
            {
                // als er een exceptie is opgedoken sluit deze code de engine af
                Running = false;
                Cursor.Show();
                Form.Hide();

                Logger.WithException(e)
                    .Log(LogLevels.Error, "Eresys terminated due to fatal exception!");

                return e.GetHashCode(); // geef een waarde terug. Dit geeft aan dat de toepassing is beëindigt door een fout
            }

            return 0; // geef nul terug. Dit geeft aan dat de toepassing normaal is beëindigt
        }

        /// <summary>
        /// Beëindigt de engine, en dus ook de toepassing
        /// </summary>
        public static void Terminate()
        {
            Running = false;
        }

        // activeert de kernel
        private static void Activate(object sender, EventArgs e)
        {
            Active = true;
        }

        // zet de kernel in een soort slaapstand
        private static void Deactivate(object sender, EventArgs e)
        {
            Active = false;
        }

        private static long fpsMeasDelay = 30;
        private static Timer timer = null;
        private static double fpsOfsTime = 0.0;
    }
}
