using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataAggregation {
    class Program {

        public static bool ToRunData(string[] arr, out RunData data) {
            double train_mse, train_mae, test_mse, test_mae, runtime;
            data = new RunData();
            if (!Double.TryParse(arr[4], out train_mse)) return false;
            if (!Double.TryParse(arr[5], out train_mae)) return false;
            if (!Double.TryParse(arr[6], out test_mse)) return false;
            if (!Double.TryParse(arr[7], out test_mae)) return false;
            if (!Double.TryParse(arr[8], out runtime)) return false;
            data = new RunData(train_mse, train_mae, test_mse, test_mae, runtime);
            return true;
        }

        static void Main(string[] args) {
            List<string> algorithms = new List<string>();

            Dictionary<string, int> algs = new Dictionary<string, int>();
            //         dataset-name       alg-name            
            Dictionary<string, Dictionary<string, RunData>> data_dic = new Dictionary<string, Dictionary<string, RunData>>();
            foreach (var line in File.ReadAllLines(@"D:\LukaLeko\SourceTest\Repos\bachelorarbeit\doc\all_results.csv.txt")) {
                string[] values = line.Split(';');
                Debug.Assert(9 == values.Length);
                string problem_name = values[0];
                string alg_name = values[1];

                RunData runData;
                if (!ToRunData(values, out runData)) continue;
                if (!data_dic.ContainsKey(problem_name)) {
                    data_dic[problem_name] = new Dictionary<string, RunData>();
                    algorithms.Add(problem_name);
                }
                if (!data_dic[problem_name].ContainsKey(alg_name) || data_dic[problem_name][alg_name].test_mse > runData.test_mse) data_dic[problem_name][alg_name] = runData;
            }

            int numProblems = data_dic.Count;
            int numAlgs = algorithms.Count;

            int[,] alg_scores = new int[numAlgs, numAlgs];

            int i = 0;
            foreach (var problem in data_dic) {
                int j = 0;
                foreach (var alg in problem.Value) {

                }
            }

        }
    }
}
