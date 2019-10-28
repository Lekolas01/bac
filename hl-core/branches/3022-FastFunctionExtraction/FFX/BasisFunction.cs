using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction
{

    struct BasisFunction
    {
        public string Var { get; set; }     // e.g. "LOG(Feature1 ** 2)"
        public double[] Val { get; set; }   // this holds the already calculated values, i.e. the value of the function written in Var
        public bool IsOperator { get; set; }// ffx needs to check if basis function has an operator, as it ignores interactions between two nonlinear functions (too complex)
        public NonlinOp Operator { get; }

        public BasisFunction(string var, double[] val, bool isOperator, NonlinOp op = NonlinOp.None)
        {
            this.Var = var;
            this.Val= val;
            this.IsOperator = isOperator;
            this.Operator = op;
        }

        public static BasisFunction operator *(BasisFunction a, BasisFunction b)
        {
            Debug.Assert(a.Val.Length == b.Val.Length);
            double[] newVal = new double[a.Val.Length];
            for(int i = 0; i < a.Val.Length; i++)
            {
                newVal[i] = a.Val[i] * b.Val[i];
            }
            return new BasisFunction(a.Var + " * " + b.Var, newVal, false);
        }

        public int Complexity() => 1;

        public ISymbolicExpressionTree Tree()
        {
            return null;
        }

        public override string ToString() {
            return this.Var;
        }

    }
}
