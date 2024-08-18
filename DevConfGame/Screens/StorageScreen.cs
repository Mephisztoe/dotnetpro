using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using System;

namespace DevConfGame.Screens;

public class StorageScreen(Game game, SpriteBatch spriteBatch) : GameScreen(game)
{
    private new GameMain Game => (GameMain)base.Game;

    public override void Draw(GameTime gameTime)
    {
        throw new NotImplementedException();
    }

    public override void Update(GameTime gameTime)
    {
        throw new NotImplementedException();
    }
}
