using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateObject : MonoBehaviour
{

    public Material mat1 = null;
    public Material mat2 = null;
    private int count = 0;
    public bool changeChildren = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            transform.Rotate(Vector3.up, Time.deltaTime*60f);
        }
        if (Input.GetKey(KeyCode.F))
        {
            transform.Rotate(Vector3.up, Time.deltaTime*10f);
        }

        if (Input.GetKeyDown("m"))
        {
            if (mat1 != null)
            {
                Debug.Log("count: " + count);
                Material m;
                if (count == 1) m = mat2;
                else m = mat1;
                
                if (changeChildren)
                {
                    for(int i = 0; i<transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        
                        child.gameObject.GetComponent<Renderer>().material = m;
                    }
                    count++;
                    if (count >= 2) count = 0;
                    return;
                }
                
                GetComponent<Renderer>().material = m;

                count++;
                if (count >= 2) count = 0;
            }
        }
    }
}
