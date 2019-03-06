using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAnimations : MonoBehaviour
{
    public Animator archer1, archer2;
    public float speed;

	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            archer1.SetTrigger("Attack");
            archer2.SetTrigger("Attack");
        }

        archer1.SetFloat("speed", speed);
        archer2.SetFloat("speed", speed);
    }
}
