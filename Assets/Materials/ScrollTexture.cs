using System.Collections;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    private Renderer _renderer;
    private float _offset;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(TextureScroll());
    }

    IEnumerator TextureScroll() {
        while (true)
        {
            _offset = (_offset + Time.deltaTime)%2;
            // If you want the texture to wrap when it scrolls, make sure the 'Wrap' property is set on the texture
            _renderer.material.SetTextureOffset("_MainTex", new Vector2(_offset-1, 0));
            yield return null;
        }
    }
}
