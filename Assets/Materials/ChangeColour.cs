using System;
using System.Collections;
using UnityEngine;

public class ChangeColour : MonoBehaviour
{
    private enum Direction {
        Forward,
        Backward
    }

    [field: SerializeField]
    public Color StartColour { get; private set; }

    [field: SerializeField]
    public Color EndColour { get; private set; }

    [field: SerializeField]
    public float Time { get; private set; } = 0;


    private Renderer _renderer;
    private Direction _direction = Direction.Forward;
    private float _offset = 0;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(ColourChange());
    }

    IEnumerator ColourChange() {
        while (true)
        {
            //_renderer.material.color = Color.cyan; // change material as new instance
            //_renderer.sharedMaterial.color = Color.cyan; // change material shared between other objects

            if (_direction == Direction.Forward)
            {
                _offset = Math.Min(_offset + UnityEngine.Time.deltaTime, Time);
                if (_offset == Time)
                    _direction = Direction.Backward;
            }
            else
            {
                _offset = Math.Max(_offset - UnityEngine.Time.deltaTime, 0);
                if (_offset == 0)
                    _direction = Direction.Forward;
            }
            _renderer.material.SetColor("_Color", Color.Lerp(StartColour, EndColour, _offset / Time));
            yield return null;
        }
    }
}
