using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;

namespace DevConfGame;

internal class Player(AnimatedSprite sprite)
{
    private Vector2 position = new(80, 90);
    private int speed = 85;
    private Direction direction = Direction.Right;
    private bool isMoving = false;

    public Vector2 Position { get => position; }

    public Direction Direction { get => direction; }

    public void SetX(float newX) => position.X = newX;

    public void SetY(float newY) => position.Y = newY;

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime
            .TotalSeconds;

        var keyboardState = Keyboard.GetState();

        isMoving = false;

        if (keyboardState.IsKeyDown(Keys.Up))
        {
            direction = Direction.Up;
            isMoving = true;
        }

        if (keyboardState.IsKeyDown(Keys.Down))
        {
            direction = Direction.Down;
            isMoving = true;
        }

        if (keyboardState.IsKeyDown(Keys.Left))
        {
            direction = Direction.Left;
            isMoving = true;
        }

        if (keyboardState.IsKeyDown(Keys.Right))
        {
            direction = Direction.Right;
            isMoving = true;
        }

        if (isMoving)
        {
            switch (direction)
            {
                case Direction.Up:
                    position.Y -= speed * dt;
                    sprite.Play("walkUp");
                    break;
                case Direction.Down:
                    position.Y += speed * dt;
                    sprite.Play("walkDown");
                    break;
                case Direction.Left:
                    position.X -= speed * dt;
                    sprite.Play("walkLeft");
                    break;
                case Direction.Right:
                    position.X += speed * dt;
                    sprite.Play("walkRight");
                    break;
                default:
                    break;
            }
        }
        else
        {
            sprite.Play("idleDown");
        }

        position = Vector2.Round(position * 5) / 5.0f;

        sprite.Update(gameTime);
    }
}
