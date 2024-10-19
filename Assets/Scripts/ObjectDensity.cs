using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDensity : MonoBehaviour
{
    [SerializeField] [Range(0,1)] float density = 1;
    public float Density { get { return density; } }

    // Start is called before the first frame update
    void Start()
    {
        Material material = GetComponent<MeshRenderer>().material;
        if (material != null){
            material.color = new Color(material.color.r, material.color.g, material.color.b, density);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
