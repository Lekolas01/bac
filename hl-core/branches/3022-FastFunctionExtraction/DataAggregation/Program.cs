using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DataAggregation {
    class Program {


        public static bool StringArrToRunData(string[] arr, out RunData data) {
            double train_mse, train_mae, test_mse, test_mae, runtime;
            data = new RunData();
            if (!Double.TryParse(arr[4], /*NumberStyles.AllowDecimalPoint, null,*/ out train_mse)) return false;
            if (!Double.TryParse(arr[5], /*NumberStyles.AllowDecimalPoint, null,*/ out train_mae)) return false;
            if (!Double.TryParse(arr[6], /*NumberStyles.AllowDecimalPoint, null,*/ out test_mse)) return false;
            if (!Double.TryParse(arr[7], /*NumberStyles.AllowDecimalPoint, null,*/ out test_mae)) return false;
            if (!Double.TryParse(arr[8], /*NumberStyles.AllowDecimalPoint, null,*/ out runtime)) return false;
            data = new RunData(train_mse, train_mae, test_mse, test_mae, runtime);
            return true;
        }

        static void Main(string[] args) {
            var scores = CompareAlgs();
        }

        private static int[,] CompareAlgs() {
            Dictionary<string, int> algs = new Dictionary<string, int>();
            int numAlgs = 0;

            Dictionary<string, Dictionary<string, RunData>> data_dic = new Dictionary<string, Dictionary<string, RunData>>();
            foreach (var line in File.ReadAllLines(@"D:\LukaLeko\SourceTest\Repos\bachelorarbeit\doc\all_results.csv.txt")) {
                string[] values = line.Split(';');
                Debug.Assert(9 == values.Length);
                string problem_name = values[0];
                string alg_name = values[1];


                RunData runData;
                if (!StringArrToRunData(values, out runData)) continue;
                if (!algs.ContainsKey(alg_name)) {
                    algs.Add(alg_name, numAlgs);
                    numAlgs++;
                }
                if (!data_dic.ContainsKey(problem_name)) {
                    data_dic[problem_name] = new Dictionary<string, RunData>();
                }
                if (!data_dic[problem_name].ContainsKey(alg_name) || data_dic[problem_name][alg_name].test_mse > runData.test_mse) data_dic[problem_name][alg_name] = runData;
            }

            int numProblems = data_dic.Count;
            int[,] alg_scores = new int[numAlgs, numAlgs];
            int i = 0;
            foreach (var problem in data_dic) {
                var myList = problem.Value.ToList();
                myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                int j = 0;
                foreach (var alg in myList) {
                    alg_scores[algs[alg.Key], j]++;
                    j++;
                }
            }
            return alg_scores;
        }
    }
}
