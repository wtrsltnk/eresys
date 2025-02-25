using Eresys.Practises.Logging;
using Eresys.Practises.Profiling;
using System.Collections;

namespace Eresys
{
    public class Profiler : IProfiler
    {
        public static bool IsProfilingEnabled
        {
            get { return enabled; }
            set { if (!started) enabled = value; }
        }

        public Profiler()
        {
            started = true;
            if (!enabled) return;
            samples = new ArrayList();
            activeSample = null;
            rootSample = null;
            rootN = 0;
            totalTime = 0.0;
            timer = new Timer();
            timer.Pause = false;
        }

        public ProfilerSample StartSample(string name)
        {
            if (!enabled) return new ProfilerSample(this);

            if (activeSample != null)
            {
                timer.Update();
                activeSample.interval += timer.Interval;
                totalTime += timer.Interval;
            }
            Sample sample = null;
            for (int i = 0; i < samples.Count; i++)
            {
                if (((Sample)samples[i]).name == name)
                {
                    sample = (Sample)samples[i];
                    break;
                }
            }
            if (sample == null)
            {
                sample = new Sample();
                sample.name = name;
                sample.time = 0.0;
                sample.time = 0.0;
                sample.peek = 0.0;
                sample.parent = activeSample;
                sample.children = new ArrayList();
                if (activeSample != null) activeSample.children.Add(sample);
                samples.Add(sample);
            }
            activeSample = sample;
            if (rootSample == null) rootSample = sample;
            timer.Update();

            return new ProfilerSample(this);
        }

        public void StopSample()
        {
            if (!enabled) return;
            timer.Update();
            activeSample.interval += timer.Interval;
            totalTime += timer.Interval;
            if (activeSample == rootSample)
            {
                rootN++;
                for (int i = 0; i < samples.Count; i++)
                {
                    Sample sample = (Sample)samples[i];
                    sample.time += sample.interval;
                    if (sample.interval > sample.peek) sample.peek = sample.interval;
                    sample.interval = 0.0;
                }
            }
            activeSample = activeSample.parent;
        }

        public void WriteReport(ILogger log, string caption)
        {
            if (!enabled) return;
            string title = "Performance Profile for " + caption + ":";
            log.Log(LogLevels.Info, title);
            log.Log(LogLevels.Info, new string('=', title.Length));
            log.Log(LogLevels.Info, "");
            if (activeSample != null)
            {
                log.Log(LogLevels.Info, "  Error in sample sequence!");
                return;
            }
            log.Log(LogLevels.Info, "                  sample                 | avrg(s) | avrg(%) | peek(s)");
            log.Log(LogLevels.Info, "  ---------------------------------------+---------+---------+--------");
            for (int i = 0; i < samples.Count; i++) ((Sample)samples[i]).childIdx = 0;
            int level = 0;
            Sample sample = rootSample;
            double sampleTime = totalTime / rootN;
            do
            {
                if (sample.childIdx == 0)
                {
                    string name = "  ";
                    for (int l = 0; l < level; l++) name += "| ";
                    name += "+ " + sample.name + " ";
                    while (name.Length < 40) name += ".";
                    double time = sample.time / rootN;
                    log.Log(LogLevels.Info, string.Format("{0} |{1,8:F4} |{2,8:F4} |{3,8:F4}", name, time, time * 100.0 / sampleTime, sample.peek));
                }
                if (sample.childIdx < sample.children.Count)
                {
                    sample = (Sample)sample.children[sample.childIdx++];
                    level++;
                }
                else
                {
                    sample = sample.parent;
                    level--;
                }
            }
            while (sample != null);
        }

        private class Sample
        {
            public string name;
            public double interval;
            public double time;
            public double peek;
            public Sample parent;
            public ArrayList children;
            public int childIdx;
        }

        private Timer timer;
        private ArrayList samples;
        private Sample activeSample;
        private Sample rootSample;
        private uint rootN;
        private double totalTime;
        private static bool enabled = false;
        private static bool started = false;
    }
}
