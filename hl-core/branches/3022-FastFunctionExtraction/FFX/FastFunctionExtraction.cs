﻿using System;
using System.Threading;
using System.Linq;
using HeuristicLab.Common; // required for parameters collection
using HeuristicLab.Core; // required for parameters collection
using HeuristicLab.Data; // IntValue, ...
using HeuristicLab.Encodings.BinaryVectorEncoding;
using HeuristicLab.Optimization; // BasicAlgorithm
using HeuristicLab.Parameters;
using HeuristicLab.Problems.Binary;
using HeuristicLab.Random; // MersenneTwister
using HEAL.Attic;
using HeuristicLab.Algorithms.DataAnalysis.Glmnet;
using HeuristicLab.Problems.DataAnalysis;
using System.Collections.Generic;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using System.Collections;
using System.Diagnostics;
using HeuristicLab.Problems.DataAnalysis.Symbolic;
using HeuristicLab.Problems.DataAnalysis.Symbolic.Regression;
using HeuristicLab.Analysis;
using HeuristicLab.Collections;

namespace HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction
{

    [Item(Name = "FastFunctionExtraction", Description = "An FFX algorithm.")]
    [Creatable(Category = CreatableAttribute.Categories.Algorithms, Priority = 999)]
    [StorableType("689280F7-E371-44A2-98A5-FCEDF22CA343")] // for persistence (storing your algorithm to a files or transfer to HeuristicLab.Hive
    public sealed class FastFunctionExtraction : FixedDataAnalysisAlgorithm<RegressionProblem>
    {

        private static readonly double[] exponents = { 0.5, 1, 2 };
        private static readonly OpCode[] nonlinFuncs = { OpCode.Absolute, OpCode.Log, OpCode.Sin, OpCode.Cos };

        private static readonly BidirectionalDictionary<OpCode, string> OpCodeToString = new BidirectionalDictionary<OpCode, string> {
        { OpCode.Log, "LOG" },
        { OpCode.Absolute, "ABS"},
        { OpCode.Sin, "SIN"},
        { OpCode.Cos, "COS"},
        { OpCode.Square, "SQR"},
        { OpCode.SquareRoot, "SQRT"},
        { OpCode.Cube, "CUBE"},
        { OpCode.CubeRoot, "CUBEROOT"}
    };

        private const string ConsiderInteractionsParameterName = "Consider Interactions";
        private const string ConsiderDenominationParameterName = "Consider Denomination";
        private const string ConsiderExponentiationParameterName = "Consider Exponentiation";
        private const string ConsiderHingeFuncsParameterName = "Consider Hinge Functions";
        private const string ConsiderNonlinearFuncsParameterName = "Consider Nonlinear functions";
        private const string NonlinearFuncsParameterName = "Nonlinear Functions";
        private const string PenaltyParameterName = "Penalty";
        private const string LambdaParameterName = "Lambda";
        private const string MaxNumBasisFuncsParameterName = "Maximum Number of Basis Functions";
        private const string VerboseParameterName = "Verbose";

        #region parameters
        public IValueParameter<BoolValue> ConsiderInteractionsParameter {
            get { return (IValueParameter<BoolValue>)Parameters[ConsiderInteractionsParameterName]; }
        }
        public IValueParameter<BoolValue> ConsiderDenominationsParameter {
            get { return (IValueParameter<BoolValue>)Parameters[ConsiderDenominationParameterName]; }
        }
        public IValueParameter<BoolValue> ConsiderExponentiationsParameter {
            get { return (IValueParameter<BoolValue>)Parameters[ConsiderExponentiationParameterName]; }
        }
        public IValueParameter<BoolValue> ConsiderNonlinearFuncsParameter {
            get { return (IValueParameter<BoolValue>)Parameters[ConsiderNonlinearFuncsParameterName]; }
        }
        public IValueParameter<BoolValue> ConsiderHingeFuncsParameter {
            get { return (IValueParameter<BoolValue>)Parameters[ConsiderHingeFuncsParameterName]; }
        }
        public IValueParameter<DoubleValue> PenaltyParameter {
            get { return (IValueParameter<DoubleValue>)Parameters[PenaltyParameterName]; }
        }
        public IValueParameter<DoubleValue> LambdaParameter {
            get { return (IValueParameter<DoubleValue>)Parameters[LambdaParameterName]; }
        }
        public IValueParameter<CheckedItemCollection<EnumValue<OpCode>>> NonlinearFuncsParameter {
            get { return (IValueParameter<CheckedItemCollection<EnumValue<OpCode>>>)Parameters[NonlinearFuncsParameterName]; }
        }
        public IValueParameter<IntValue> MaxNumBasisFuncsParameter {
            get { return (IValueParameter<IntValue>)Parameters[MaxNumBasisFuncsParameterName]; }
        }

