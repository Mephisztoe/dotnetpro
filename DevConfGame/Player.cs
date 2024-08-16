using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using System;

namespace DevConfGame;

public class Player(Game game) :
    GameCharacter(game, new Vector2(80, 90), 85)
{
    public override void LoadContent()
    {
        var spriteTexture = Game.Content.Load<Texture2D>("Player/SpriteSheet");
        var spriteAtlas = Texture2DAtlas.Create("spriteAtlas", spriteTexture, 16, 16);
        var spriteSheet = new SpriteSheet("spriteSheet", spriteAtlas);

        AddAnimationCycle(spriteSheet, "walkLeft", [0, 4, 0, 8]);
        AddAnimationCycle(spriteSheet, "walkRight", [3, 7, 3, 11]);
        AddAnimationCycle(spriteSheet, "walkUp", [2, 6, 2, 10]);
        AddAnimationCycle(spriteSheet, "walkDown", [1, 5, 1, 9]);
        AddAnimationCycle(spriteSheet, "idleDown", [1, 12, 13, 14, 14, 13, 12, 1], frameDuration: .08f);

        sprite = new AnimatedSprite(spriteSheet, "idleDown")
        {
            OriginNormalized = new Vector2(0, 0)
        };
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            var movement = direction switch
            {
                Direction.Up => new Vector2(0, -speed * dt),
                Direction.Down => new Vector2(0, speed * dt),
                Direction.Left => new Vector2(-speed * dt, 0),
                Direction.Right => new Vector2(speed * dt, 0),
                _ => Vector2.Zero
            };

            position += movement;

            var animation = direction switch
            {
                Direction.Up => "walkUp",
                Direction.Down => "walkDown",
                Direction.Left => "walkLeft",
                Direction.Right => "walkRight",
                _ => sprite.CurrentAnimation // falls direction nicht passt, bleibt die aktuelle Animation
            };

            if (!sprite.CurrentAnimation.Equals(animation))
            {
                sprite.SetAnimation(animation);
            }
        }
        else
        {
            if (!sprite.CurrentAnimation.Equals("idleDown"))
                sprite.SetAnimation("idleDown");
        }

        position = Vector2.Round(position * 5) / 5.0f;

        Game1.DebugRects.Add(new Tuple<RectangleF, Color>(
            new RectangleF(position.X + 2, position.Y + 12, 12, 4), Color.Red));

        sprite.Update(gameTime);
    }
}
