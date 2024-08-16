using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevConfGame;

public abstract class GameCharacter(Game game, Vector2 startPosition, int movementSpeed)
{
    protected readonly Game1 Game = (Game1)game;
    protected AnimatedSprite sprite;
    protected Vector2 position = startPosition;
    protected readonly int speed = movementSpeed;
    protected Direction direction = Direction.Right;
    protected bool isMoving = false;

    public Vector2 Position => position;

    public Direction Direction => direction;

    // Öffentliche Methoden
    public abstract void LoadContent();

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(sprite, position);
    }

    public abstract void Update(GameTime gameTime);

    public void SetX(float newX) => position.X = newX;

    public void SetY(float newY) => position.Y = newY;


    protected void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = .15f)
    {
        var cycle = new SpriteSheetAnimationCycle
        {
            IsLooping = isLooping,
            FrameDuration = frameDuration
        };

        foreach (var frame in frames)
        {
            cycle.Frames.Add(new SpriteSheetAnimationFrame(frame, frameDuration));
        }

        spriteSheet.Cycles.Add(name, cycle);
    }
}