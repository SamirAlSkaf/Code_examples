from __future__ import absolute_import, division, print_function

# Import TensorFlow >= 1.10 and enable eager execution
import tensorflow as tf
tf.enable_eager_execution()

import os
import time
import numpy as np
import glob
import matplotlib.pyplot as plt
import PIL
import imageio
from IPython import display

from GAN_Generator import Generator, generator_loss
from GAN_Discriminator import Discriminator, discriminator_loss

(train_images, train_labels), (_, _) = tf.keras.datasets.cifar10.load_data()

class_id = 1
train_images_filtered = []
for i in range(len(train_images)):
    if train_labels[i][0] == class_id:
        train_images_filtered.append(train_images[i]) 

train_images_filtered = np.asarray(train_images_filtered)

def rgb2gray(rgb):
    return np.dot(rgb[...,:3], [0.299, 0.587, 0.114])

train_images_filtered_gray = []
for img in train_images_filtered:
    train_images_filtered_gray.append(rgb2gray(img))

train_images_filtered_gray = np.asarray(train_images_filtered_gray)

train_images_filtered_gray = train_images_filtered_gray.reshape(train_images_filtered_gray.shape[0], 32, 32, 1).astype('float32')
# We are normalizing the images to the range of [-1, 1]
train_images_filtered_gray = (train_images_filtered_gray - 127.5) / 127.5

BUFFER_SIZE = 60000
BATCH_SIZE = 256

train_dataset = tf.data.Dataset.from_tensor_slices(train_images_filtered_gray).shuffle(BUFFER_SIZE).batch(BATCH_SIZE)

generator = Generator()
discriminator = Discriminator()

# Defun gives 10 secs/epoch performance boost
generator.call = tf.contrib.eager.defun(generator.call)
discriminator.call = tf.contrib.eager.defun(discriminator.call)

discriminator_optimizer = tf.train.AdamOptimizer(1e-4)
generator_optimizer = tf.train.AdamOptimizer(1e-4)

checkpoint_dir = './training_checkpoints'
checkpoint_prefix = os.path.join(checkpoint_dir, "ckpt")
checkpoint = tf.contrib.eager.Checkpoint(generator_optimizer=generator_optimizer,
                                 discriminator_optimizer=discriminator_optimizer,
                                 generator=generator,
                                 discriminator=discriminator)

EPOCHS = 300
noise_dim = 100
num_examples_to_generate = 16

# keeping the random vector constant for generation (prediction) so
# it will be easier to see the improvement of the gan.
random_vector_for_generation = tf.random_normal([num_examples_to_generate,
                                                 noise_dim])

def generate_and_save_images(model, epoch, test_input):
    # make sure the training parameter is set to False because we
    # don't want to train the batchnorm layer when doing inference.
    predictions = model(test_input, training=False)
    
    for i in range(predictions.shape[0]):
        plt.subplot(4, 4, i+1)
        plt.imshow(predictions[i, :, :, 0] * 127.5 + 127.5, cmap="gray")
        plt.axis('off')
        
    plt.savefig('image_at_epoch_{:04d}.png'.format(epoch))
    plt.close('all')

def train(dataset, epochs, noise_dim):  
    for epoch in range(epochs):
        start = time.time()

        for images in dataset:
            # generating noise from a uniform distribution
            noise = tf.random_normal([BATCH_SIZE, noise_dim])
            
            with tf.GradientTape() as gen_tape, tf.GradientTape() as disc_tape:
                generated_images = generator(noise, training=True)
                
                real_output = discriminator(images, training=True)
                generated_output = discriminator(generated_images, training=True)
                
                gen_loss = generator_loss(generated_output)
                disc_loss = discriminator_loss(real_output, generated_output)
            
            gradients_of_generator = gen_tape.gradient(gen_loss, generator.variables)
            gradients_of_discriminator = disc_tape.gradient(disc_loss, discriminator.variables)
            
            generator_optimizer.apply_gradients(zip(gradients_of_generator, generator.variables))
            discriminator_optimizer.apply_gradients(zip(gradients_of_discriminator, discriminator.variables))
    
        if epoch % 1 == 0:
            display.clear_output(wait=True)
            generate_and_save_images(generator,
                                     epoch + 1,
                                     random_vector_for_generation)

        # saving (checkpoint) the model every 15 epochs
        if (epoch + 1) % 15 == 0:
            checkpoint.save(file_prefix = checkpoint_prefix)

        print ('Time taken for epoch {} is {} sec'.format(epoch + 1, time.time()-start))

    # generating after the final epoch
    display.clear_output(wait=True)
    generate_and_save_images(generator,
                             epochs,
                             random_vector_for_generation)

train(train_dataset, EPOCHS, noise_dim)

# restoring the latest checkpoint in checkpoint_dir
checkpoint.restore(tf.train.latest_checkpoint(checkpoint_dir))

with imageio.get_writer('dcgan.gif', mode='I') as writer:
    filenames = glob.glob('image*.png')
    filenames = sorted(filenames)
    last = -1
    for i,filename in enumerate(filenames):
        frame = 2*(i**0.5)
        if round(frame) > round(last):
            last = frame
        else:
            continue
        image = imageio.imread(filename)
        writer.append_data(image)
    image = imageio.imread(filename)
    writer.append_data(image)