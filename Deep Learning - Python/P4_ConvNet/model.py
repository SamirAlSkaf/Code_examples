# -*- coding: utf-8 -*-
"""
Created on Fri Mar  2 20:19:06 2018

@author: LaRu
"""
import tensorflow as tf

def build_model(input_data, label, train_mode, keep_prob, learning_rate, batch_size):
    input_layer = input_data
    conv_1_layer = tf.layers.conv2d(input_layer, 32, 3, activation=tf.nn.relu, padding="same", kernel_initializer=tf.contrib.layers.xavier_initializer(), bias_initializer=tf.constant_initializer(0.0001))
    conv_2_layer = tf.layers.conv2d(conv_1_layer, 32, 3, activation=tf.nn.relu, padding="same", kernel_initializer=tf.contrib.layers.xavier_initializer(), bias_initializer=tf.constant_initializer(0.0001))
    pool_1_layer = tf.layers.max_pooling2d(conv_2_layer, 2, strides=2)
    conv_3_layer = tf.layers.conv2d(pool_1_layer, 64, 3, activation=tf.nn.relu, padding="same", kernel_initializer=tf.contrib.layers.xavier_initializer(), bias_initializer=tf.constant_initializer(0.0001))
    pool_2_layer = tf.layers.max_pooling2d(conv_3_layer, 2, strides=2)
    flatten_1_layer = tf.layers.flatten(pool_2_layer)
    fc_1_layer = tf.layers.dense(flatten_1_layer, 512)
    dropout_1_layer = tf.layers.dropout(fc_1_layer, rate=1-keep_prob, training=train_mode)
    fc_2_layer = tf.layers.dense(dropout_1_layer, 10)
    output_layer = fc_2_layer

    loss = tf.losses.sparse_softmax_cross_entropy(label, output_layer)
    prediction = tf.argmax(output_layer, axis=1, output_type=tf.int32)
    correct_prediction = tf.equal(label, prediction)
    acc = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))

    """
    logits = tf.reshape(output_layer, [-1, output_layer.shape[1] * output_layer.shape[2] * output_layer.shape[3]])

    predictions = {
        'classes': tf.argmax(input=logits, axis=1),
        'probabilities': tf.nn.softmax(logits, name='softmax_tensor')
    }

    print('logits shape:', logits.shape)
    loss = tf.reduce_mean(tf.nn.sparse_softmax_cross_entropy_with_logits(labels=label, logits=logits))
    acc, acc_op = tf.metrics.accuracy(predictions=predictions['classes'], labels=tf.argmax(label, 1))
    """

    global_step = tf.Variable(0, dtype=tf.int32, trainable=False, name='global_step')
    if train_mode:
        # Optimization
        optimizer = tf.train.AdamOptimizer(learning_rate).minimize(loss, global_step)    
        #optimizer = tf.train.RMSPropOptimizer(lr,decay=1e-6).minimize(loss, global_step)
        tf.summary.scalar('loss', loss)
    tf.summary.scalar('accuracy', acc)
    tf.summary.histogram('histogram loss', loss)
    summary_op = tf.summary.merge_all()
    if train_mode:
        return optimizer, global_step, loss, acc, summary_op
    else:
        return global_step, acc, summary_op