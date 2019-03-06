using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchObjects : MonoBehaviour
{
    private List<GameObject> children = new List<GameObject>();
    private int numChild = 0;

	void Start ()
    {
        Application.targetFrameRate = 60;

        for (int i = 0; i < transform.childCount; i++)
            children.Add(transform.GetChild(i).gameObject);

        HideAllChildren();
        ShowChild(numChild);
    }
	
    public void ShowObject()
    {
        numChild++;
        if (numChild >= children.Count)
            numChild = 0;

        HideAllChildren();
        ShowChild(numChild);
    }

    private void HideAllChildren()
    {
        foreach (var child in children)
            child.SetActive(false);
    }

    private void ShowChild(int num)
    {
        children[num].SetActive(true);
    }
}