        public IValueParameter<BoolValue> VerboseParameter {
            get { return (IValueParameter<BoolValue>)Parameters[VerboseParameterName]; }
        }

        #endregion

        #region properties
        public bool ConsiderInteractions {
            get { return ConsiderInteractionsParameter.Value.Value; }
            set { ConsiderInteractionsParameter.Value.Value = value; }
        }
        public bool ConsiderDenominations {
            get { return ConsiderDenominationsParameter.Value.Value; }
            set { ConsiderDenominationsParameter.Value.Value = value; }
        }
        public bool ConsiderExponentiations {
            get { return ConsiderExponentiationsParameter.Value.Value; }
            set { ConsiderExponentiationsParameter.Value.Value = value; }
        }
        public bool ConsiderNonlinearFuncs {
            get { return ConsiderNonlinearFuncsParameter.Value.Value; }
            set { ConsiderNonlinearFuncsParameter.Value.Value = value; }
        }
        public bool ConsiderHingeFuncs {
            get { return ConsiderHingeFuncsParameter.Value.Value; }
            set { ConsiderHingeFuncsParameter.Value.Value = value; }
        }
        public double Penalty {
            get { return PenaltyParameter.Value.Value; }
            set { PenaltyParameter.Value.Value = value; }
        }
        public DoubleValue Lambda {
            get { return LambdaParameter.Value; }
            set { LambdaParameter.Value = value; }
        }
        public CheckedItemCollection<EnumValue<OpCode>> NonlinearFuncs {
            get { return NonlinearFuncsParameter.Value; }
            set { NonlinearFuncsParameter.Value = value; }
        }
        public int MaxNumBasisFuncs {
            get { return MaxNumBasisFuncsParameter.Value.Value; }
            set { MaxNumBasisFuncsParameter.Value.Value = value; }
        }

        public bool Verbose {
            get { return VerboseParameter.Value.Value; }
            set { VerboseParameter.Value.Value = value; }
        }
        #endregion

        [StorableConstructor]
        private FastFunctionExtraction(StorableConstructorFlag _) : base(_) { }
        public FastFunctionExtraction(FastFunctionExtraction original, Cloner cloner) : base(original, cloner)
        {
        }
        public FastFunctionExtraction() : base()
        {
            var items = new CheckedItemCollection<EnumValue<OpCode>>();
            foreach (var op in nonlinFuncs)
            {
                items.Add(new EnumValue<OpCode>(op));
            }
            base.Problem = new RegressionProblem();
            Parameters.Add(new ValueParameter<BoolValue>(ConsiderInteractionsParameterName, "True if you want the models to include interactions, otherwise false.", new BoolValue(true)));
            Parameters.Add(new ValueParameter<BoolValue>(ConsiderDenominationParameterName, "True if you want the models to include denominations, otherwise false.", new BoolValue(false)));
            Parameters.Add(new ValueParameter<BoolValue>(ConsiderExponentiationParameterName, "True if you want the models to include exponentiation, otherwise false.", new BoolValue(true)));
            Parameters.Add(new ValueParameter<BoolValue>(ConsiderNonlinearFuncsParameterName, "True if you want the models to include nonlinear functions(abs, log,...), otherwise false.", new BoolValue(true)));
            Parameters.Add(new ValueParameter<BoolValue>(ConsiderHingeFuncsParameterName, "True if you want the models to include Hinge Functions, otherwise false.", new BoolValue(false)));
            Parameters.Add(new FixedValueParameter<DoubleValue>(PenaltyParameterName, "Penalty factor (alpha) for balancing between ridge (0.0) and lasso (1.0) regression", new DoubleValue(0.9)));
            Parameters.Add(new OptionalValueParameter<DoubleValue>(LambdaParameterName, "Optional: the value of lambda for which to calculate an elastic-net solution. lambda == null => calculate the whole path of all lambdas"));
            Parameters.Add(new ValueParameter<CheckedItemCollection<EnumValue<OpCode>>>(NonlinearFuncsParameterName, "What nonlinear functions the models should be able to include.", items));
            Parameters.Add(new ValueParameter<IntValue>(MaxNumBasisFuncsParameterName, "Maximum Number of Basis Functions in the final models.", new IntValue(15)));
            Parameters.Add(new ValueParameter<BoolValue>(VerboseParameterName, "Verbose?", new BoolValue(false)));
        }

