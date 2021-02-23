# get rid of warning: Your CPU supports instructions that this TensorFlow binary was not compiled to use: AVX2 FMA
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

import pprint
import numpy as np
import tensorflow as tf

# from create_dict import dict_deu, dict_deu_inv, dict_eng, dict_eng_inv

filespath = os.path.dirname(__file__) + './text2text/'
filenames = os.listdir(filespath)
filenames.sort()
filenames.reverse()

dict_eng = {0: "<s>", 1: "</s>"}
dict_deu = {0: "<s>", 1: "</s>"}
i_eng = 2
i_ger = 2

for filename in filenames:
    file = open(filespath + filename, "r", encoding="utf-8")
    lines = file.readlines()
    file.close()

    if (filename.startswith("iso")):
        line = lines[0].split("\t")[1].lower()[:-1]
        for element in line.split("|"):
            dict_eng[i_eng] = element
            i_eng += 1
        line = lines[1].split("\t")[1].lower()[:-1]
        for element in line.split("|"):
            dict_deu[i_ger] = element
            i_ger += 1
    elif (filename.startswith("con")):
        line = lines[2].split("\t")[1].lower()[:-1]
        for element in line.split(" "):
            element = element.replace("?", "")
            element = element.replace("!", "")
            element = element.replace(".", "")
            element = element.replace(",", "")
            if not element in dict_eng.values():
                dict_eng[i_eng] = element
                i_eng += 1
        line = lines[3].split("\t")[1].lower()[:-1]
        for element in line.split(" "):
            element = element.replace("?", "")
            element = element.replace("!", "")
            element = element.replace(".", "")
            element = element.replace(",", "")
            if not element in dict_deu.values():
                dict_deu[i_ger] = element
                i_ger += 1

dict_eng_inv = {v: k for k, v in dict_eng.items()}
dict_deu_inv = {v: k for k, v in dict_deu.items()}

# pp = pprint.PrettyPrinter(indent=4)
# pp.pprint(dict_deu)

########

def ids_to_words_eng(array):
    out = []
    for i in range(len(array)):
        out.append(dict_eng[array[i]])
    return out

def ids_to_words_deu(array):
    out = []
    for i in range(len(array)):
        out.append(dict_deu[array[i]])
    return out

def words_to_ids_eng(array):
    out = []
    for i in range(len(array)):
        out.append(dict_eng_inv[array[i]])
    return out

def words_to_ids_deu(array):
    out = []
    for i in range(len(array)):
        out.append(dict_deu_inv[array[i]])
    return out

########

batch_size_training = 1
batch_size = 1
embedding_size = 128
num_units = 128
num_epochs = 10000

encoder_inputs = tf.placeholder(tf.int32, shape=[batch_size_training, None], name="enc_in")
decoder_inputs = tf.placeholder(tf.int32, shape=[batch_size_training, None], name="dec_in")
decoder_outputs = tf.placeholder(tf.int32, shape=[batch_size_training, None], name="dec_out")

sequence_length = tf.placeholder(tf.int32, name="seq_len")
decoder_length = tf.placeholder(tf.int32, shape = [None], name="dec_len")

sos_id = dict_eng_inv["<s>"]
eos_id = dict_eng_inv["</s>"]

sentences_enc_input = [
    [words_to_ids_eng(['the', 'student', 'has', 'to', 'taste', 'the', 'beer', '</s>'])],
    [words_to_ids_eng(['do', 'your', 'parents', 'live', 'in', 'cologne', 'or', 'hamburg', 'now', '</s>'])],
    [words_to_ids_eng(['what', 'is', 'the', 'baker', 'called', '</s>'])],
    [words_to_ids_eng(['my', 'grandparents', 'get', 'a', 'monthly', 'pension', '</s>'])],
    [words_to_ids_eng(['my', 'daughter', 'was', 'in', 'very', 'bad', 'pain', 'when', 'she', 'gave', 'birth', 'to', 'her', 'baby', '</s>'])],
    [words_to_ids_eng(['i', 'will', 'buy', 'four', 'tickets', 'for', 'the', 'museum', 'for', 'you', '</s>'])]
]
sentences_decoder_input = [
    [words_to_ids_deu(['<s>', 'der', 'student', 'muss', 'das', 'bier', 'probieren'])],
    [words_to_ids_deu(['<s>', 'wie', 'heisst', 'der', 'bäcker'])],
    [words_to_ids_deu(['<s>', 'wohnen', 'deine', 'eltern', 'jetzt', 'in', 'köln', 'oder', 'in', 'hamburg'])],
    [words_to_ids_deu(['<s>', 'meine', 'grosseltern', 'bekommen', 'monatlich', 'eine', 'rente'])],
    [words_to_ids_deu(['<s>', 'meine', 'tochter', 'hatte', 'starke', 'schmerzen', 'bei', 'der', 'geburt', 'ihres', 'babys'])],
    [words_to_ids_deu(['<s>', 'ich', 'kaufe', 'dir', 'vier', 'eintrittskarten', 'für', 'das', 'museum'])]
]
sentences_decoder_output = [
    [words_to_ids_deu(['der', 'student', 'muss', 'das', 'bier', 'probieren','</s>'])],
    [words_to_ids_deu(['wie', 'heisst', 'der', 'bäcker','</s>'])],
    [words_to_ids_deu(['wohnen', 'deine', 'eltern', 'jetzt', 'in', 'köln', 'oder', 'in', 'hamburg','</s>'])],
    [words_to_ids_deu(['meine', 'grosseltern', 'bekommen', 'monatlich', 'eine', 'rente','</s>'])],
    [words_to_ids_deu(['meine', 'tochter', 'hatte', 'starke', 'schmerzen', 'bei', 'der', 'geburt', 'ihres', 'babys', '</s>'])],
    [words_to_ids_deu(['ich', 'kaufe', 'dir', 'vier', 'eintrittskarten', 'für', 'das', 'museum', '</s>'])]
]

