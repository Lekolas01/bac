using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction
{
    public enum NonlinOp { None, Abs, Log, Sin, Cos };
    static class Operator
    {
        
        public static string ToString(NonlinOp op, string val) => op != NonlinOp.None ? op.ToString() + "(" + val + ")" : val;
    }
}
