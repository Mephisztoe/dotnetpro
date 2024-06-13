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
    TiledMapTileLayer floorLayer;
    TiledMapTileLayer decorationLayer;
    private Texture2D walkUp;
    private Texture2D walkDown;
    private Texture2D walkLeft;
    private Texture2D walkRight;
    private Texture2D idleDown;
    public static ImGuiRenderer guiRenderer;

    bool enableCollisionDetection = true;
    bool enableDebugRect = true;

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
        floorLayer = tiledMap.GetLayer<TiledMapTileLayer>("Floor");
        decorationLayer = tiledMap.GetLayer<TiledMapTileLayer>("Decoration");

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

        // Backup Position
        var playerPos = player.Position;

        player.Update(gameTime);
        tiledMapRenderer.Update(gameTime);
       
        bool collision = CollisionCheck(player.Position, player.Direction);

        if (enableCollisionDetection && collision)
        {
            // Revert Position
            player.SetX(playerPos.X);
            player.SetY(playerPos.Y);
        }

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

        DrawDebugRect(spriteBatch, new RectangleF(player.Position.X, player.Position.Y, 16, 16), 1, Color.Red);
        
        spriteBatch.End();

        base.Draw(gameTime);

        guiRenderer.BeginLayout(gameTime);
        DrawImGuiOverlay(frameRate);
        ImGui.Begin("Collision Details");
        ImGui.Checkbox("Debug Rect", ref enableDebugRect);
        ImGui.Checkbox("Collision Detection", ref enableCollisionDetection);
        ImGui.End();
        guiRenderer.EndLayout();
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

        int x = (int)Math.Round(rect.Left);
        int y = (int)Math.Round(rect.Top);
        int w = (int)rect.Width;
        int h = (int)rect.Height;

        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new Color[] { color });

        batch.Draw(texture, new Rectangle(x, y, w, border), Color.White); // Top      
        batch.Draw(texture, new Rectangle(x + w - border, y, border, h), Color.White); // Right   
        batch.Draw(texture, new Rectangle(x, y + h - border, w, border), Color.White); // Bottom      
        batch.Draw(texture, new Rectangle(x, y, border, h), Color.White); // Left
    }

    private List<Point> GetRelevantTiles(Vector2 position, Direction direction)
    {
        var tw = tiledMap.TileWidth;
        var th = tiledMap.TileHeight;

        var baseTileX = (ushort)((position.X + 8) / tw);
        var baseTileY = (ushort)((position.Y + 8) / th);

        var tiles = new List<Point> { new(baseTileX, baseTileY) };

        switch (direction)
        {
            case Direction.Left:
                tiles.Add(new Point(baseTileX - 1, baseTileY));
                tiles.Add(new Point(baseTileX - 1, baseTileY - 1));
                tiles.Add(new Point(baseTileX - 1, baseTileY + 1));
                break;
            case Direction.Right:
                tiles.Add(new Point(baseTileX + 1, baseTileY));
                tiles.Add(new Point(baseTileX + 1, baseTileY - 1));
                tiles.Add(new Point(baseTileX + 1, baseTileY + 1));
                break;
            case Direction.Up:
                tiles.Add(new Point(baseTileX, baseTileY - 1));
                tiles.Add(new Point(baseTileX - 1, baseTileY - 1));
                tiles.Add(new Point(baseTileX + 1, baseTileY - 1));
                break;
            case Direction.Down:
                tiles.Add(new Point(baseTileX, baseTileY + 1));
                tiles.Add(new Point(baseTileX - 1, baseTileY + 1));
                tiles.Add(new Point(baseTileX + 1, baseTileY + 1));
                break;
        }

        return tiles;
    }

    private bool CollisionCheck(Vector2 position, Direction direction)
    {
        var tilePositions = GetRelevantTiles(position, direction);

        foreach (var tilePos in tilePositions)
        {            
            if (CollisionDetected(tilePos, position))
            {
                return true;
            }
        }

        return false;
    }

    private bool CollisionDetected(Point tilePos, Vector2 playerPos)
    {
        var tw = tiledMap.TileWidth;
        var th = tiledMap.TileHeight;

        TiledMapTile? collisionTile = null;

        bool found = floorLayer.TryGetTile((ushort)tilePos.X, (ushort)tilePos.Y, out collisionTile);

        if (found && !collisionTile.Value.IsBlank)
        {
            var tileset = tiledMap.GetTilesetByTileGlobalIdentifier(collisionTile.Value.GlobalIdentifier);
            var firstGlobalIdentifier = tiledMap.GetTilesetFirstGlobalIdentifier(tileset);
            var localTileIdentifier = collisionTile.Value.GlobalIdentifier - firstGlobalIdentifier;

            var tilesetTile = tileset.Tiles.FirstOrDefault(x => x.LocalTileIdentifier == localTileIdentifier);
            if (tilesetTile != null && tilesetTile.Objects.Count != 0)
            {
                var localRect = new RectangleF(tilesetTile.Objects[0].Position.X, tilesetTile.Objects[0].Position.Y,
                                               tilesetTile.Objects[0].Size.Width, tilesetTile.Objects[0].Size.Height);

                var globalRect = new RectangleF(tilePos.X * tw + localRect.X, tilePos.Y * th + localRect.Y,
                                                localRect.Width, localRect.Height);                              

                var playerRect = new RectangleF(playerPos.X, playerPos.Y, 16, 16);

                var collision = globalRect.Intersects(playerRect);

                return collision;
            }
        }

        return false;
    }
}
