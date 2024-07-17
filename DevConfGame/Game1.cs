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
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

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

    private AnimatedSprite sprite;

    private OrthographicCamera camera;
    Player player;
    TiledMap tiledMap;
    TiledMapRenderer tiledMapRenderer;
    TiledMapTileLayer floorLayer;
    TiledMapTileLayer decorationLayer;
    TiledMapTileLayer foregroundLayer;


    public static ImGuiRenderer guiRenderer;

    bool enableCollisionDetection = true;
    bool enableDebugRect = true;
    bool enableForegroundLayer = true;
    bool enableDecorationLayer = true;
    bool enableFloorLayer = true;
    List<Tuple<RectangleF, Color>> debugRects = [];

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

        tiledMap = Content.Load<TiledMap>("Maps/Map");
        tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, tiledMap);
        floorLayer = tiledMap.GetLayer<TiledMapTileLayer>("Floor");
        decorationLayer = tiledMap.GetLayer<TiledMapTileLayer>("Decoration");
        foregroundLayer = tiledMap.GetLayer<TiledMapTileLayer>("Foreground");

        var spriteTexture = Content.Load<Texture2D>("Player/SpriteSheet");
        var spriteAtlas = TextureAtlas.Create("spriteAtlas", spriteTexture, 16, 16);
        var spriteSheet = new SpriteSheet { TextureAtlas = spriteAtlas };

        AddAnimationCycle(spriteSheet, "walkLeft", [0, 4, 0, 8]);
        AddAnimationCycle(spriteSheet, "walkRight", [3, 7, 3, 11]);
        AddAnimationCycle(spriteSheet, "walkUp", [2, 6, 2, 10]);
        AddAnimationCycle(spriteSheet, "walkDown", [1, 5, 1, 9]);
        AddAnimationCycle(spriteSheet, "idleDown", [1, 12, 13, 14, 14, 13, 12, 1], frameDuration: .08f);

        sprite = new AnimatedSprite(spriteSheet, "idleDown")
        {
            OriginNormalized = new Vector2(0, 0)
        };

        player = new Player(sprite);

        guiRenderer.RebuildFontAtlas();

    }

    private void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = .15f)
    {
        var cycle = new SpriteSheetAnimationCycle();

        foreach (var f in frames)
        {
            cycle.Frames.Add(new SpriteSheetAnimationFrame(f, frameDuration));
        }

        cycle.IsLooping = isLooping;
        cycle.FrameDuration = frameDuration;
        spriteSheet.Cycles.Add(name, cycle);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        debugRects.Clear();

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

        camera.Position = new Vector2(
            MathHelper.Clamp(camera.Position.X, -20, 20),
            MathHelper.Clamp(camera.Position.Y, -20, 160));

        var transformationMatrix = camera.GetViewMatrix();

        if (enableFloorLayer)
            tiledMapRenderer.Draw(floorLayer, viewMatrix: transformationMatrix);

        if (enableDecorationLayer)
            tiledMapRenderer.Draw(decorationLayer, viewMatrix: transformationMatrix);

        camera.Position = oldCamPosition;

        spriteBatch.Begin(transformMatrix: transformationMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(sprite, player.Position);

        debugRects.Add(new Tuple<RectangleF, Color>(new RectangleF(player.Position.X + 2, player.Position.Y + 12, 12, 4), Color.Red));

        foreach (var debugRect in debugRects)
        {
            DrawDebugRect(spriteBatch, debugRect.Item1, 1, debugRect.Item2);
        }

        spriteBatch.End();

        base.Draw(gameTime);

        if (enableForegroundLayer)
            tiledMapRenderer.Draw(foregroundLayer, viewMatrix: transformationMatrix);

        guiRenderer.BeginLayout(gameTime);
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

        batch.DrawRectangle(rect, color, .2f);
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

                var playerRect = new RectangleF(playerPos.X + 2, playerPos.Y + 12, 12, 4);

                var collision = globalRect.Intersects(playerRect);

                if (collision)
                {
                    debugRects.Add(new Tuple<RectangleF, Color>(globalRect, Color.Red));
                }
                else
                {
                    debugRects.Add(new Tuple<RectangleF, Color>(globalRect, Color.Green));
                }

                return collision;
            }
        }

        return false;
    }
}
