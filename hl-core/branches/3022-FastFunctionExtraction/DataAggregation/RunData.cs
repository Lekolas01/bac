using System;
using System.Collections.Generic;
using System.Text;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction {
    struct RunData {
        public double train_mse { get; set; }
        public double train_mae { get; set; }
        public double test_mse { get; set; }
        public double test_mae { get; set; }
        public double runtime { get; set; }

        public RunData(double train_mse, double train_mae, double test_mse, double test_mae, double runtime) {
            this.train_mse = train_mse;
            this.train_mae = train_mae;
            this.test_mse = test_mse;
            this.test_mae = test_mae;
            this.runtime = runtime;
        }

        internal int CompareTo(RunData value) {
            if (test_mse < value.test_mse) return -1;
            else if (test_mse == value.test_mse) return 0;
            return 1;
        }
    }
}
