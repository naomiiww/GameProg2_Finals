using UnityEngine;

public interface ISwitchable
{
    ColorSet CurrentColor { get; }
    void SetColor(ColorSet newColor);
}

public enum ColorSet
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Violet = 3
}