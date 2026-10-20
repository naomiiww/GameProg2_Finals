using UnityEngine;

public interface ISwitchable
{
    // allows the player to change their "State" or "Color" when clicked.
    //When the player's state changes, both the player and the bullets they shoot must visually update to match that new state/color.
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