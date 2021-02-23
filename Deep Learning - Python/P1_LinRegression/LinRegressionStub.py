import numpy as numpy                               #Klassische Mathematik-Funktionen wie Matrizen oder Lineare Algebra Methoden
import sklearn                                      #Funktionen für die Extraktion von Informationen und DataSets
from sklearn.datasets import load_boston            #Datensatz von Hauspreisen
from sklearn.model_selection import train_test_split#Funktion Für den Split des Datensatzes zu Train- & Testdaten
import matplotlib.pyplot as matplotlib              #Funktionen für die Darstellung von Informationen
import seaborn as seaborn                           #Funktionen für die Darstellung von Informationen(basierend auf matplotlib)
from tensorflow.python.framework import ops as tf   #ML-Source tensorflow
from pandas import *

#Vorbereitungen für Visualisierung
seaborn.set_style("whitegrid")                      #Darstellungsform für Seaborn-Darstellung
seaborn.set_context("poster")                       #Details zur Darstellung(labels,linien & elemente)

#Ausgaben um Datensatz auzuchecken
bostonGesamterDatensatz = load_boston()                                           #SciKitLearn-Datensatz zuweisen
print("bostonGesamterDatensatz-Keys: ",bostonGesamterDatensatz.keys())            #keys ausgeben()
print("Feature-Names(13stk.): ",bostonGesamterDatensatz.feature_names)            #features ausgeben
print("Basic-Datashape: ",bostonGesamterDatensatz.data.shape)                     #Form der Daten

#Erste Variablen-Zuweisungen für spätere Verwendung
Datensatz = bostonGesamterDatensatz.data                                                 #Zuweisung von der Wohnungsdaten mit Features          
print("Datensatz, Alle wohnungen mit 13 Features: ")                          #Datensatz ausgeben. Wohnung jeweils mit allen 13 features
print(DataFrame(Datensatz))
ErwarteteErgebnisse = bostonGesamterDatensatz.target    #Target sind die tatsächlichen Mietpreise der Wohnungen
print("Erwartete Ergebnisse: ")     #Ausgabe der Mieten
print(DataFrame(ErwarteteErgebnisse))


DatensatzDurchschnittCol = numpy.mean(Datensatz,axis=0)             #Durschschnitt entlang Colums(axis=0)/Features
print("DatensatzDurchschnittColums")
print(DataFrame(DatensatzDurchschnittCol))


sigmaProCol = numpy.std(Datensatz,axis=0)                           #standard deviation/abweichung für jede Colum(Feature entlang)
print("sigmaProCol: ")     
print(DataFrame(sigmaProCol))


Datensatz = (Datensatz - DatensatzDurchschnittCol)/sigmaProCol      #? Normieren?
print("Datensatz-Shape nach sigma-rechnung: ",Datensatz.shape)


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

#NORMAL EQUATION
#optimum solution for this model
#opt = 
#print(opt)
#######################end of a) b) c)


######start d)###################################
tf.reset_default_graph()
input()

#the inumpyut X, y is taken into account in the computational graph via a placeholder
#define placeholder for computational graph of shape [400,14] and type tf.float32
X_Train = tf.placeholder(tf.float32, [400,14])
print("X_Train: ",X_Train)
#define placeholder for target y with shape [400]
Y_Train = tf.placeholder(tf.float,[400])
print("Y_Train: ",Y_Train)



#the parameters that need to be learned are defined by means of variables
#define theta variable 
##theta_hat= tf.Variable(to do)
##ErwarteteErgebnissehat = tf.tensordot(X_Train,theta_hat,1)

##error_vec = 
#check tf.subtract, tf.reduce_sum and tf.square
##loss = 
# define optimizer as gradient descent with learning rate of 0.0001 to minimize loss
#look for GradientDescentOptimizer in the tensorflow API
optimizer = tf.train.GradientDescentOptimizer(0.5) 

#discomment when ready
#with tf.Session() as sess:
#    sess.run(tf.global_variables_initializer())
#    for epoch in range(1000):
#        l, e_v, ErwarteteErgebnissepred, t_hat, _ = sess.run([loss,error_vec,ErwarteteErgebnissehat,theta_hat, optimizer], feed_dict={X:Datensatztrain, y:ErwarteteErgebnissetrain})
#        
#        #print('epoch, y pred, loss, diff_vec: ', epoch, ErwarteteErgebnissepred.shape, l, e_v.shape)
#print('theta_hat iterative: ', t_hat)      
#print('theta_hat normal equations: ', opt) 
     
#fill test target and predicted target
#matplotlib.scatter(....TO DO)
#matplotlib.xlabel("Prices: $ErwarteteErgebnissei$")
#matplotlib.ylabel("Predicted prices: $\hat{y}_i$")
#matplotlib.title("Prices vs Predicted prices: $ErwarteteErgebnissei$ vs $\hat{y}_i$")

