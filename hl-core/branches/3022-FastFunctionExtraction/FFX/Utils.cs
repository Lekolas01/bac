using HeuristicLab.Optimization;
using HeuristicLab.Problems.DataAnalysis.Symbolic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction {
    internal static class Utils {
        // returns n logarithmically evenly spaced double values ranging from start to end
        public static double[] logspace(double start, double end, int n) {
            if (start <= 0) throw new ArgumentException(nameof(start));
            if (end <= 0) throw new ArgumentException(nameof(end));
            if (n <= 1) throw new ArgumentException(nameof(n));
            double d = (double)n, p = end / start;
            return Enumerable.Range(0, n).Select(i => start * Math.Pow(p, i / d)).ToArray();
        }

        // return the nth row of a matrix
        public static T[] GetRow<T>(T[,] matrix, int n) {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[n, i];
            return array;
        }

        // returns all row indices of the models in coeffs that are supposed to be in a pareto front
        public static IEnumerable<T> NondominatedFilter<T>(T[] models, double[,] coeff, double[] error, Func<double[], int> complexity) {
            double[][] qualities = new double[coeff.GetLength(0)][];
            for (int i = 0; i < coeff.GetLength(0); i++) {
                qualities[i] = new double[2];
                qualities[i][0] = error[i];
                qualities[i][1] = complexity(GetRow(coeff, i));
            }
            var front = DominationCalculator<T>.CalculateBestParetoFront(models, qualities, new bool[2] { true, true });
            return front.Select(val => val.Item1);
        }

        public static double eval(OpCode op, double x) {
            switch (op) {
                case OpCode.Absolute:
                    return Math.Abs(x);
                case OpCode.Log:
                    return Math.Log10(x);
                case OpCode.Sin:
                    return Math.Sin(x);
                case OpCode.Cos:
                    return Math.Cos(x);
                default:
                    throw new Exception("Unimplemented operator: " + op.ToString());
            }
        }
    }
}
