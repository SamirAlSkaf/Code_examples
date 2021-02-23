import numpy as np
import sklearn

import seaborn as sns
from tensorflow.python.framework import ops as tf
sns.set_style("whitegrid")
sns.set_context("poster")


from sklearn.datasets import load_boston
boston = load_boston()
print(boston.keys())
print(boston.data.shape)

#print(boston.feature_names)
#print(boston.DESCR)
X_ = boston.data
y_ = boston.target
#normalize features, features are in cols so take mean over each col-> axis = 0
mu = np.mean(X_,axis=0)#take mean over each col
sigma = np.std(X_,axis=0)
X_ = (X_ - mu)/sigma


train_test_split                                                    #Pandas Funktion zur Zuweisung von Trainings- und Testdaten
DatensatzTest,DatensatzTrain,  ErwartetesErgebnisTest, ErwartetesErgebnisTrain = train_test_split(Datensatz, ErwarteteErgebnisse, test_size = 400, random_state = 0)

#Extra Colum mit Nullen für jeden Datensatz
DatensatzTrain = numpy.insert(DatensatzTrain,0,values=0,axis=1)         #Colum/Array an Nullen hinzufügen(an nullster Stelle)
print("DATENSATZ-TRAINING-SHAPE: ",DatensatzTrain.shape)
DatensatzTest = numpy.insert(DatensatzTest,0,values=0,axis=1)     
print("DATENSATZ-TEST-SHAPE:",DatensatzTest.shape)

print("ERGEBNISSE-TRAININGS-SHAPE: ",ErwartetesErgebnisTrain.shape)
laenge = len(ErwartetesErgebnisTrain)                                     #längenabfrage für Array
extraColumOfZeros = numpy.zeros((laenge,), dtype=int)                     #Array an Nullen erstellt(Form 506zeilen,1Spalte)
print("extraColumOfZeros.Shape: ",extraColumOfZeros.shape)                
ErwartetesErgebnisTrain = numpy.stack((extraColumOfZeros,ErwartetesErgebnisTrain),axis=1) #Colum aus Nullen an Ergebnis Array angehängt
print("ERGEBNISSE-TRAININGS-SHAPE: NACH EXTRA COLUM:  ",ErwartetesErgebnisTrain.shape)
print(DataFrame(ErwartetesErgebnisTrain))

print("ERGEBNISSE-TEST-SHAPE:", ErwartetesErgebnisTest.shape)
laenge = len(ErwartetesErgebnisTest)
extraColumOfZeros = numpy.zeros((laenge,), dtype=int)                     #Array an Nullen erstellt(Form 506zeilen,1Spalte)
print("extraColumOfZeros.Shape: ",extraColumOfZeros.shape)   
ErwartetesErgebnisTest = numpy.stack((extraColumOfZeros,ErwartetesErgebnisTest),axis=1) #Colum aus Nullen an Ergebnis Array angehängtprint("ERGEBNISSE-TEST-SHAPE NACH EXTRA COLUM ",ErwartetesErgebnisTest.shape)
print(DataFrame(ErwartetesErgebnisTest))


#optimum solution for this model
#opt = 
#print(opt)
#######################end of a) b) c)


######start d)###################################
tf.reset_default_graph()
#define placeholder for computational graph as in the other exercise
X_Train = tf.placeholder(tf.float32, [400,14])
print("X_Train: ",X_Train)
#define placeholder for target y with shape [400]
Y_Train = tf.placeholder(tf.float,[400])
print("Y_Train: ",Y_Train)



#define theta hat variable 
#theta_hat= 
#calculate y_hat
#y_hat = tf.tensordot(TO DO)
#calculate error vector e (y - y_hat)
#error_vec = 
#calculate gradient vector
#gradient_vec = tf.tensordot(TO DO)

#update theta_hat
#alpha = 0.0001
#theta_hat = theta_hat.assign(theta_hat - alpha*gradient_vec)#with tf.Session() as sess:
#    sess.run(tf.global_variables_initializer())
#    for epoch in range(1000):
#        
#        e_vec, grad_vec, t_hat = sess.run([error_vec, gradient_vec, theta_hat], feed_dict={X:X_train, y:y_train})
#        
#        #print('iteration, error vec, grad vec, theta vec: ', epoch, e_vec, grad_vec, t_hat)
#print('theta_hat iterative: ', t_hat)          
#print('theta_hat normal equations: ', opt) 
     







