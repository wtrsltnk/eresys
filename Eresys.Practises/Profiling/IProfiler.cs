using Eresys.Practises.Logging;
using System;

namespace Eresys.Practises.Profiling
{
    public class ProfilerSample : IDisposable
    {
        public ProfilerSample(IProfiler profiler)
        {
            _profiler = profiler;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private IProfiler _profiler;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _profiler.StopSample();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IProfilerSample() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public interface IProfiler
    {
        ProfilerSample StartSample(string name);

        void StopSample();

        void WriteReport(ILogger log, string caption);
    }
}
