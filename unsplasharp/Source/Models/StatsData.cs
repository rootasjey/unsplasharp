using System.Collections.Generic;

namespace Unsplasharp.Models {
    /// <summary>
    /// Data statistics.
    /// </summary>
    public class StatsData {
        /// <summary>
        /// Total count of a specific stat.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Stat's history.
        /// </summary>
        public StatsHistorical Historical { get; set; }
    }

    /// <summary>
    /// Stat's history.
    /// </summary>
    public class StatsHistorical {
        /// <summary>
        /// Total number of stat for the past 'quantity' 'resolution' (eg. 30 days).
        /// </summary>
        public double Change { get; set; }

        /// <summary>
        /// Average number of stat for the past 'quantity' 'resolution' (eg. 30 days).
        /// </summary>
        public int Average { get; set; }

        /// <summary>
        /// The frequency of the stats.
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// The amount of for each stat.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// List of data sets.
        /// </summary>
        public List<StatsValue> Values { get; set; }
    }

    /// <summary>
    /// A data set.
    /// </summary>
    public class StatsValue {
        /// <summary>
        /// Stat's date.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Stat's value.
        /// </summary>
        public double Value { get; set; }
    }
}
