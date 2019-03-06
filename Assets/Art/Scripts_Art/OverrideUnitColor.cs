using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class OverrideUnitColor : MonoBehaviour
{
    public Color Color = Color.white;

    private MaterialPropertyBlock _mpb;
    public static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

	void Start ()
    {
        _mpb = new MaterialPropertyBlock();
	}

#if UNITY_EDITOR
    private void Update()
    {
        ApplyColor();
    }
#endif

    public void ApplyColor()
    {
#if UNITY_EDITOR
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
#endif
        _mpb.SetColor(ColorPropertyId, Color);
        var renderers = transform.GetComponentsInChildren<Renderer>(false);
        foreach (var r in renderers)
            r.SetPropertyBlock(_mpb);
    }
}