        [StorableHook(HookType.AfterDeserialization)]
        private void AfterDeserialization() { }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new FastFunctionExtraction(this, cloner);
        }

        public override Type ProblemType { get { return typeof(RegressionProblem); } }
        public new RegressionProblem Problem { get { return (RegressionProblem)base.Problem; } }

        public override bool SupportsPause { get { return true; } }

        private Stopwatch sw = new Stopwatch();

        // for debug purposes
        private static TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        protected override void Run(CancellationToken cancellationToken)
        {
            var basisFunctions = createBasisFunctions(Problem.ProblemData);
            if (Verbose) Results.Add(new Result(
              "Basis Functions",
              "A Dataset consisting of the generated Basis Functions from FFX Alg Step 1.",
              createProblemData(Problem.ProblemData, basisFunctions)
            ));

            // add denominator bases to the already existing basis functions
            if (ConsiderDenominations) basisFunctions = basisFunctions.Concat(createDenominatorBases(Problem.ProblemData, basisFunctions)).ToList();

            // create either path of solutions, or one solution for given lambda
            LearnModels(Problem.ProblemData, basisFunctions);
        }

        private List<BasisFunction> createBasisFunctions(IRegressionProblemData problemData)
        {
            var basisFunctions = createUnivariateBases(problemData);

            if (ConsiderInteractions)
                basisFunctions = basisFunctions.Concat(createMultivariateBases(basisFunctions)).ToList();
            return basisFunctions;
        }

        private List<BasisFunction> createUnivariateBases(IRegressionProblemData problemData)
        {
            var B1 = new List<BasisFunction>();
            var inputVariables = problemData.AllowedInputVariables;
            var validExponents = ConsiderExponentiations ? exponents : new double[] { 1 };
            var validFuncs = ConsiderNonlinearFuncs ? NonlinearFuncs.CheckedItems.Select(val => val.Value) : new List<OpCode>();
            // TODO: add Hinge functions

            foreach (var variableName in inputVariables)
            {
                foreach (var exp in validExponents)
                {
                    var data = problemData.Dataset.GetDoubleValues(variableName).Select(x => Math.Pow(x, exp)).ToArray();
                    if (!ok(data)) continue;
                    var name = expToString(exp, variableName);
                    B1.Add(new BasisFunction(name, data, false));
                    foreach (OpCode _op in validFuncs)
                    {
                        var inner_data = data.Select(x => eval(_op, x)).ToArray();
                        if (!ok(inner_data)) continue;
                        var inner_name = OpCodeToString.GetByFirst(_op) + "(" + name + ")";
                        B1.Add(new BasisFunction(inner_name, inner_data, true));
                    }
                }
            }
            return B1;
        }

        private static List<BasisFunction> createMultivariateBases(List<BasisFunction> B1)
        {
            var B2 = new List<BasisFunction>();
            for (int i = 0; i < B1.Count(); i++)
            {
                var b_i = B1.ElementAt(i);
                for (int j = 0; j < i; j++)
                {
                    var b_j = B1.ElementAt(j);
                    if (b_j.IsOperator) continue; // disallow op() * op()
                    var b_inter = b_i * b_j;
                    B2.Add(b_inter);
                }
            }

            return B2;
        }

