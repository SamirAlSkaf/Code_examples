using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FlipFaces : MonoBehaviour
{
    void Start()
    {
        //In folgender Zeile wird das Mesh des Körpers auf die Variable mesh zugewiesen
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        // In der nächsten Zeile werden alle geformten Dreiecke des Meshes in ihrer Aufzählung umgedreht.
        mesh.triangles = mesh.triangles.Reverse().ToArray();
    }
}
