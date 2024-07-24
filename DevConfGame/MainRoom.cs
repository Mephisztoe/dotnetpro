using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using System;

namespace DevConfGame;

public class MainRoom(Game game) : GameScreen(game)
{
    private new Game1 Game => (Game1)base.Game;

    public override void Draw(GameTime gameTime)
    {
        throw new NotImplementedException();
    }

    public override void Update(GameTime gameTime)
    {
        throw new NotImplementedException();
    }
}
