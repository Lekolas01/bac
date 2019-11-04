using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction {
    class Program {

        private static CultureInfo culture = new CultureInfo("en-US");

        public static bool StringArrToRunData(string[] arr, out RunData data) {
            double train_mse, train_mae, test_mse, test_mae, runtime;
            data = new RunData();
            if (!Double.TryParse(arr[4], NumberStyles.AllowDecimalPoint, culture, out train_mse)) return false;
            if (!Double.TryParse(arr[5], NumberStyles.AllowDecimalPoint, culture, out train_mae)) return false;
            if (!Double.TryParse(arr[6], NumberStyles.AllowDecimalPoint, culture, out test_mse)) return false;
            if (!Double.TryParse(arr[7], NumberStyles.AllowDecimalPoint, culture, out test_mae)) return false;
            if (!Double.TryParse(arr[8], NumberStyles.AllowDecimalPoint, culture, out runtime)) return false;
            data = new RunData(train_mse, train_mae, test_mse, test_mae, runtime);
            return true;
        }

        static void Main(string[] args) {
            int[,] errorAlgs, runtimeAlgs;
            string[] errorAlgNames, runtimeAlgNames;
            CompareAlgs(delegate(KeyValuePair<string, RunData> val1, KeyValuePair<string, RunData> val2) {
                if (val1.Value.test_mse < val2.Value.test_mse) return -1;
                else if (val1.Value.test_mse == val2.Value.test_mse) return 0;
                return 1;
            }, out errorAlgs, out errorAlgNames);
            CompareAlgs(delegate (KeyValuePair<string, RunData> val1, KeyValuePair<string, RunData> val2) {
                if (val1.Value.runtime < val2.Value.runtime) return -1;
                else if (val1.Value.runtime == val2.Value.runtime) return 0;
                return 1;
            }, out errorAlgs, out errorAlgNames);
        }

        private static void CompareAlgs(Comparison<KeyValuePair<string, RunData>> cr, out int[,] alg_scores, out string[] algorithmNames) {
            Dictionary<string, int> algs = new Dictionary<string, int>();
            int numAlgs = 0;

            Dictionary<string, Dictionary<string, RunData>> data_dic = new Dictionary<string, Dictionary<string, RunData>>();
            foreach (var line in File.ReadAllLines(@"D:\LukaLeko\SourceTest\Repos\bachelorarbeit\doc\all_results.csv.txt")) {
                string[] values = line.Split(';');
                Debug.Assert(9 == values.Length);
                string problem_name = values[0];
                string alg_name = values[1];

                RunData runData;
                if (!StringArrToRunData(values, out runData)) continue; // unparsable lines are supposed to be ignored
                if (!algs.ContainsKey(alg_name)) {
                    algs.Add(alg_name, numAlgs);
                    numAlgs++;
                }
                if (!data_dic.ContainsKey(problem_name)) data_dic[problem_name] = new Dictionary<string, RunData>();
                if (!data_dic[problem_name].ContainsKey(alg_name) || data_dic[problem_name][alg_name].test_mse > runData.test_mse) data_dic[problem_name][alg_name] = runData;
            }

            alg_scores = new int[numAlgs, numAlgs];
            algorithmNames = new string[numAlgs];
            int i = 0;
            foreach(var alg in algs) {
                algorithmNames[i] = alg.Key;
                i++;
            }
            foreach (var problem in data_dic) {
                int j = 0;
                var myList = problem.Value.ToList();
                myList.Sort(cr);
                foreach (var alg in myList) {
                    alg_scores[algs[alg.Key], j]++;
                    j++;
                }
            }
        }
    }
}
