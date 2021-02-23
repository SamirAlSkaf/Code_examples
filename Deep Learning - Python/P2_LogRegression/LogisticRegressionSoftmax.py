from tensorflow.examples.tutorials.mnist import input_data
import numpy as np 
import tensorflow as tf
from pandas import *
old_v = tf.compat.v1.logging.get_verbosity()                    #Hide Update Warnings
tf.compat.v1.logging.set_verbosity(tf.compat.v1.logging.ERROR)  

#Data-PreProcessing
#region
tf.compat.v1.reset_default_graph()
mnist = input_data.read_data_sets("MNIST_data/", one_hot=False)
print("######################################################################")
print("TRAIN-BILDER SHAPE: ",mnist.train.images.shape) #Colums & Rows reduce
print("TRAIN-LABEL SHAPE : ",mnist.train.labels.shape)
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
img_input, labels = iterator.get_next() #Daten Zuweisung
print("IMG_INPUT: ",img_input)
print("LABELS   : " ,labels)
print("######################################################################")
#endregion
###############################################################################




#define the parameters variable (we use W instead of thetas) of shape 784x2 of type tf.float32
#use initializer = tf.contrib.layers.xavier_initializer()
W = tf.compat.v1.get_variable("W", shape=[784, 2], initializer=tf.contrib.layers.xavier_initializer())
#define bias variable of shape 2, type float, initializer = tf.constant_initializer(0.0001)
b = tf.compat.v1.get_variable("b", shape=[2], initializer=tf.constant_initializer(0.0001))

# LOGITS = Input * Parameters + bias  (TensorflowCompuGraph); Folie 20(bias hinzugefügt)
# [1 2 3 4 5]*[0.1 0.5   + [0.01 0.05] = [5.41 ]
#              0.3 0.2
#              0.2 0.8
#              0.9 0.6
#              0.1 0.4]

#compute Input * Parameters + bias 
logits = tf.add(tf.tensordot(img_input, W, axes=1), b)

# checkout tf.losses.sparse_softmax_cross_entropy
cross_entropy = tf.compat.v1.losses.sparse_softmax_cross_entropy(labels, logits) #(labels,logits) aus Folie 24 und Reihenfolge nach Docs

#compute the prediction by looking if the index of the maximum in the logits equals the label
#checkout tf.argmax, tf.equal and you also need to convert the index from float to tf.int32
#-> 1.index of maximun in logits. 2.Vergleich von Maximum und label 3.convert index to float
correct_prediction = tf.cast(tf.equal(tf.argmax(logits, axis=1, output_type=tf.int32), labels),tf.int32)
print("CORRECT_PREDICTION: ",correct_prediction)

#compute the accuracy by computing the mean of the correct_prediction#checkout tf.reduce_mean
accuracy = tf.reduce_mean(correct_prediction)
print("ACCURACY: ",accuracy)

#use the minimize function -> .minimize(cross_entropy) //(losses)
##of the in-built Gradient descent optimizer -> tf.train.GradientDescentOptimizer()
#with a learning rate that you need to find out what would work-> Testing?
opt = tf.compat.v1.train.GradientDescentOptimizer(0.25).minimize(cross_entropy) 


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
iterator_t = tf.compat.v1.data.make_one_shot_iterator(dataset_t) #Änderungen: dataset nun als Parameter,nicht als Objekt
img_input_t, labels_t = iterator_t.get_next()
#need to put input in format(batchsize, dimension), alternatively one can use dataset_t = dataset_t.batch(1) BEFORE one shot iterator
img_input_t = tf.expand_dims(img_input_t,0)
print("IMG_INPUT_T: ",img_input_t)
print("LABELS-T: ",labels_t)




#Die nächsten drei Zuweisungen sind wie oben, allerdings mit Daten/Variablen aus der Testumgebung
logits_t = tf.add(tf.tensordot(img_input_t, W, axes=1), b)  correct_prediction_t = tf.cast(tf.equal(tf.argmax(logits_t, axis=1, output_type=tf.int32), labels_t), tf.float32)
accuracy_t = tf.reduce_mean(correct_prediction_t)




print("--------------SESSION-STARTED-------------")
j = 0
total_acc = 0.0
with tf.compat.v1.Session() as sess:
    sess.run(tf.compat.v1.global_variables_initializer())
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
 
tf.logging.set_verbosity(old_v) #Warnungen unterdrücken.

