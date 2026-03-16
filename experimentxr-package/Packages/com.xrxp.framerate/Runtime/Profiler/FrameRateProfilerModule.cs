#if UNITY_EDITOR

using System;
using Unity.Profiling;
using Unity.Profiling.Editor;

namespace XRXP.Modules.FrameRateAnalyser
{
    /// <summary>
    /// Custom Unity Profiler module for frame rate monitoring.
    /// </summary>
    [Serializable]
    [ProfilerModuleMetadata("FrameRate Monitor")]
    public class FrameRateProfilerModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
        {
            new ProfilerCounterDescriptor(FrameRateStats.PeakSignalsName, FrameRateStats.PeakCategory),
            new ProfilerCounterDescriptor(FrameRateStats.FPSName, FrameRateStats.PeakCategory)
        };

        static readonly string[] k_AutoEnabledCategoryNames = new string[]
        {
            ProfilerCategory.Scripts.Name,
            ProfilerCategory.Memory.Name
        };

        public FrameRateProfilerModule() : base(k_Counters, autoEnabledCategoryNames: k_AutoEnabledCategoryNames) { }
    }
}

#endif
