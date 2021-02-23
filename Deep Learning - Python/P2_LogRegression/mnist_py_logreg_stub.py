# -*- coding: utf-8 -*-
"""
Created on Tue Apr 24 14:13:19 2018

@author: floko
"""

from tensorflow.examples.tutorials.mnist import input_data
import numpy as np    

"""
Hyperparams
"""
learning_rate = 0.0001
#number iterations for gradient descent (gdc)
gdc_steps = 4
    
"""
Data Import & Preprocessing
"""
# import dataset
mnist = input_data.read_data_sets("./mnist_data/", one_hot=False)

def data_preprocess(images, labels):

    # number of examples m  
    m = images.shape[0]
    
    # create vector of ones to concatenate to our data matrix (for intercept terms)
    ones = np.ones(shape=[m, 1])
    images = np.concatenate((ones, images), axis=1)
    
    # to retrieve the images and corresponding labels where the label is either 0 or 1, 
    # we define two logical vectors that can be used to subset our data_matrices
    logical_mask_0 = labels == 0
    logical_mask_1 = labels == 1
    
    images_zeros = images[logical_mask_0]
    labels_zeros = labels[logical_mask_0]
    images_ones = images[logical_mask_1]
    labels_ones = labels[logical_mask_1]
    
    X = np.concatenate((images_zeros, images_ones), axis=0)
    y = np.concatenate((labels_zeros, labels_ones), axis=0)
    
    # shuffle the data and corresponding labels in unison
    def _shuffle_in_unison(a, b):
        assert len(a) == len(b)
        p = np.random.permutation(len(a))
        return a[p], b[p]

    return _shuffle_in_unison(X,y)   



# get train and test datasets
dataset_train = mnist.train
dataset_test = mnist.test

X,y = data_preprocess(dataset_train.images, dataset_train.labels)


"""
Logistic Regression Model and Grdient Descent (GDC) optimization
"""
# number of features n
n = X.shape[1]
# we need to define our model parameters to be learned. we use W (weights) instead of theta this time.
mu, sigma = 0, 0.01 # mean and standard deviation
w = np.random.normal(mu, sigma, n)

# define sigmoid nonlinearity, Foliennr.
def sigmoid(z):
    return 1/(1+np.exp(-z))
#print("sigmoid(0.7) =", sigmoid(0.7))
#implement equation 35 in the slides of Lecture_2
#y_hat is h_theta(X) in equation 35
#y and y_hat are vectors in the dimension of training samples
def compute_cross_entropy_loss(y, y_hat):
    return -(y.dot(np.log(y_hat)) + (1 - y).dot(np.log(1 - y_hat)))

print("_______________________________")
print("Starting optimization on training set")

for step in range(0, gdc_steps):
    print("Performing step " + str(step) + " of gradient descent.")
    # compute X*theta (eq 1 in the slides), here we denote the params not theta but w
    z = X.dot(w)

    # apply the sigmoid nonlinearity
    y_hat = sigmoid(z)
    
    loss = compute_cross_entropy_loss(y, y_hat)
    print("Loss at step " + str(step) + ": " + str(loss))
    
    # compute the error term, i.e. the difference between labels and estimated labels y_hat, see equation 24 in the slides 
    error_term = y_hat - y
    
    # compute the gradient. as our data matrix X is currently layed out as X_j_i, we got to transpose it
    # see derived formula of the gradient calculation
    gradients = X.T.dot(sigmoid(z) - y)
    print(X.shape)
    print(error_term.shape)
    print(gradients.shape)
    
    # update w using the gdc update rule
    w = w-learning_rate*gradients
    
    # compute the predictions, i.e. whenever y hat is greater or equal to 0.5 we get class 1
    # and cast them to int values
    predictions = [1 if i >= 0.5 else 0 for i in y_hat]
    
    # compute mean accuracy by looking if the predictions match the labels y and applying the np.mean function to the result
    accuracy = np.mean(np.equal(predictions, y).astype(int))
    print("Accuracy at step " + str(step) + ": " + str(accuracy))
    
    
"""
Evaluation of test set with the trained logred model
"""
print("_______________________________")
print("Starting evaluation of test set")

X,y = data_preprocess(dataset_test.images, dataset_test.labels)
z = X.dot(w)
y_hat = sigmoid(z)
predictions = [1 if i >= 0.5 else 0 for i in y_hat]
accuracy = np.mean(np.equal(predictions, y).astype(int))
print("Accuracy of test set: " + str(accuracy))
