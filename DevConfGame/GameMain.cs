using DevConfGame.Screens;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using MonoGame.ImGuiNet;
using System;
using System.Collections.Generic;
using Vec2 = System.Numerics.Vector2;

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
    MainScreen,
    StorageScreen
}

public class GameMain : Game
{
    GraphicsDeviceManager graphics;

    SpriteBatch spriteBatch;

    public OrthographicCamera Camera { get; private set; }

    public Player Player { get; set; }

    public NonPlayer NPC { get; set; }

    public ImGuiRenderer GuiRenderer { get; private set; }

    public event Action ImGuiRenderRequested;

    public CollisionDetector CollisionDetector { get; private set; }


    readonly Dictionary<ScreenName, GameScreen> screens = [];
    readonly ScreenManager screenManager;
    ScreenName currentScreen;


    bool enableCollisionDetection = true;
    bool enableDebugRect = true;


    static public List<Tuple<RectangleF, Color>> DebugRects = [];

    public GameMain()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        screenManager = new ScreenManager();
        Components.Add(screenManager);

        Player = new Player(this);

        NPC = new NonPlayer(this, waypoints:
        [
            new(80, 100),
            new(200, 100),
            new(200, 200),
            new(80, 200)
        ]);
    }

    protected override void Initialize()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 320, 180);
        Camera = new OrthographicCamera(viewportAdapter);

        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = 1600;
        graphics.PreferredBackBufferHeight = 900;
        graphics.ApplyChanges();

        screens.Add(ScreenName.MainScreen, new MainScreen(this, spriteBatch));
        screens.Add(ScreenName.StorageScreen, new StorageScreen(this, spriteBatch));

        GuiRenderer = new ImGuiRenderer(this);
        GuiRenderer.RebuildFontAtlas();

        ImGuiRenderRequested += RegisterImGuiHandlers;

        base.Initialize();
    }

    protected override void LoadContent()
    {       
        LoadScreen(ScreenName.MainScreen);

        Player.LoadContent();
        NPC.LoadContent();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        // Event-Handler entfernen
        ImGuiRenderRequested -= RegisterImGuiHandlers;
    }

    private void RegisterImGuiHandlers()
    {
        ImGui.Begin("Visual Debugging");
        ImGui.Checkbox("Show Debug Rects", ref enableDebugRect);
        ImGui.Text($"Player Position: {Player.Position.X} {Player.Position.Y}");
        ImGui.End();
    }

    public void LoadScreen(ScreenName screen, EventHandler onStateChanged = null)
    {
        var transition = new FadeTransition(GraphicsDevice, Color.CornflowerBlue, 0.5f);
        if (onStateChanged != null)
        {
            transition.StateChanged += onStateChanged;
        }

        screenManager.LoadScreen(screens[screen], transition);
        currentScreen = screen;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        DebugRects.Clear();

        Vector2 delta = Player.Position - Camera.Position - new Vector2(152, 82);
        Camera.Position += (delta * 0.08f);

        Camera.Position = new Vector2(
            MathHelper.Clamp(Camera.Position.X, -20, 20),
            MathHelper.Clamp(Camera.Position.Y, -20, 160));

        Camera.Position = Vector2.Round(Camera.Position * 5) / 5.0f;
              
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Window.Title = $"{currentScreen}";
        GraphicsDevice.Clear(Color.CornflowerBlue);

        float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Zeichnet alle Komponenten / Screens
        base.Draw(gameTime);

        spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
        {
            if (enableDebugRect)
                DebugRects.ForEach(debugRect => spriteBatch.DrawRectangle(debugRect.Item1, debugRect.Item2, thickness: .2f));
        }
        spriteBatch.End();

        GuiRenderer.BeginLayout(gameTime);
        {
            // Lokales ImGUI Overlay
            DrawImGuiOverlay(frameRate);

            // Registrierte ImGUI Overlays
            ImGuiRenderRequested?.Invoke();
        }
        GuiRenderer.EndLayout();
    }

    #region "ImGUI Overlay"

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

    #endregion
}
