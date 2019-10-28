using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction
{
    // A GLM is a function in the form of a linear combination of basis functions (plus an offset)
    class GeneralizedLinearModel
    {
        public double Offset { get; }
        public double[] Coefficients { get; }
        public BasisFunction[] BasisFunctions { get; }
        public int Complexity() => BasisFunctions.Length + BasisFunctions.Sum(x => x.Complexity());

        public GeneralizedLinearModel(double offset, double[] coefficients, BasisFunction[] basisFunctions)
        {
            Offset = offset;
            Coefficients = coefficients;
            BasisFunctions = basisFunctions;
        }

        public double eval()
        {
            Debug.Assert(Coefficients.Length == BasisFunctions.Length);
            return 0;
        }
    }
}