        // creates 1 denominator basis function for each corresponding basis function from basisFunctions
        private IEnumerable<BasisFunction> createDenominatorBases(IRegressionProblemData problemData, IEnumerable<BasisFunction> basisFunctions)
        {
            var y = new BasisFunction(problemData.TargetVariable, problemData.TargetVariableValues.ToArray(), false);
            var denomBasisFuncs = new List<BasisFunction>();
            foreach (var func in basisFunctions)
            {
                var denomFunc = y * func;
                denomBasisFuncs.Add(denomFunc);
            }
            return denomBasisFuncs;
        }

        private static string expToString(double exponent, string varname)
        {
            if (exponent.IsAlmost(1)) return varname;
            if (exponent.IsAlmost((double)1 / 2)) return OpCodeToString.GetByFirst(OpCode.SquareRoot) + "(" + varname + ")";
            if (exponent.IsAlmost((double)1 / 3)) return OpCodeToString.GetByFirst(OpCode.CubeRoot) + "(" + varname + ")";
            if (exponent.IsAlmost(2)) return OpCodeToString.GetByFirst(OpCode.Square) + "(" + varname + ")";
            if (exponent.IsAlmost(3)) return OpCodeToString.GetByFirst(OpCode.Cube) + "(" + varname + ")";
            else return varname + " ^ " + exponent;
        }

