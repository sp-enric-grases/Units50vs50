using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetFrameRate : MonoBehaviour
{
    public int frameRate = 60;
    private List<Animator> anims = new List<Animator>();

	void Start ()
    {
        Application.targetFrameRate = frameRate;

        anims = GetComponentsInChildren<Animator>().ToList();
	}

    public void Attack()
    {
        foreach (var item in anims)
        {
            item.SetTrigger("Attack");
        }
    }
}
