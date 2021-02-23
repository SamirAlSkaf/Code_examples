import numpy as np

## a)
##
#  forward propagation
##
def preactivationFunction(prev, W, b):
    return np.dot(W, prev) + b

def sigmoid(z):
    return 1/(1+np.exp(-z))

def forwardPropagationHidden(prev, W, b):
    z = preactivationFunction(prev, W, b)
    a = sigmoid(z)
    return a

def softmax(z):
    return np.exp(z) / np.exp(z).sum()

def forwardPropagationOutput(prev, W, b):
    z = preactivationFunction(prev, W, b)
    f = softmax(z)
    return f


## b)
##
#  delta rule (output layer)
##
def deltaOutput(I, f):
    return -(I - f)


## c)
##
#  delta rule (hidden layers)
##
def sigmoidDeriv(z):
    return sigmoid(z) * (1 - sigmoid(z))

def deltaHidden(W, delta, z):
    return np.transpose(W).dot(delta) * sigmoidDeriv(z)


## d)
##
# weight gradients
##
def gradientsWeight(delta, a):
    return delta.dot(np.transpose(a))


## d)
##
# bias gradients
##
def gradientsBias(delta, W, z):
    return deltaHidden(W, delta, z)

def gradientsBiasLast(delta, f):
    return deltaOutput(delta, f)


## f)
##
# example
##
X = [[0.1], [0.8]]
W_1 = [[0.2, 0.5], [0.3, 0.4]]
b_1 = [[0.1], [0.6]]
W_2 = [[0.1, 0.3], [0.7, 0.4], [0.01, 0.02]]
b_2 = [[0.2], [0.1], [0.4]]
I = [[0], [0], [1]]

# i)
a_1 = forwardPropagationHidden(X, W_1, b_1)
f = forwardPropagationOutput(a_1, W_2, b_2)
print("\nPrediction Hidden Layer:\n", a_1)
print("\nPrediction Output Layer:\n", f)

# ii)
delta_b_2 = gradientsBiasLast(I, f)
delta_W_2 = gradientsWeight(delta_b_2, a_1)

delta_b_1 = gradientsBias(delta_b_2, W_2, preactivationFunction(X, W_1, b_1))
delta_W_1 = gradientsWeight(delta_b_1, X)

print("\nDelta Bias 1:\n", delta_b_1)
print("\nDelta Weight 1:\n", delta_W_1)
print("\nDelta Bias 2:\n", delta_b_2)
print("\nDelta Weight 2:\n", delta_W_2)