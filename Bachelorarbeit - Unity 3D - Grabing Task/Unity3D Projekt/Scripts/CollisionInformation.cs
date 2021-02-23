using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInformation : MonoBehaviour
{   
    // Das sind die beiden Zangenzähne in der 3D-Szene
    public GameObject Zange1;
    public GameObject Zange2;


    //Die Funktion OnCollisionEnter wird aufgerufen sobald es eine Kollision gegeben hat
    private void OnCollisionEnter(Collision collision)
    {
        //Das ist der Normalenvektor derjenigen Fläche, die berührt wurde
        Vector3 normal = collision.contacts[0].normal;

        //Abfrage ob und welcher welcher Zangenzahn berührt wurde
        if (collision.collider.gameObject.name == "ZangeLowPoly1")
        {   
            //Falls Zahn1 berührt wurde-> im Agent-Skript den Boolean Zange1SideTouched auf true
            EndoscopeAgentScript.Zange1SideTouched = true;

            //Abfrage ob die Innenseite des Zangenzahns berührt wurde (Normale der Kollisionsfläche == Normale des Zangenzahns?)
            if (normal == Zange1.transform.right * -1 )
            {
                //Falls die Innenseite berührt wurde-> im Agent-Skript den Boolean Zange1Touched auf true
                EndoscopeAgentScript.Zange1Touched = true;
            }
        }
        else if (collision.collider.gameObject.name == "ZangeLowPoly2")
        {
            //Falls Zahn2 berührt wurde-> im Agent-Skript den Boolean Zange2SideTouched auf true
            EndoscopeAgentScript.Zange2SideTouched = true;

            //Abfrage ob die Innenseite des Zangenzahns berührt wurde (Normale der Kollisionsfläche == Normale des Zangenzahns?)
            if (normal == Zange2.transform.right )
            {
                //Falls die Innenseite berührt wurde-> im Agent-Skript den Boolean Zange2Touched auf true
                EndoscopeAgentScript.Zange2Touched = true;
            }
        }

        if(collision.collider.gameObject.name == "MitteDerZange")
        {
            EndoscopeAgentScript.MitteTouched = true;
        }
    }


    private void OnCollisionExit(Collision collision)
    {

        if (collision.collider.gameObject.name == "ZangeLowPoly1")
        {
            EndoscopeAgentScript.Zange1Touched = false;
            EndoscopeAgentScript.Zange1SideTouched = false;
        }

        if (collision.collider.gameObject.name == "ZangeLowPoly2")
        {
            EndoscopeAgentScript.Zange2Touched = false;
            EndoscopeAgentScript.Zange2SideTouched = false;
        }
    }
}
