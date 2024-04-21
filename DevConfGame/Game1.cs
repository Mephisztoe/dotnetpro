﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Linq;

namespace DevConfGame;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D playerSprite;
    private OrthographicCamera camera;

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
        playerSprite = GenerateRandomTexture(GraphicsDevice, 16, 16);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        var transformationMatrix = camera.GetViewMatrix();
        spriteBatch.Begin(transformMatrix: transformationMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(playerSprite, new Vector2(50, 50), Color.White);
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
