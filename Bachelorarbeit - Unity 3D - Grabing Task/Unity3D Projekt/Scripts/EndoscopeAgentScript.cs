using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class EndoscopeAgentScript : Agent
{

    #region Objekte
    public GameObject OesoUndMagen; // Das sind das Modell des Körpers
    public GameObject Coin;         // Das ist der Fremdkörper
    Rigidbody AgentsRigidbody;      // Das ist das Basisobjekt/ der Agent. Wir brauchen im Code allerdings nur den Physik-Rigidbody.
    public GameObject[] allPivots;  // Das ist ein Array an allen Drehpunkten (Pivots) des Gelenks
    public GameObject CapsuleZangenTraeger;     // Das ist der Träger der Instrumente (Zange, etc.)
    public GameObject aeusesterPunkt;           // Äußester Punkt des Zangenträgers (In diesem Fall die Lichtquelle)

    public GameObject Zahn1;            // Zangenzahn 1 von 2
    public GameObject Zahn2;            // Zangenzahn 2 von 2
    public GameObject ZahnPivot1;       // Gelenkpunkte des Zagenzahns 1 von 2
    public GameObject ZahnPivot2;       // Gelenkpunkte des Zagenzahns 2 von 2
    #endregion

    #region Variablen für Vor-/ Zurückbewegung und Rotation des Basisobjekts / des Agents
    Vector3 beginningPosition;                  //Startposition des Agents
    Quaternion agentStartRotation;              //Startrotation des Agents
    public static float movementSpeed = 200;    //Geschwindigkeit des Vor- und Zurückbewegung des Agents
    public float rotationSpeed = 5.0f;          //Geschwindigkeit der Rotation des Agents
    #endregion

    #region Variablen für das Biegen des Gelenks
    Quaternion pivotRotation;                   //Der Winkel der Beugung 
    public float BendingAngleSpeed = 1.0f;      //Die Geschwindigkeit der Beugung / Rotation der Gelenkpunkte
    #endregion

    #region Variablen für die Rotation des Zangenträgers/ Instrumentträgers
    Quaternion CapsuleZangenTraegerStartRotation;   // Der Winkel der Rotation des Zangenträgers
    public float ZangenRotationSpeed = 1;           // Die Geschwindigkeit des Rotation des Zangenträgers
    #endregion

    #region Variablen für das Zugreifen mit der Zange
    Vector3 Zahn1LocalStartPosition;        // Die Startposition des Zangenzahns 1 von 2
    Vector3 Zahn2LocalStartPosition;        // Die Startposition des Zangenzahns 2 von 2
    Quaternion zahn1BeginningRotation;      // Die Startrotation des Zangenzahns 1 von 2
    Quaternion zahn2BeginningRotation;      // Die Startrotation des Zangenzahns 2 von 2
    public float openingSpeed = 3.0f;       // Die Geschwindigkeit in der die Zange geöffnet oder geschlossen wird
    #endregion

    #region Variablen für den Fremdkörper
    Vector3 coinStartPosition;              // Startposition der Fremdkörpers
    Quaternion coinStartRotation;           // Startrotation des Fremdkörpers (Weltkoordinatensystem)
    Quaternion CoinStartLocalRotation;      // Startrotation des Fremdkörpers (Lokales Koordinatensystem)
    float coinRotationAngle;                // Winkel der Rotation des Fremdkörpers

    public float coinSpawnRadius = 2;       // Radius innerhalb dem der Fremdkörper positioniert werden kann
    public bool randomCoinPosition = false; // Boolen für die Entwicklungen und Code-Testing

    #endregion

    #region Variablen für die Rewards

    //Die folgenden Boolschen Werte vermitteln Informationen über den Kontakt zwischen Zange und Fremdkörper
    public static bool Zange1SideTouched = false;           // Zangenberührung jeder Art für Zangenzahn 1 von 2
    public static bool Zange2SideTouched = false;           // Zangenberührung jeder Art für Zangenzahn 2 von 2
    public static bool Zange1Touched = false;               // Zangenberührung im Inneren des Zangenzahns 1 von 2
    public static bool Zange2Touched = false;               // Zangenberührung im Inneren des Zangenzahns 2 von 2
    public static bool MitteTouched = false;                // Berührung der Zangenmitte zwischen den beiden Zangenzähnen (auch ohne jede Berührung der Zangenzähne)


    float distanceZahn1ToCoin;                              // Abstand zwischen Zangenzahn 1 und dem Fremdkörper
    float distanceZahn2ToCoin;                              // Abstand zwischen Zangenzahn 2 und dem Fremdkörper

    float CoinAbstandNACHaction;                                //Abstand zwischen dem äußesterm Punkt des Zangenträgers und dem Fremdkörper ( VOR der Aktion des Agents)
    float CoinAbstandVORaction;                               //Abstand zwischen dem äußesterm Punkt des Zangenträgers und dem Fremdkörper ( NACH der Aktion des Agents)

    #endregion

    #region Variablen für verschiedene Zähler während des Trainings oder Testings
    int BeruhrungJederArt = 0;            // Zähler für die Berührungen der Zangenzähne an jeder Stelle
    int BeruhrungDerInnenSeite = 0;       // Zähler für die Berührungen der Innenseite der Zangenzähne
    int AnzahlGegriffen = 0;                // Zähler für das Greifen mit beiden Zangenzähnen
    int AnzahlInDerMitte = 0;               // Zähler für die Positionierung des Fremdkörpers in der Mitte zwischen beiden Zangenzähnen, auch ohne Berührungen der Zangenzähne
    int AnzahlInDerMitteForCoin = 0;        // Ebenfalls ein Zähler für die Positionierung des Fremdkörpers in der Mitte, allerdings für andere Funktionen

    int stepCounter = 0;                    // Zählt die Aktionen/Schritte die vom Agent durchgefürt wurden
    int VersuchsCounter = 0;                // Zählt die Anzahl der Versuche
    public int ZyklusCount = 50000;         // Anzahl der Schritte in die der CoinSpawnRadius eingeteilt wird (CoindSpawnRadius / ZyklusCount = Schrittgröße )
    #endregion

    //Die Start()-Funktion wird zum Start der Anwendung aufgerufen
    // Hier werden verschiedene, der oben aufgeführten, Variablen ihren zugehörigen Werten in der 3D-Szene zugewiesen
    void Start()
    {   
        //Agent: Physik-Rigidbody mit Startposition und -rotation
        beginningPosition = this.transform.position;
        agentStartRotation = this.transform.rotation;
        AgentsRigidbody = GetComponent<Rigidbody>();

        CapsuleZangenTraegerStartRotation = CapsuleZangenTraeger.transform.rotation; //Startposition des Zangenträgers

        //Position und Rotation der beiden Zangenzähne
        Zahn1LocalStartPosition = Zahn1.transform.localPosition;
        Zahn2LocalStartPosition = Zahn2.transform.localPosition;
        zahn1BeginningRotation = ZahnPivot1.transform.localRotation;
        zahn2BeginningRotation = ZahnPivot2.transform.localRotation;
        
        //Startposition und -rotation des Fremdkörpers
        coinStartPosition = Coin.transform.position;
        coinStartRotation = Coin.transform.rotation;
        CoinStartLocalRotation = Coin.transform.localRotation;
    }

    //Die Heuristic()-Funktion ermöglicht es, Tastatur-Eingaben zu nutzen um diese wie eine Aktion des Agents zu verwenden (Zu Entwicklungszwecken)
    public override void Heuristic(float[] actionsOut)
    {   
        //Agent vor & Zurück mit den Pfeiltasten Hoch und Runter
        actionsOut[0] = Input.GetAxis("Vertical");
        //Agent rotieren mit den Pfeiltasten Links und Rechts
        actionsOut[1] = Input.GetAxis("Horizontal") * -1;
        //Gelenk beugen mit einer Bewegung der Maus auf der vertikalen Achse 
        actionsOut[2] = Input.GetAxis("Mouse Y");


        //Zangendrehung mit einer Bewegung der Maus auf horizontaler Asche
        actionsOut[3] = Input.GetAxis("Mouse X");

        //Zange öffnen und schließen mit der Drehung der Mausrads
        actionsOut[4] = Input.mouseScrollDelta.y;

    }

    //Die Funktion OnEpisodeBegin wird jedesmal aufgerufen wenn eine neue Trainingseinheit beginnt (der einzelne Versuch)
    // Dies kann zum Beispiel passieren wenn der Agent die Aufgabe löst und ein neuer Verusch gestartet wird, oder der Versuch wird abgebrochen und ein neuer gestartet
    // In unserer Funktion werden hauptsächlich die Bedingungen zum Start des Versuchs wiederhergestellt
    public override void OnEpisodeBegin()
    {   
        //Position, Rotation und physikalische Gegebenheiten des Agents werden zurückgesetzt (auf ihre Startwerte oder auf 0)
        this.transform.position = beginningPosition;
        this.transform.rotation = agentStartRotation;
        AgentsRigidbody.velocity = new Vector3(0, 0, 0);
        AgentsRigidbody.angularVelocity = new Vector3(0, 0, 0);
           
        //Alle Gelenkpunkte des Gelenks werden zurückgesetzt
        foreach(GameObject pivot in allPivots)
        {
            pivot.transform.localRotation = Quaternion.identity;

        }

        // Die Rotation des Zangenträgers wird wieder zurückgesetzt
        CapsuleZangenTraeger.transform.rotation = CapsuleZangenTraegerStartRotation;

        // Die Positionen und Rotationen der Zangenzähne wird zurückgesetzt
        Zahn1.transform.localPosition = Zahn1LocalStartPosition;
        Zahn2.transform.localPosition = Zahn2LocalStartPosition;
        ZahnPivot1.transform.localRotation = zahn1BeginningRotation;
        ZahnPivot2.transform.localRotation = zahn2BeginningRotation;

        //Alle Boolschen Werte der Kollisionen werden auf false gesetzt 
        Zange1Touched = false;
        Zange2Touched = false;
        Zange1SideTouched = false;
        Zange2SideTouched = false;
        MitteTouched = false;

        //Zu Beginn des Versuchs, die Abfrage ob der Fremkörper zufällig platziert werden soll oder nicht
        if (randomCoinPosition == true)
        {
            SpawnCoinAtRandomPositionInMesh(OesoUndMagen);  //Aufruf der Funktion welche den Fremdkörper zufällig platziert
        }
        else
        {
            Coin.transform.position = coinStartPosition;    // Position des Fremdkörpers bleibt stets die gleiche
        }

        VersuchsCounter++;  //Der Zähler für die Anzahl der Versuche wird erhöhrt
        Debug.Log("Die Anzahl der Versuche: " + VersuchsCounter); //Die Anzahl der Versuche wird ausgegeben
    }

    // In der Funktion CollectObservations() können wir definieren welche Teile der 3D-Szene, dem Agent als Observationen dienen (zum Beispiel Vektoren oder Winkel)
    public override void CollectObservations(VectorSensor sensor)
    {
        //Observationen zum Agent selbst
        sensor.AddObservation(beginningPosition - this.transform.position);     // Vektor zwischen Start- und aktueller Position   (Vector3)
        sensor.AddObservation(this.transform.rotation.y);                       // Die Rotation des Agents                          (float)             
        sensor.AddObservation(pivotRotation);                                   // Die Winkel der Gelenkbeugung                     (float)
        sensor.AddObservation(CapsuleZangenTraeger.transform.localRotation.y);  // Der Winkel der Rotation des Zangenträgers        (float)
        sensor.AddObservation(Zahn1.transform.localPosition.x);                 // Die lokale Position des ersten Zangenzahn        (float)
        sensor.AddObservation(Zahn2.transform.localPosition.x);                 // Die lokale Position des zweiten Zangenzahn       (float)

        //Observationen zu Berührungen
        sensor.AddObservation(Zange1Touched);       //Berührung der Innenseite von Zangenzahn 1                                     (bool)
        sensor.AddObservation(Zange2Touched);       //Berührung der Innenseite von Zangenzahn 2                                     (bool)
        sensor.AddObservation(MitteTouched);        //Berührung der Mitte zwischen den Zangenzähnen                                 (bool)

        //Observationen zur Münze
        sensor.AddObservation(Coin.transform.position - aeusesterPunkt.transform.position);     //Vektor zwischen Fremdkörper und Zangenträger  (Vector3)
        sensor.AddObservation(CoinAbstandVORaction);                                           //Abstand zwischen Fremdkörper und Zangenmitte  (float)
    }

    //In der Funktion OnActionReceived() werden die Aktionen des Agents verarbeitet
    public override void OnActionReceived(float[] vectorAction)
    {
        //Noch bevor die Aktionen durchgeführt werden, wird der Abstand zum Fremdkörper gemessen, um diesen nach der Aktion zu vergleichen
        CoinAbstandVORaction = (aeusesterPunkt.transform.position - Coin.transform.position).magnitude;     //Länge des Vektors zwischen Rand des Zangenträger und Fremdkörper

        //------------------------------AKTIONEN--------------------------------------//
        //AKTION 1: Vor- und Zurückbewegung des Agents / Endoskops
        float MovementAction = vectorAction[0];             //Die Aktion des Agents wird einem Float zugewiesen
        AgentsRigidbody.AddForce( MovementAction * AgentsRigidbody.transform.up * movementSpeed);  //Die Aktion wirkt eine Kraft auf den Physik-Rigidbody des Agents und verschiebt ihn nach vorn

        //AKTION 2: Rotation des Agents / Endoskops
        float RotationAction = vectorAction[1];             //Die Aktion des Agents wird einem Float zugewiesen
        transform.Rotate(0, RotationAction * rotationSpeed, 0); //Die Aktion führt zu einer Rotation des Agents

        //AKTION 3: Biegen des Gelenks
        float BendingAction = vectorAction[2];              //Die Aktion des Agents wird einem Float zugewiesen
        //Wir gehen nun alle Gelenkpunkte des Gelenkes durch und wirken die Aktion mit einer Rotation darauf aus
        foreach (GameObject pivot in allPivots)             //Alle Gelenkpunkte des Gelenks werden durchgegangen
        {
            pivotRotation = pivot.transform.localRotation;                          //Zwischenzuweisung des aktuellen Winkels des Gelenkpunkts
            pivotRotation.z += BendingAction * BendingAngleSpeed * Time.deltaTime;  //Die Aktion führt eine Rotation des Gelenkpunktes durch (Z-Achse)
            pivotRotation.z = Mathf.Clamp(pivotRotation.z, -0.2f, 0.2f);            //Die mögliche Rotation der Gelenkpunkte wird zwischen -0.2 und 0.2 gehalten (Radiant)
            pivot.transform.localRotation = pivotRotation;                          //Zuweisung des veränderten Winkels auf die Rotation des Gelenkpunktes
        }

        
        //AKTION 4: Zangenträger rotieren
        float ZangenRotationAction = vectorAction[3]; //Zuweisung der Aktion des Agents
        CapsuleZangenTraeger.transform.Rotate(0, ZangenRotationAction * -ZangenRotationSpeed, 0, Space.Self); //Die Aktion führt eine Rotation des Zangenträgers aus
        

        //AKTION 5: Zange schließen und öffnen
        float ZangenOpeningAction = vectorAction[4]; //Aktion des Agents als float
        Vector3 localRight = Zahn1.transform.worldToLocalMatrix.MultiplyVector(Zahn1.transform.right);  //Hilfsvektor der die Richtung der Bewegung der Zangenzähne trägt
        Zahn1.transform.Translate((-localRight * ZangenOpeningAction) * openingSpeed);  //Aktion führt Bewegung in Richtung Hilfsvektor*-1 aus
        Zahn2.transform.Translate((localRight * ZangenOpeningAction) * openingSpeed);   //Aktion führt Bewegung in Richtung Hilfsvektor aus
        //Diese IF-Statements sind Limits für die Zangenzahn-Bewegung
        if (Zahn1.transform.localPosition.x < -0.2f) { Zahn1.transform.localPosition = new Vector3(-0.2f, 1.5f, 0); }
        if(Zahn1.transform.localPosition.x > 0) { Zahn1.transform.localPosition = new Vector3(0, 1.5f, 0); }
        if (Zahn2.transform.localPosition.x > 0.2f) { Zahn2.transform.localPosition = new Vector3(0.2f, 1.5f, 0); }
        if (Zahn2.transform.localPosition.x < 0) { Zahn2.transform.localPosition = new Vector3(0, 1.5f, 0); }
        
        //------------------------------ENDE DER AKTIONEN--------------------------------------//

        //Nach allen Aktionen des Agents wird nochmal der Abstand zum Fremdkörper ermittelt
        CoinAbstandNACHaction = (aeusesterPunkt.transform.position - Coin.transform.position).magnitude;
        
        //Die Anzahl der gezählten Aktionen wird erhöht.
        stepCounter++;

        //Nach allen Aktionen wird die Funktion GiveRewards() aufgerufen, diese verteilt die verschiedene Belohnungen
        GiveRewards(); 
    }

    //Die Funktion GiveRewards() wird nach den Aktionen des Agents aufgerufen und verteilt die Belohnungen
    void GiveRewards()
    {

        //Belohnung/Bestrafung je nach Abstand zum Fremdkörper
        //Abfrage ob der Abstand zum Fremdkörper, nach der Aktion des Agents, kleiner oder größer geworden ist
        if (CoinAbstandNACHaction < CoinAbstandVORaction)
        {
            AddReward(0.001f);  //Bei kleiner werden des Abstands: Belohnung von 0.001
        }
        else if (CoinAbstandNACHaction > CoinAbstandVORaction)
        {
            AddReward(-0.001f);     //Bei größer werden des Abstands: Bestrafung von -0.001
        }


        //Belohnung wenn sich der Fremdkörper zwischen den beiden Zangenzähnen befindet
        if (MitteTouched == true)
        {
            AddReward(0.0125f);          //Belohnung von 0.125
            AnzahlInDerMitte++;         //Zähler für die Anzahl in der Mitte wird erhöht.
            AnzahlInDerMitteForCoin++;  //Zähler für die Anzahl in der Mitte wird erhöht.
        }


        //Belohnunge bei jeder Art von Berührung zwischen Zange und Fremdkörper
        if (Zange1SideTouched == true)
        {
            AddReward(0.0025f);           //Belohnung von 0.25
            BeruhrungJederArt++;        //Zähler für Berührungen jeder Art wird hochgesetzt
        }
        if (Zange2SideTouched == true)
        {
            AddReward(0.0025f);           //Belohnung von 0.25
            BeruhrungJederArt++;        //Zähler für Berührungen jeder Art wird hochgesetzt
        }


        //Belohnugen bei der Berührung der Zangeninnenseite 1 von 2
        if (Zange1Touched)
        {
            AddReward(0.005f);          //Belohnung von 0.005
            BeruhrungDerInnenSeite++;   //Zähler für Berührungen der Innenseite hochsetzten
        }
        //Belohnugen bei der Berührung der Zangeninnenseite 2 von 2
        if (Zange2Touched)
        {
            AddReward(0.005f);          //Belohnung von 0.005
            BeruhrungDerInnenSeite++;   //Zähler für Berührungen der Innenseite hochsetzten
        }
         

        //Belohnung bei Griff (in zwei versch. Ausführungen: 1. mit bools 2. mit Abständen)
        //Belohnung bei Griff durch beide Zangenzähne (Ermittelt über die boolschen Werte)
        if (Zange1Touched == true && Zange2Touched == true)
        {
            AddReward(1f);                 //Belohnung von 1
            AnzahlGegriffen++;                  //Zähler hochsetzten

            EndEpisode();           //Bei Greifen wird der Versuch beendet
        }

        //Wir ermitteln noch mit einem anderen Verfahren ob der Fremdkörper gegriffen wurde: wir messen den Abstand zwischen den beiden Zangenzähnen und der Münze.
        distanceZahn1ToCoin = (Zahn1.transform.position - Coin.transform.position).magnitude;   //Abstand zwischen Zangenzahn 1 und dem Fremdkörper
        distanceZahn2ToCoin = (Zahn2.transform.position - Coin.transform.position).magnitude;   //Abstand zwischen Zangenzahn 2 und dem Fremdkörper

        //Belohnung bei Griff durch beide Zangenzähne (Ermittelt über den Abstand zwischen Fremdkörper und Zangenzähnen)
        //Wenn Abstand zwischen Fremdkörper und den beiden Zanganzähnen kleiner als 0.135 gilt der Fremdkörper als gegriffen
        if (distanceZahn1ToCoin < 0.135f && distanceZahn2ToCoin < 0.135f)
        {
            AddReward(1f);                 //Belohnung von 1
            AnzahlGegriffen++;                  //Zähler erhöhen

            Zange1Touched = true;   //Der Boolsche Wert wird hier auf True gesetz da dieser auch als Observation genutzt wird
            Zange2Touched = true;   //Der Boolsche Wert wird hier auf True gesetz da dieser auch als Observation genutzt wird

            EndEpisode();           //Bei Greifen wird der Versuch beendet
        }

        //Zuletzt werden die genutzten Zähler in der Console ausgegeben
        Debug.Log("Berührungen der jeder Art: " + BeruhrungJederArt);
        Debug.Log("Berührungen der einzelnen Innenseite: " + BeruhrungDerInnenSeite);
        Debug.Log("In der Mitte gewesen: " + AnzahlInDerMitte);
        Debug.Log("Gegriffen mit beiden Zähnen: " + AnzahlGegriffen);
        Debug.Log("Anzahl der Erfahrungen: " + stepCounter);
    }

    //Die Funktion OnCollisionEnter() wird aufgerufen sobald eine Kollision stattfindet
    void OnCollisionEnter(Collision collision)
    {   
        //Abfrage ob das Objekt, mit dem kollidiert wurde, der Körper ist
        if(collision.gameObject.name == "ÖsophagusEingang" || collision.gameObject.name == "HumanBodyOriginal")
        {
            AddReward(-1);  //Falls ja, Bestrafung von -1
            EndEpisode();   //Ende des fehlgeschlagenen Versuchs und Start eines neuen Versuchs
        }     
    }

    //Die Funktion SpawnCoinAtRandomPositionInMesh() wird aufgerufen sobald ein neuer Versuch gestartet wird und positioniert den Fremdkörper neu und passt seine Rotation an
    void SpawnCoinAtRandomPositionInMesh(GameObject gameObjectWithMesh)
    {     
        //POSITION DES FREMDKÖRPERS 
       //Das Prinzip: 1. Es wird ein zufälliger Vektor bestimmt 2. Die Länge des Vektors wird bestimmt 3. Der Fremdkörper wird mit diesem Vektor verschoben 4. Die Rotation wird angepasst
        Vector3 RandomVector = Random.insideUnitSphere.normalized;                          // Zufälliger Vektor wird erstellt
        RandomVector.z = Mathf.Clamp(RandomVector.z, 0, 1);                                 // z-wert des Vektors soll positiv (0- 1) bleiben, um nicht in den Agent hinein zu zeigen
        
        //Die Länge des Vektor hängt entweder davon ab wie viele Aktioen bereits getätigt wirden ODER wie oft das Ziel erreicht wurde
        //float increasingDistance = (coinSpawnRadius / ZyklusCount) * stepcounter                  //(Radius / Teilungen) * Schritt     
        float increasingDistance = (coinSpawnRadius / ZyklusCount) * (AnzahlInDerMitteForCoin);     //(Radius / Teilungen) * Erfolge
        Vector3 ThisVector = RandomVector * increasingDistance;                                     //Die bestimmte Länge wird auf den Vektor angewnadt
        ThisVector = CoinStartLocalRotation * ThisVector;                                           //Der Vektor wird an die Rotation des Fremdkörpers angepasst (Entwicklungszwecke)
        Coin.transform.position = coinStartPosition + ThisVector;                                   //Die Position des Fremdkörpers = Startposition + NeuerVektor


        //ROTATION DES FREMDKÖRPERS
        //Das Prinzip: Entweder rein zufällige Rotation ODER eine die von der Anzahl der Aktionen abhängt ODER eine die von der Anzahl der Erfolge abhängt
        Coin.transform.rotation = coinStartRotation;                //Zuvor kurz die Rotation des Fremdkörpers zurücksetzten
        //Coin.transform.rotation = Random.rotation;               //zufällige Rotation                   
        //coinRotationAngle = ((float)360 / ZyklusCount) * stepcounter; //Abhängig von Anzahl der Aktionen          (360/Teilung) * Aktionen
        coinRotationAngle = ((float)360 / ZyklusCount) * AnzahlInDerMitteForCoin;   //Abhängig von Anzahl der Erfolge       (360/Teilungen) * Erfolge                           
        Coin.transform.Rotate(coinRotationAngle, coinRotationAngle, coinRotationAngle); //Der neu Bestimmte Rotationwinkel 


        //ZURÜCKSETZTEN DES INTERVALLS
        // Falls es mehr Erfolge als Teilungen gab
        if (AnzahlInDerMitteForCoin > ZyklusCount)
        {
            AnzahlInDerMitteForCoin = 0;            //Erfolgszähler wird zurückgesetzt
            coinSpawnRadius += 1;                   //Der mögliche Radius erhöht sich
            ZyklusCount += ZyklusCount;             //Die Teilungen müssen passend erhöht werden
            if (coinSpawnRadius > 5)                //Falls der Radius größer als 5 ist
            {
                coinSpawnRadius = 1;                //Radius wieder verkleinern
                ZyklusCount = 10;                   //Die Teilungen anpassenn
            }
        }
    }
}
