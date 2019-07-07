using Eresys.Practises.Logging;

namespace Eresys.Practises.Profiling
{
    public class NullProfiler : IProfiler
    {
        public ProfilerSample StartSample(string name)
        {
            return new ProfilerSample(this);
        }

        public void StopSample()
        {
            // This is a null profiler
        }

        public void WriteReport(ILogger log, string caption)
        {
            // This is a null profiler
        }
    }
}
