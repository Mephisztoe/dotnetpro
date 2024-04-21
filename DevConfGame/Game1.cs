using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Linq;

namespace DevConfGame;

enum Direction
{
    Up, 
    Down, 
    Left, 
    Right
}


public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D playerSprite;
    private OrthographicCamera camera;
    Player player = new();
    Texture2D map;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 320, 180);
        camera = new OrthographicCamera(viewportAdapter);

        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = 1600;
        graphics.PreferredBackBufferHeight = 900;
        graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        playerSprite = Content.Load<Texture2D>("Player/PlayerIdle");
        map = Content.Load<Texture2D>("Maps/Map");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        player.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        var transformationMatrix = camera.GetViewMatrix();
        spriteBatch.Begin(transformMatrix: transformationMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(map, new Vector2(0, -40), Color.White);
        spriteBatch.Draw(playerSprite, player.Position, Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private static Texture2D GenerateRandomTexture(GraphicsDevice device, int width, int height)
    {
        Random rnd = new();
        var data = Enumerable.Range(0, width * height)
                .Select(_ => new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)))
                .ToArray();

        var texture = new Texture2D(device, width, height);
        texture.SetData(data);

        return texture;
    }
}
