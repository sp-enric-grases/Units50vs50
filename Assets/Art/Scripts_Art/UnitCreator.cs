using UnityEngine;

public class UnitCreator : MonoBehaviour
{
    public bool useAnimation;
    public GameObject prefab;
    public int rows, columns;
    public Vector2 spaceBetweenUnits;

	void Start ()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                Vector3 pos = new Vector3(transform.position.x + c * spaceBetweenUnits.x, 0, transform.position.z + r * spaceBetweenUnits.x);
                GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);
                if (!useAnimation) Destroy(go.GetComponent<Animator>());
            }
        }
	}
}
