using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class IconBehaviour : MonoBehaviour
{

    bool up = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpDown());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSomething()
    {
        transform.Rotate(new UnityEngine.Vector3(0,Time.deltaTime*10, 0));
    }

    IEnumerator UpDown()
    {
        while (true)
        {
            float step  = Time.deltaTime*100;
            if (up)
            {
                transform.localScale.Scale(new UnityEngine.Vector3(step, 0, step));
                if (transform.localScale.x >= 10)
                    up = false;
            }
            else
            {
                transform.localScale.Scale(new UnityEngine.Vector3(step, 0,step));
                if (transform.localScale.x <= 0)
                    up = true;
            }
            yield return null;
        }
    }   
}
