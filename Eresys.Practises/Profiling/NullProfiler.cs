using Eresys.Practises.Logging;

namespace Eresys.Practises.Profiling
{
    public class NullProfiler : IProfiler
    {
        public void StartSample(string name)
        {
            // This is a null profiler
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