#test_sentence = [words_to_ids_eng(['my', 'daughter', 'gave', 'birth', 'to', 'the', 'baker', '</s>'])] #translates to: ['hamburg', 'hamburg'] with 100.000 steps
test_sentence = sentences_enc_input[4]

## Embedding
embedding_encoder = tf.get_variable("embedding_encoder", [len(dict_eng), embedding_size])
encoder_emb_inp = tf.nn.embedding_lookup(embedding_encoder, encoder_inputs)

embedding_decoder = tf.get_variable("embedding_decoder", [len(dict_deu), embedding_size])
decoder_emb_inp = tf.nn.embedding_lookup(embedding_decoder, decoder_inputs)

## Encoder
encoder_cell = tf.nn.rnn_cell.BasicLSTMCell(num_units)
encoder_outputs, encoder_state = tf.nn.dynamic_rnn(encoder_cell, encoder_emb_inp,sequence_length=sequence_length, time_major=False, dtype=tf.float32)

## Decoder
decoder_cell = tf.nn.rnn_cell.BasicLSTMCell(num_units)
helper = tf.contrib.seq2seq.TrainingHelper(decoder_emb_inp, decoder_length, time_major=False)

projection_layer = tf.layers.Dense(len(dict_deu), use_bias=True)
decoder = tf.contrib.seq2seq.BasicDecoder(decoder_cell, helper, encoder_state, output_layer=projection_layer)

outputs, _, _ = tf.contrib.seq2seq.dynamic_decode(decoder)
logits = outputs.rnn_output

## Loss
target_weights = tf.transpose(tf.sequence_mask(tf.shape(decoder_outputs)[1]))

crossent = tf.nn.sparse_softmax_cross_entropy_with_logits(labels=decoder_outputs, logits=logits)
train_loss = (tf.reduce_sum(crossent * tf.to_float(target_weights)) / batch_size)

## Calculate and clip gradients
params = tf.trainable_variables()
gradients = tf.gradients(train_loss, params)
clipped_gradients, _ = tf.clip_by_global_norm(gradients, 5)

## Optimization
optimizer = tf.train.AdamOptimizer(0.0005)
update_step = optimizer.apply_gradients(zip(clipped_gradients, params))

print('start of training')
sess = tf.Session()
sess.run(tf.global_variables_initializer())
for j in range(num_epochs):
    if j % 100 == 0 and j > 0:
        print('epochs done:', j)
        print('----------------------------')
    crossent_episode = 0
    train_loss_episode = 0

    for i in range(len(sentences_enc_input)):
        crossent_train, train_loss_train, update_step_train = sess.run([crossent, train_loss, update_step], feed_dict={
            encoder_inputs:sentences_enc_input[i],
            decoder_inputs:sentences_decoder_input[i], 
            decoder_outputs:sentences_decoder_output[i],
            sequence_length:[np.shape(sentences_enc_input[i])[1]],
            decoder_length:[np.shape(sentences_decoder_input[i])[1]]})

        train_loss_episode = train_loss_episode + train_loss_train

    print(train_loss_episode / len(sentences_enc_input))

maximum_iterations = tf.round(tf.reduce_max(sequence_length) * 2)

# Helper
test_helper = tf.contrib.seq2seq.GreedyEmbeddingHelper(embedding_decoder, tf.fill([batch_size], sos_id), eos_id)

# Decoder
test_decoder = tf.contrib.seq2seq.BasicDecoder(decoder_cell, test_helper, encoder_state, output_layer=projection_layer)
# Dynamic decoding
test_outputs, _, __ = tf.contrib.seq2seq.dynamic_decode(test_decoder, maximum_iterations=maximum_iterations)
translations = test_outputs.sample_id

print('start of test')
translations_test = sess.run([translations], feed_dict={
            encoder_inputs:test_sentence, 
            sequence_length:[len(test_sentence[0])]})

print(ids_to_words_eng(test_sentence[0])[:-1])
print('translates to:')
print(ids_to_words_deu(translations_test[0][0]))
sess.close()