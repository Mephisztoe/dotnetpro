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
using System.Collections.Generic;

namespace DevConfGame;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
public enum ScreenName
{
    MainRoom,
    Locker
}

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;

    public SpriteBatch SpriteBatch { get; private set; }

    public OrthographicCamera Camera { get; private set; }

    Player player;

    public ImGuiRenderer GuiRenderer { get; private set; }

    public CollisionDetector CollisionDetector { get; private set; }

    TiledMap tiledMap;
    TiledMapRenderer tiledMapRenderer;
    TiledMapTileLayer floorLayer;
    TiledMapTileLayer decorationLayer;
    TiledMapTileLayer foregroundLayer;

    bool enableCollisionDetection = true;
    bool enableDebugRect = true;
    bool enableForegroundLayer = true;
    bool enableDecorationLayer = true;
    bool enableFloorLayer = true;
    
    static public List<Tuple<RectangleF, Color>> DebugRects = [];

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        player = new Player(this);
    }

    protected override void Initialize()
    {
        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 320, 180);
        Camera = new OrthographicCamera(viewportAdapter);

        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = 1600;
        graphics.PreferredBackBufferHeight = 900;
        graphics.ApplyChanges();

        GuiRenderer = new ImGuiRenderer(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        tiledMap = Content.Load<TiledMap>("Maps/MainRoom");
        tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, tiledMap);
        floorLayer = tiledMap.GetLayer<TiledMapTileLayer>("Floor");
        decorationLayer = tiledMap.GetLayer<TiledMapTileLayer>("Decoration");
        foregroundLayer = tiledMap.GetLayer<TiledMapTileLayer>("Foreground");

        CollisionDetector = new CollisionDetector(tiledMap);

        player.LoadContent();


        GuiRenderer.RebuildFontAtlas();
    }   

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        DebugRects.Clear();

        // Backup Position
        var playerPos = player.Position;

        player.Update(gameTime);
        tiledMapRenderer.Update(gameTime);

        bool collision = CollisionDetector.CollisionCheck(floorLayer, player.Position, player.Direction);

        if (enableCollisionDetection && collision)
        {
            // Revert Position
            player.SetX(playerPos.X);
            player.SetY(playerPos.Y);
        }
       
        Vector2 delta = player.Position - Camera.Position - new Vector2(152, 82);
        Camera.Position += (delta * 0.08f);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        Vector2 oldCamPosition = Camera.Position;
        Camera.Position = Vector2.Round(Camera.Position * 5) / 5.0f;

        Camera.Position = new Vector2(
            MathHelper.Clamp(Camera.Position.X, -20, 20),
            MathHelper.Clamp(Camera.Position.Y, -20, 160));

        var transformationMatrix = Camera.GetViewMatrix();

        if (enableFloorLayer)
            tiledMapRenderer.Draw(floorLayer, viewMatrix: transformationMatrix);

        if (enableDecorationLayer)
            tiledMapRenderer.Draw(decorationLayer, viewMatrix: transformationMatrix);

        Camera.Position = oldCamPosition;

        SpriteBatch.Begin(transformMatrix: transformationMatrix, samplerState: SamplerState.PointClamp);
        player.Draw(gameTime, SpriteBatch);
      
        DebugRects.Add(new Tuple<RectangleF, Color>(new RectangleF(player.Position.X + 2, player.Position.Y + 12, 12, 4), Color.Red));

        foreach (var debugRect in DebugRects)
        {
            DrawDebugRect(SpriteBatch, debugRect.Item1, 1, debugRect.Item2);
        }

        SpriteBatch.End();

        base.Draw(gameTime);

        if (enableForegroundLayer)
            tiledMapRenderer.Draw(foregroundLayer, viewMatrix: transformationMatrix);

        GuiRenderer.BeginLayout(gameTime);
        DrawImGuiOverlay(frameRate);
        ImGui.Begin("Collision Details");
        ImGui.Checkbox("Debug Rect", ref enableDebugRect);
        ImGui.Checkbox("Collision Detection", ref enableCollisionDetection);
        ImGui.End();
        ImGui.Begin("Tilemap");
        ImGui.Checkbox("Show Floor Layer", ref enableFloorLayer);
        ImGui.Checkbox("Show Decoration Layer", ref enableDecorationLayer);
        ImGui.Checkbox("Show Foreground Layer", ref enableForegroundLayer);
        ImGui.End();

        GuiRenderer.EndLayout();
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

    private void DrawDebugRect(SpriteBatch batch, RectangleF rect, int border, Color color)
    {
        if (!enableDebugRect)
            return;

        batch.DrawRectangle(rect, color, .2f);
    }    
}
