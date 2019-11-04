import numpy as np
# import ffx.api
import sys, os
import importlib
import core
import pandas

def main():
    
    # training_IN = np.array(pandas.read_csv(sys.argv[1]))
    # training_OUT = np.array(pandas.read_csv(sys.argv[2]))
    # test_IN = np.array(pandas.read_csv(sys.argv[3]))
    # test_OUT = np.array(pandas.read_csv(sys.argv[4]))
    
    training_IN = np.array(pandas.read_csv('Training_IN.csv'))
    training_OUT = np.array(pandas.read_csv('Training_OUT.csv'))
    test_IN = np.array(pandas.read_csv('Test_IN.csv'))
    test_OUT = np.array(pandas.read_csv('Test_OUT.csv'))
    

    # print("training_IN: \n", training_IN)
    # print("\ntraining_OUT: \n", training_OUT)
    # print("\ntest_IN: \n", test_IN)
    # print("\ntest_OUT: \n", test_OUT)
    train_X = np.array( [ (1.5,2,3), (4,5,6) ] ).T
    train_y = np.array( [1,2,3])
    test_X = np.array( [ (5.241,1.23, 3.125), (1.1,0.124,0.391) ] ).T
    test_y = np.array( [3.03,0.9113,1.823])
    print(train_X.shape[0])
    print(train_X.shape[1])
    print(train_X.shape)

    # core.MultiFFXModelFactory().build(train_X, train_y, test_X, test_y, ['column_a', 'column_b'], False, False)
    core.MultiFFXModelFactory().build(training_IN, training_OUT, test_IN, test_OUT, ['x1', 'x2', 'x3'], verbose = True, myDebug = True)

# models = ffx.run(train_X, train_y, test_X, test_y, ["predictor_a", "predictor_b"], True)
# for model in models:
#     yhat = model.simulate(test_X)
#     print(model)

# if __name__ == "__main__":
main()
