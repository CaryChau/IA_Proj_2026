using System;
using UnityEngine;

public class ProfileViewModel
{
    public event Action<string> NameChanged;
    public event Action<string> MetaChanged;
    public event Action<Texture2D> HeaderChanged;
    private string _name;
    private string _meta;
    private Texture2D _headerTexture;

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            NameChanged?.Invoke(_name);
        }
    }

    public string Meta
    {
        get => _meta;
        set
        {
            if (_meta == value) return;
            _meta = value;
            MetaChanged?.Invoke(_meta);
        }
    }

    public Texture2D HeaderTexture
    {
        get => _headerTexture;
        set
        {
            if (_headerTexture == value) return;
            _headerTexture = value;
            HeaderChanged?.Invoke(_headerTexture);
        }
    }

    /// Optional helper: set all at once
    public void SetAll(string name, string meta, Texture2D headerTexture)
    {
        Name = name;
        Meta = meta;
        HeaderTexture = headerTexture;
    }
}