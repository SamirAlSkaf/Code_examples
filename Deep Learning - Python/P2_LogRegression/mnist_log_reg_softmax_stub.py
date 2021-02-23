# -*- coding: utf-8 -*-
"""
Created on Fri Feb 16 16:22:23 2018

@author: LaRu
"""

from tensorflow.examples.tutorials.mnist import input_data
import numpy as np
import tensorflow as tf

tf.reset_default_graph()
mnist = input_data.read_data_sets("MNIST_data/", one_hot=False)
print(mnist.train.images.shape)
print(mnist.train.labels.shape)
mask_0 = mnist.train.labels == 0
mask_1 = mnist.train.labels == 1
zeros = tf.boolean_mask(mnist.train.images, mask_0)
labels_0 = tf.boolean_mask(mnist.train.labels, mask_0)
ones = tf.boolean_mask(mnist.train.images, mask_1)
labels_1 = tf.boolean_mask(mnist.train.labels, mask_1)
all0_1 = tf.concat([zeros, ones], 0)
all0_1_labels = tf.cast(tf.concat([labels_0, labels_1], 0), tf.int32)

#import Data with tf.data.Dataset.from_tensor_slices
dataset = tf.data.Dataset.from_tensor_slices((all0_1,all0_1_labels))
# Shuffle, repeat, and batch the examples.
dataset = dataset.shuffle(12000).repeat(10).batch(10)
iterator = dataset.make_initializable_iterator()
img_input, labels = iterator.get_next()
print(img_input, labels)

#define the parameters variable (we use W instead of thetas) of shape 784x2 of type tf.float32
#use initializer = tf.contrib.layers.xavier_initializer()
W = tf.get_variable("W", shape=[784, 2], initializer=tf.contrib.layers.xavier_initializer())
#define bias variable of shape 2, type float, initializer = tf.constant_initializer(0.0001)
b = tf.get_variable("b", shape=[2], initializer=tf.constant_initializer(0.0001))

#compute Input * Parameters + bias (see slide 22 (bias term omitted there))
#==============================================================================
# Logit numerical example (not in python syntax) but pseudocode
# One sample with 5 features, W (theta) parameters 
# [1 2 3 4 5]*[0.1 0.5   + [0.01 0.05] = [5.41 ]
#              0.3 0.2
#              0.2 0.8
#              0.9 0.6
#              0.1 0.4]
#==============================================================================
logits = tf.add(tf.tensordot(img_input, W, axes=1), b)
# checkout tf.losses.sparse_softmax_cross_entropy
cross_entropy = tf.losses.sparse_softmax_cross_entropy(labels, logits)

#compute the prediction by looking if the index of the maximum in the logits equals the label
#checkout tf.argmax, tf.equal and you also need to convert the index from float to tf.int32
correct_prediction = tf.cast(tf.equal(tf.argmax(logits, axis=1, output_type=tf.int32), labels), tf.float32)
#compute the accuracy by computing the mean of the correct_prediction
#checkout tf.reduce_mean
accuracy = tf.reduce_mean(correct_prediction)
#use the minimize function of the in-built Gradient descent optimizer with a learning rate that you need to find out what would work
opt = tf.train.GradientDescentOptimizer(0.1).minimize(cross_entropy)
# 0.0001 - 0.495
# 0.001  - 0.784
# 0.01   - 0.980, 0.983
# 0.05   - 0.998
# 0.1    - 0.997, 0.998
# 0.2    - 0.996, 0.998
# 0.3    - 0.995, 0.996
# 0.5    - 0.997
# 1      - 0.997

############test dataset####################################################
mask_0_t = mnist.test.labels == 0
mask_1_t = mnist.test.labels == 1
zeros_t = tf.boolean_mask(mnist.test.images, mask_0_t)
labels_0_t = tf.boolean_mask(mnist.test.labels, mask_0_t)
ones_t = tf.boolean_mask(mnist.test.images, mask_1_t)
labels_1_t = tf.boolean_mask(mnist.test.labels, mask_1_t)
all0_1_t = tf.concat([zeros_t, ones_t], 0)
all0_1_labels_t = tf.cast(tf.concat([labels_0_t, labels_1_t], 0), tf.int32)

dataset_t = tf.data.Dataset.from_tensor_slices((all0_1_t,all0_1_labels_t))
#one shot iterator, iterates only once through the data
iterator_t = dataset_t.make_one_shot_iterator()
img_input_t, labels_t = iterator_t.get_next()
#need to put input in format(batchsize, dimension), alternatively one can use dataset_t = dataset_t.batch(1) BEFORE one shot iterator
img_input_t = tf.expand_dims(img_input_t,0)
print(img_input_t, labels_t)
logits_t = tf.add(tf.tensordot(img_input_t, W, axes=1), b)
correct_prediction_t = tf.cast(tf.equal(tf.argmax(logits_t, axis=1, output_type=tf.int32), labels_t), tf.float32)
accuracy_t = tf.reduce_mean(correct_prediction_t)

j = 0
total_acc = 0.0
with tf.Session() as sess:
    sess.run(tf.global_variables_initializer())
    #iterator should be initialized
    sess.run(iterator.initializer)
    #run training
    for i in range(10):
        ce, acc, _ = sess.run([cross_entropy, accuracy, opt])
        print(ce, acc)
    #run test, we used a one shot iterator therefor no initialization is needed
    try:
        while True:
                cp, acc = sess.run([correct_prediction_t, accuracy_t]) 
                total_acc += acc
                j += 1
    except tf.errors.OutOfRangeError:
        pass
    all_ = sess.run(all0_1)
    print(all_.shape)
print(total_acc/j)
sess.close()   
