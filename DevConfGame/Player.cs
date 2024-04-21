using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DevConfGame;

internal class Player
{
    private Vector2 _position = new(50, 50);
    private int _speed = 200;

    public Vector2 Position { get => _position; }

    public void SetX(float newX)
    {
        _position.X = newX;
    }

    public void SetY(float newY)
    {
        _position.Y = newY;
    }
}
