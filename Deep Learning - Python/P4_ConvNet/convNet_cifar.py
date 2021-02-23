# -*- coding: utf-8 -*-
"""
Created on Thu Mar  1 11:00:33 2018

@author: LaRu
"""
import tensorflow as tf

from tensorflow.python.keras import datasets
import numpy as np
from model import *


#discomment after restart
(x_train, y_train), (x_test, y_test) = datasets.cifar10.load_data()

print(x_train.shape)#(50000, 32, 32, 3)
print(y_train.shape, y_train.dtype)
x_train = x_train.astype(np.float32)
y_train = y_train.astype(np.int32)
#print('type: ', x_train.dtype)
x_test = x_test.astype(np.float32)
y_test = y_test.astype(np.int32)
mode = 1 # 1=eval, 0 = train,
epochs = 20
batch_size = 20
lr = 0.0005
keep_prob = 0.5 #keep
checkpoints_path = './checkpoints'
num_iter = np.int(x_train.shape[0]/batch_size)*epochs
print('iter: ', num_iter)
#imgs_placeholder = tf.placeholder(x_train.dtype, x_train.shape)
#labels_placeholder = tf.placeholder(y_train.dtype, y_train.shape)

tf.reset_default_graph()
if (mode == 0):
    y_train = np.reshape(y_train,[50000])
    dataset = tf.data.Dataset.from_tensor_slices((x_train,y_train))
    batched_dataset = dataset.batch(batch_size)
    #
    iterator = batched_dataset.make_initializable_iterator()
    next_element, next_label = iterator.get_next()
    print('img_tensor: ', next_element)
    print('label: ', next_label)
     # conv, bias, relu
    #tf.reset_default_graph()
    optimizer, global_step, loss, accuracy, summary_op = build_model(next_element, next_label, True, keep_prob, lr, batch_size)
    saver = tf.train.Saver()

    train_sess = tf.Session()
    writer = tf.summary.FileWriter('./graphs/convnet', tf.get_default_graph())#train_sess.graph)
    
   
    train_sess.run(tf.global_variables_initializer())
    train_sess.run(tf.local_variables_initializer())

    path = tf.train.latest_checkpoint(checkpoints_path)
        
    if path:
        print('restore path: ', path)
        saver.restore(train_sess,path)
        print('global_step', global_step.eval(train_sess))
    else:
        print('could not restore')
    total_acc = 0
    for i in range(epochs):
        train_sess.run(iterator.initializer)
        try:
            while True:
                loss_, acc_m, summary_, _, global_step_ = train_sess.run([loss, accuracy, summary_op, optimizer, global_step])#,feed_dict={imgs_placeholder: x_train, labels_placeholder: y_train}) 
                if (global_step_ % 200 == 0):
                    print ("Loss at global step", global_step_, ":", loss_, acc_m)
                writer.add_summary(summary_)
                total_acc += acc_m
        except tf.errors.OutOfRangeError:
            print('Epoch finished')
    print('total_acc_train: ', total_acc/num_iter)
    saver.save(train_sess, checkpoints_path+'/ckpts', global_step = global_step)    
    writer.close()
    train_sess.close()
if (mode == 1):
    y_test = np.reshape(y_test, [10000])
    dataset = tf.data.Dataset.from_tensor_slices((x_test,y_test))
    print('test shape: ', x_test.shape, y_test)
    dataset = dataset.batch(batch_size)
    iterator = dataset.make_one_shot_iterator()

    next_element, next_label = iterator.get_next()
     # conv, bias, relu
    #tf.reset_default_graph()
    print('test dataset: ', next_element, next_label)
    global_step, accuracy_mean, summary_op = build_model(next_element, next_label, False, keep_prob, lr, batch_size)
    saver = tf.train.Saver()
    eval_sess = tf.Session()
    print(eval_sess.run([tf.shape(next_element), tf.shape(next_label)]))
    eval_sess.run(tf.global_variables_initializer())
    writer = tf.summary.FileWriter('./graphs/convnettest', tf.get_default_graph())#train_sess.graph)
    path = tf.train.latest_checkpoint(checkpoints_path)
        
    if path:
        print('restore path: ', path)
        saver.restore(eval_sess,path)
        #print('global_step', global_step.eval(eval_sess))
    else:
        print('could not restore')
    total_acc = 0
    i = 0
    try:
        while True:
            accmean_, summary_, gs_ = eval_sess.run([accuracy_mean, summary_op, global_step])#,feed_dict={imgs_placeholder: x_train, labels_placeholder: y_train}) 
            if (i%100==0):
                print(accmean_)
            writer.add_summary(summary_)
            total_acc += accmean_
            i += 1
    except tf.errors.OutOfRangeError:
        pass
    print('total_acc_test: ', total_acc/i)
    writer.close()
    eval_sess.close()

'''
    10  epochs  0.0005 lr               -   training 0.666    test 0.622
    +10 epochs  0.0005 lr               -   training 0.852    test 0.594
    +20 epochs  0.0005 lr & 0.5 do      -   training 0.834    test 0.605
'''