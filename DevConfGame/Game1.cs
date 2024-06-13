using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Linq;
using MonoGame.ImGuiNet;
using ImGuiNET;

using Vec2 = System.Numerics.Vector2;
using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

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
    public static ImGuiRenderer guiRenderer;

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

        guiRenderer = new ImGuiRenderer(this);

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

        guiRenderer.RebuildFontAtlas();

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
        float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

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

        guiRenderer.BeginLayout(gameTime);
        DrawImGuiOverlay(frameRate);
        ImGui.Begin("Collision Details");
        ImGui.End();
        guiRenderer.EndLayout();
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

    //OverlayVariables
    float distanceX = 10.0f;
    float distanceY = 10.0f;
    int corner = 0;

    private void DrawImGuiOverlay(float frameRate)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMove;

        if (corner >= 0 && corner < 4)
        {
            //offset X
            if (corner == 1 || corner == 3)
            {
                distanceX = (float)graphics.PreferredBackBufferWidth - 250.0f;
            }
            else
            {
                distanceX = 10.0f;
            }
            //offset Y
            if (corner == 2 || corner == 3)
            {
                distanceY = (float)graphics.PreferredBackBufferHeight - 100.0f;
            }
            else
            {
                distanceY = 10.0f;
            }


            Vec2 windowPosition = new Vec2(distanceX, distanceY);
            ImGui.SetNextWindowPos(windowPosition);
        }

        ImGui.SetNextWindowBgAlpha(0.35f);
        if (ImGui.Begin("Example: Simple overlay", windowFlags))
        {
            ImGui.Text("Simple overlay\nin the corner of the screen\n(right-click to change position)");
            ImGui.Separator();
            if (ImGui.IsMousePosValid())
            {
                ImGui.Text(string.Format("Mouse Position: ({0},{1})", io.MousePos.X, io.MousePos.Y));
            }
            else
            {
                ImGui.Text("Mouse Position: <invalid>");
            }

            ImGui.Text(string.Format("Frames per second: {0}", frameRate.ToString()));

            if (ImGui.BeginPopupContextWindow())
            {
                //if (ImGui.MenuItem("Custom", null, corner == -1)) { corner = -1; }
                if (ImGui.MenuItem("Top-left", null, corner == 0)) { corner = 0; }
                if (ImGui.MenuItem("Top-right", null, corner == 1)) { corner = 1; }
                if (ImGui.MenuItem("Bottom-left", null, corner == 2)) { corner = 2; }
                if (ImGui.MenuItem("Bottom-right", null, corner == 3)) { corner = 3; }

                ImGui.EndPopup();
            }
        }

        ImGui.End();
    }
}
