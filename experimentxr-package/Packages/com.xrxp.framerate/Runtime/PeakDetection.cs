using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace XRXP.Modules.FrameRateAnalyser
{
    /// <summary>
    /// PeakDetection is an implementation of the Z-score algorithm used to identify peaks in a signal.
    /// Based on: https://stackoverflow.com/questions/22583391/peak-signal-detection-in-realtime-timeseries-data/22640362#22640362
    /// </summary>
    public class PeakDetection
    {
        private List<double> _y;
        private int _lag;
        private float _threshold;
        private float _influence;
        private List<int> _signals;
        private List<double> _filteredY;
        private double _avgFilter;
        private double _stdFilter;

        public PeakDetection(int lag, float threshold, float influence)
        {
            this._lag = lag;
            this._threshold = threshold;
            this._influence = influence;
            this._y = new List<double>();
        }

        /// <summary>
        /// Detects if a new value is a peak.
        /// </summary>
        /// <param name="newValue">The new value to check</param>
        /// <returns>
        /// Returns 0 if the value is not a peak,
        /// 1 if the peak is above the moving mean,
        /// -1 if the peak is under the moving mean, and
        /// null if there are not enough values to calculate if there is a peak.
        /// </returns>
        public int? IsPeak(double newValue)
        {
            this._y.Add(newValue);
            
            if (this._y.Count < this._lag)
            {
                return null;
            }
            else if (this._y.Count == this._lag)
            {
                this._signals = new List<int>(new int[this._y.Count]);
                this._filteredY = new List<double>(this._y);

                List<double> window = this._y.GetRange(0, this._lag);
                this._avgFilter = window.Average();
                this._stdFilter = this.CalculateStandardDeviation(window);
                return null;
            }
            else
            {
                this._signals.Add(0);
                this._filteredY.Add(0);
                int i = this._y.Count - 1;
                
                if (Math.Abs(this._y[i] - this._avgFilter) > (this._threshold * this._stdFilter))
                {
                    if (this._y[i] > this._avgFilter)
                    {
                        this._signals[i] = 1;
                    }
                    else
                    {
                        this._signals[i] = -1;
                    }
                    this._filteredY[i] = (this._influence * this._y[i]) + ((1 - this._influence) * this._filteredY[i - 1]);
                }
                else
                {
                    this._signals[i] = 0;
                    this._filteredY[i] = this._y[i];
                }
                
                List<double> window = this._filteredY.GetRange(i - this._lag, this._lag);
                this._avgFilter = window.Average();
                this._stdFilter = this.CalculateStandardDeviation(window);

                // Reduce the size of the list
                this._y.RemoveAt(0);
                this._signals.RemoveAt(0);
                this._filteredY.RemoveAt(0);
                
                return this._signals[i - 1];
            }
        }

        /// <summary>
        /// Calculates the standard deviation of a list of values.
        /// </summary>
        private double CalculateStandardDeviation(List<double> values)
        {
            double ret = 0;
            if (values.Count > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt(sum / (values.Count - 1));
            }
            return ret;
        }
    }
}