        public static double eval(OpCode op, double x)
        {
            switch (op)
            {
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

        public static IEnumerable<double> logspace(double start, double end, int count)
        {
            double d = (double)count, p = end / start;
            return Enumerable.Range(0, count).Select(i => start * Math.Pow(p, i / d));
        }

        // Uses ElasticNet Linear Regression to find best coefficients along the path of different lambdas
        // Results: Coefficient Graph, NMSE Graph, a set of ALL generated models, the final pareto-front of those models
        private void LearnModels(IRegressionProblemData problemData, List<BasisFunction> basisFunctions)
        {
            double[] lambda;
            double[] trainNMSE;
            double[] testNMSE;
            double[,] coeff;
            double[] intercept;
            int numNominatorBases = ConsiderDenominations ? basisFunctions.Count / 2 : basisFunctions.Count;
            //IDictionary<int, ISymbolicRegressionSolution> paretoFront = new Dictionary<int, ISymbolicRegressionSolution>();

            // wraps the list of basis functions in a dataset, so that it can be passed on to the ElNet function
            var X_b = createProblemData(problemData, basisFunctions);

            ElasticNetLinearRegression.RunElasticNetLinearRegression(X_b, Penalty, out lambda, out trainNMSE, out testNMSE, out coeff, out intercept, double.NegativeInfinity, double.PositiveInfinity, MaxNumBasisFuncs);

            var errorTable = NMSEGraph(coeff, lambda, trainNMSE, testNMSE);
            Results.Add(new Result(errorTable.Name, errorTable.Description, errorTable));
            var coeffTable = CoefficientGraph(coeff, lambda, X_b.AllowedInputVariables, X_b.Dataset);
            Results.Add(new Result(coeffTable.Name, coeffTable.Description, coeffTable));

            ItemCollection<IResult> models = new ItemCollection<IResult>();
            for (int modelIdx = 0; modelIdx < coeff.GetUpperBound(0); modelIdx++)
            {
                var coeffs = GetRow(coeff, modelIdx);
                var tree = Tree(basisFunctions, coeffs, intercept[modelIdx]);
                ISymbolicRegressionModel m = new SymbolicRegressionModel(Problem.ProblemData.TargetVariable, tree, new SymbolicDataAnalysisExpressionTreeInterpreter());
                //ISymbolicRegressionSolution s = new SymbolicRegressionSolution(m, Problem.ProblemData);
                bool withDenom(double[] coeffarr) => coeffarr.Take(coeffarr.Length / 2).ToArray().Any(val => !val.IsAlmost(0.0));
                models.Add(new Result("Model " + (modelIdx < 10 ? "0" + modelIdx : modelIdx.ToString()), m));
            }

            int complexity(double[] modelCoeffs) => modelCoeffs.Count(val => val != 0);
            var paretoFront = getParetoFront<IResult>(models.ToArray(), coeff, trainNMSE, complexity);

            Results.Add(new Result("Models", "The model path returned by the Elastic Net Regression (not only the pareto-optimal subset). ", models));
            Results.Add(new Result("Pareto Front", "The Pareto Front of the Models. ", new ItemCollection<IResult>(paretoFront)));
        }

        // returns all row indices of the models in coeffs that are supposed to be in the pareto front
        private static IEnumerable<T> getParetoFront<T>(T[] models, double[,] coeff, double[] error, Func<double[], int> complexity)
        {
            double[][] qualities = new double[coeff.GetLength(0)][];
            double[] asdf = new double[coeff.GetLength(0)];
            for (int i = 0; i < coeff.GetLength(0); i++)
            {
                qualities[i] = new double[2];
                qualities[i][0] = error[i];
                qualities[i][1] = complexity(GetRow(coeff, i));
            }
            var front = DominationCalculator<T>.CalculateBestParetoFront(models, qualities, new bool[2] { true, true });
            return front.Select(val => val.Item1);
        }

        private static IndexedDataTable<double> CoefficientGraph(double[,] coeff, double[] lambda, IEnumerable<string> allowedVars, IDataset ds, bool showOnlyRelevantBasisFuncs = true)
        {
            var coeffTable = new IndexedDataTable<double>("Coefficients", "The paths of standarized coefficient values over different lambda values");
            coeffTable.VisualProperties.YAxisMaximumAuto = false;
            coeffTable.VisualProperties.YAxisMinimumAuto = false;
            coeffTable.VisualProperties.XAxisMaximumAuto = false;
            coeffTable.VisualProperties.XAxisMinimumAuto = false;

            coeffTable.VisualProperties.XAxisLogScale = true;
            coeffTable.VisualProperties.XAxisTitle = "Lambda";
            coeffTable.VisualProperties.YAxisTitle = "Coefficients";
            coeffTable.VisualProperties.SecondYAxisTitle = "Number of variables";

            var nLambdas = lambda.Length;
            var nCoeff = coeff.GetLength(1);
            var dataRows = new IndexedDataRow<double>[nCoeff];

            var nNonZeroCoeffs = new int[nLambdas];
            var relevantBasisFuncs = new bool[nCoeff]; // relevant -> at least 1 non-zero value over the whole path

            var doubleVariables = allowedVars.Where(ds.VariableHasType<double>);
            var factorVariableNames = allowedVars.Where(ds.VariableHasType<string>);
            var factorVariablesAndValues = ds.GetFactorVariableValues(factorVariableNames, Enumerable.Range(0, ds.Rows)); //must consider all factor values (in train and test set)

            for (int i = 0; i < coeff.GetLength(0); i++)
            {
                for (int j = 0; j < coeff.GetLength(1); j++)
                {
                    if (!coeff[i, j].IsAlmost(0.0))
                    {
                        nNonZeroCoeffs[i]++;
                        relevantBasisFuncs[j] = true;
                    }
                }
            }

            var relevantCoeffs = new double[coeff.GetLength(0), relevantBasisFuncs.Count(val => val)];
            //var relevantDataRows = new IndexedDataRow<double>[]

            int k;
            for (int i = 0; i < coeff.GetLength(0); i++)
            {
                k = 0;
                for (int j = 0; j < coeff.GetLength(1); j++)
                {
                    if (!relevantBasisFuncs[j] || coeff[i, j].IsAlmost(0.0)) continue;
                    relevantCoeffs[i, k] = coeff[i, j];
                    k++;
                }
            }


            {
                int i = 0;
                foreach (var factorVariableAndValues in factorVariablesAndValues)
                {
                    foreach (var factorValue in factorVariableAndValues.Value)
                    {
                        double sigma = ds.GetStringValues(factorVariableAndValues.Key)
                          .Select(s => s == factorValue ? 1.0 : 0.0)
                          .StandardDeviation(); // calc std dev of binary indicator
                        var path = Enumerable.Range(0, nLambdas).Select(r => Tuple.Create(lambda[r], relevantCoeffs[r, i] * sigma)).ToArray();
                        dataRows[i] = new IndexedDataRow<double>(factorVariableAndValues.Key + "=" + factorValue, factorVariableAndValues.Key + "=" + factorValue, path);
                        i++;
                    }
                }

                foreach (var doubleVariable in doubleVariables)
                {
                    double sigma = ds.GetDoubleValues(doubleVariable).StandardDeviation();
                    var path = Enumerable.Range(0, nLambdas).Select(r => Tuple.Create(lambda[r], relevantCoeffs[r, i] * sigma)).ToArray();
                    var sum = path.Sum(val => Math.Abs(val.Item2));
                    dataRows[i] = new IndexedDataRow<double>(doubleVariable, doubleVariable, path);
                    i++;
                }

                var temp = dataRows.OrderByDescending(r => r.Values.Select(t => t.Item2).Sum(x => Math.Abs(x)));
                // add to coeffTable by total weight (larger area under the curve => more important);
                foreach (var r in temp)
                {
                    coeffTable.Rows.Add(r);
                }
            }

            if (lambda.Length > 2)
            {
                coeffTable.VisualProperties.XAxisMinimumFixedValue = Math.Pow(10, Math.Floor(Math.Log10(lambda.Last())));
                coeffTable.VisualProperties.XAxisMaximumFixedValue = Math.Pow(10, Math.Ceiling(Math.Log10(lambda.Skip(1).First())));
            }
            coeffTable.Rows.Add(new IndexedDataRow<double>("Number of variables", "The number of non-zero coefficients for each step in the path", lambda.Zip(nNonZeroCoeffs, (l, v) => Tuple.Create(l, (double)v))));
            coeffTable.Rows["Number of variables"].VisualProperties.ChartType = DataRowVisualProperties.DataRowChartType.Points;
            coeffTable.Rows["Number of variables"].VisualProperties.SecondYAxis = true;

            return coeffTable;
        }

        private static IndexedDataTable<double> NMSEGraph(double[,] coeff, double[] lambda, double[] trainNMSE, double[] testNMSE)
        {
            var errorTable = new IndexedDataTable<double>("NMSE", "Path of NMSE values over different lambda values");
            var numNonZeroCoeffs = new int[lambda.Length];
            errorTable.VisualProperties.YAxisMaximumAuto = false;
            errorTable.VisualProperties.YAxisMinimumAuto = false;
            errorTable.VisualProperties.XAxisMaximumAuto = false;
            errorTable.VisualProperties.XAxisMinimumAuto = false;

            for (int i = 0; i < coeff.GetLength(0); i++)
            {
                for (int j = 0; j < coeff.GetLength(1); j++)
                {
                    if (!coeff[i, j].IsAlmost(0.0))
                    {
                        numNonZeroCoeffs[i]++;
                    }
                }
            }

            errorTable.VisualProperties.YAxisMinimumFixedValue = 0;
            errorTable.VisualProperties.YAxisMaximumFixedValue = 1.0;
            errorTable.VisualProperties.XAxisLogScale = true;
            errorTable.VisualProperties.XAxisTitle = "Lambda";
            errorTable.VisualProperties.YAxisTitle = "Normalized mean of squared errors (NMSE)";
            errorTable.VisualProperties.SecondYAxisTitle = "Number of variables";
            errorTable.Rows.Add(new IndexedDataRow<double>("NMSE (train)", "Path of NMSE values over different lambda values", lambda.Zip(trainNMSE, (l, v) => Tuple.Create(l, v))));
            errorTable.Rows.Add(new IndexedDataRow<double>("NMSE (test)", "Path of NMSE values over different lambda values", lambda.Zip(testNMSE, (l, v) => Tuple.Create(l, v))));
            errorTable.Rows.Add(new IndexedDataRow<double>("Number of variables", "The number of non-zero coefficients for each step in the path", lambda.Zip(numNonZeroCoeffs, (l, v) => Tuple.Create(l, (double)v))));
            if (lambda.Length > 2)
            {
                errorTable.VisualProperties.XAxisMinimumFixedValue = Math.Pow(10, Math.Floor(Math.Log10(lambda.Last())));
                errorTable.VisualProperties.XAxisMaximumFixedValue = Math.Pow(10, Math.Ceiling(Math.Log10(lambda.Skip(1).First())));
            }
            errorTable.Rows["NMSE (train)"].VisualProperties.ChartType = DataRowVisualProperties.DataRowChartType.Points;
            errorTable.Rows["NMSE (test)"].VisualProperties.ChartType = DataRowVisualProperties.DataRowChartType.Points;
            errorTable.Rows["Number of variables"].VisualProperties.ChartType = DataRowVisualProperties.DataRowChartType.Points;
            errorTable.Rows["Number of variables"].VisualProperties.SecondYAxis = true;

            return errorTable;
        }

        private ISymbolicExpressionTree Tree(List<BasisFunction> basisFunctions, double[] coeffs, double offset)
        {
            Debug.Assert(basisFunctions.Count() == coeffs.Length);
            var numNumeratorFuncs = ConsiderDenominations ? basisFunctions.Count() / 2 : basisFunctions.Count();
            // the first half are the numerator basis funcs, the second half the denominator basis funcs
            var numeratorFuncs = basisFunctions.Take(numNumeratorFuncs);

            // returns true if there exists at least 1 coefficient value in the model that is part of the denominator 
            // (i.e. if there exists at least 1 non-zero value in the second half of the array)
            bool withDenom(double[] coeffarr) => coeffarr.Take(coeffarr.Length / 2).ToArray().Any(val => !val.IsAlmost(0.0));
            string model = "(" + offset.ToString();
            for (int i = 0; i < numNumeratorFuncs; i++)
            {
                var func = basisFunctions.ElementAt(i);
                // only generate nodes for relevant basis functions (those with non-zero coeffs)
                if (coeffs[i] != 0)
                    model += " + (" + coeffs[i] + ") * " + func.Var;
            }
            if (ConsiderDenominations && withDenom(coeffs))
            {
                model += ") / (1";
                for (int i = numNumeratorFuncs; i < basisFunctions.Count(); i++)
                {
                    var func = basisFunctions.ElementAt(i);
                    // only generate nodes for relevant basis functions (those with non-zero coeffs)
                    if (coeffs[i] != 0)
                        model += " + (" + coeffs[i] + ") * " + func.Var.Substring(4);
                }
            }
            model += ")";
            InfixExpressionParser p = new InfixExpressionParser();
            return p.Parse(model);
        }

        // wraps the list of basis functions into an IRegressionProblemData object
        private static IRegressionProblemData createProblemData(IRegressionProblemData problemData, List<BasisFunction> basisFunctions)
        {
            HashSet<string> variableNames = new HashSet<string>();
            List<IList> variableVals = new List<IList>();
            foreach (var basisFunc in basisFunctions)
            {
                variableNames.Add(basisFunc.Var);
                // basisFunctions already contains the calculated values of the corresponding basis function, so you can just take that value 
                variableVals.Add(new List<double>(basisFunc.Val));
            }
            var matrix = new ModifiableDataset(variableNames, variableVals);

            // add the unmodified target variable to the matrix
            matrix.AddVariable(problemData.TargetVariable, problemData.TargetVariableValues.ToList());
            IEnumerable<string> allowedInputVars = matrix.VariableNames.Where(x => !x.Equals(problemData.TargetVariable)).ToArray();
            IRegressionProblemData rpd = new RegressionProblemData(matrix, allowedInputVars, problemData.TargetVariable);
            rpd.TrainingPartition.Start = problemData.TrainingPartition.Start;
            rpd.TrainingPartition.End = problemData.TrainingPartition.End;
            rpd.TestPartition.Start = problemData.TestPartition.Start;
            rpd.TestPartition.End = problemData.TestPartition.End;
            return rpd;
        }

        private static bool ok(double[] data) => data.All(x => !double.IsNaN(x) && !double.IsInfinity(x));

        // helper function which returns a row of a 2D array
        private static T[] GetRow<T>(T[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }
    }
}