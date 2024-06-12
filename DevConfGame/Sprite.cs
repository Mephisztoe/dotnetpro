using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DevConfGame;

public class Sprite
{
    protected Texture2D spriteSheet;
    public Vector2 position = Vector2.Zero;
    public Color color = Color.White;
    public Vector2 origin;
    public float rotation = 0f;
    public float scale = 1f;
    public SpriteEffects spriteEffect;
    protected Rectangle[] frames;
    protected int frameIndex = 0;
    private float timeElapsed;
    public bool IsLooping = true;
    private float timeToUpdate;

    public int FramesPerSecond
    {
        set
        {
            timeToUpdate = (1f / value);
        }
    }

    public Sprite(Texture2D spriteSheet, int frameCount, int fps)
    {
        FramesPerSecond = fps;
        this.spriteSheet = spriteSheet;
        int width = spriteSheet.Width / frameCount;

        frames = new Rectangle[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            frames[i] = new Rectangle(i * width, 0, width, spriteSheet.Height);
        }
    }

    public void Update(GameTime gameTime)
    {
        timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (timeElapsed > timeToUpdate)
        {
            timeElapsed -= timeToUpdate;

            if (frameIndex < frames.Length - 1)
                frameIndex++;

            else if (IsLooping)
                frameIndex = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(spriteSheet, position, frames[frameIndex], color, rotation, origin, scale, spriteEffect, 0f);
    }

    public void SetFrame(int frame)
    {
        frameIndex = frame;
    }
}
