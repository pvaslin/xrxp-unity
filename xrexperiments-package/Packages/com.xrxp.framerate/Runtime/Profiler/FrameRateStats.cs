using Unity.Profiling;

namespace XRXP.Modules.FrameRateAnalyser
{
    /// <summary>
    /// Custom profiler counters for frame rate statistics.
    /// https://docs.unity3d.com/Manual/Profiler-creating-custom-counters.html
    /// </summary>
    public static class FrameRateStats
    {
        public static readonly ProfilerCategory PeakCategory = ProfilerCategory.Scripts;

        public const string PeakSignalsName = "Detection of framerate variations";
        public const string FPSName = "FPS Counter";

        public static readonly ProfilerCounterValue<int> PeakSignals =
            new ProfilerCounterValue<int>(PeakCategory, PeakSignalsName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);
        
        public static readonly ProfilerCounterValue<float> FPSCounter =
            new ProfilerCounterValue<float>(PeakCategory, FPSName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);
    }
}
