using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
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
    TiledMap tiledMap;
    TiledMapRenderer tiledMapRenderer;
    private Texture2D walkUp;
    private Texture2D walkDown;
    private Texture2D walkLeft;
    private Texture2D walkRight;
    private Texture2D idleDown;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {        
        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 320, 180);
        camera = new OrthographicCamera(viewportAdapter);

        camera.LookAt(player.Position);

        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = 1600;
        graphics.PreferredBackBufferHeight = 900;
        graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
       
        playerSprite = Content.Load<Texture2D>("Player/PlayerIdle");
        walkUp = Content.Load<Texture2D>("Player/WalkUp");
        walkDown = Content.Load<Texture2D>("Player/WalkDown");
        walkLeft = Content.Load<Texture2D>("Player/WalkLeft");
        walkRight = Content.Load<Texture2D>("Player/WalkRight");
        idleDown = Content.Load<Texture2D>("Player/IdleDown");

        tiledMap = Content.Load<TiledMap>("Maps/Map");
        tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, tiledMap);

        player.animations[0] = new Sprite(walkUp, 4, 8);
        player.animations[1] = new Sprite(walkDown, 4, 8);
        player.animations[2] = new Sprite(walkLeft, 4, 8);
        player.animations[3] = new Sprite(walkRight, 4, 8);
        player.animations[4] = new Sprite(idleDown, 8, 14);

        player.anim = player.animations[0];

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
               
        player.Update(gameTime);
        tiledMapRenderer.Update(gameTime);

        //camera.LookAt(player.Position);
        Vector2 delta = player.Position - camera.Position - new Vector2(152, 82);
        camera.Position += (delta * 0.08f);


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Vector2 oldCamPosition = camera.Position;
        camera.Position = Vector2.Round(camera.Position * 5) / 5.0f;

        var transformationMatrix = camera.GetViewMatrix();
        tiledMapRenderer.Draw(transformationMatrix);

        camera.Position = oldCamPosition;

        spriteBatch.Begin(transformMatrix: transformationMatrix, samplerState: SamplerState.PointClamp);
        player.anim.Draw(spriteBatch);
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
