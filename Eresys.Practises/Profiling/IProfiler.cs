using Eresys.Practises.Logging;

namespace Eresys.Practises.Profiling
{
    public interface IProfiler
    {
        void StartSample(string name);

        void StopSample();

        void WriteReport(ILogger log, string caption);
    }
}
