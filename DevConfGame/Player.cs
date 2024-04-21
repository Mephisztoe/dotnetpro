using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DevConfGame;

internal class Player
{
    private Vector2 position = new(50, 50);
    private int speed = 200;

    public Vector2 Position { get => position; }

    public void SetX(float newX) => position.X = newX;
    
    public void SetY(float newY) => position.Y = newY;
    
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        position.X += speed * dt;
    }
}
