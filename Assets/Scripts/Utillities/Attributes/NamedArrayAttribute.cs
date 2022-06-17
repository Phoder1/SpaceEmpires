using UnityEngine;
using System;

public class NamedArrayAttribute : PropertyAttribute
{
    private string _elementName;
    public string ElementName => _elementName;

    public NamedArrayAttribute(string elementName = "Name")
    {
        _elementName = elementName ?? throw new ArgumentNullException(nameof(elementName));
    }
}