using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace DevConfGame.Screens;

public class MainScreen(Game game, SpriteBatch spriteBatch) : GameScreen(game)
{
    TiledMap tiledMap;
    TiledMapRenderer tiledMapRenderer;
    TiledMapTileLayer floorLayer;
    TiledMapTileLayer decorationLayer;
    TiledMapTileLayer foregroundLayer;

    bool enableCollisionDetection = true;
    bool enableForegroundLayer = true;
    bool enableFloorLayer = true;
    bool enableDecorationLayer = true;

    CollisionDetector collisionDetector;

    new GameMain Game => (GameMain)base.Game;

    public override void LoadContent()
    {      
        tiledMap = Content.Load<TiledMap>("Maps/MainScreen");
        tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, tiledMap);

        floorLayer = tiledMap.GetLayer<TiledMapTileLayer>("Floor");
        decorationLayer = tiledMap.GetLayer<TiledMapTileLayer>("Decoration");
        foregroundLayer = tiledMap.GetLayer<TiledMapTileLayer>("Foreground");

        collisionDetector = new CollisionDetector(tiledMap);

        Game.ImGuiRenderRequested += RegisterImGuiHandlers;
    }
    public override void UnloadContent()
    {
        base.UnloadContent();

        // Event-Handler entfernen
        Game.ImGuiRenderRequested -= RegisterImGuiHandlers;
    }


    public override void Update(GameTime gameTime)
    {
        // Backup Position
        var playerPos = Game.Player.Position;

        Game.Player.Update(gameTime);
        tiledMapRenderer.Update(gameTime);

        var collisionTile = collisionDetector.CollisionCheck(floorLayer, Game.Player.Position, Game.Player.Direction);
        // var collisionLockerDoor = collisionDetector.CollisionCheck(decorationLayer, Game.Player.Position, Game.Player.Direction, "LockerDoor");

        //        if (enableCollisionDetection && (collisionTile != null || collisionLockerDoor != null))
        if (enableCollisionDetection && collisionTile)
        {
            // Revert Position
            Game.Player.SetX(playerPos.X);//
            Game.Player.SetY(playerPos.Y);
        }

        //if (collisionLockerDoor != null)
        //{
        //    Game.LoadScreen(ScreenName.Locker, (sender, e) =>
        //    {
        //        Game.Player.SetX(80);
        //        Game.Player.SetY(90);
        //    });
        //}
    }

    public override void Draw(GameTime gameTime)
    {
        if (enableFloorLayer)
            tiledMapRenderer.Draw(floorLayer, Game.Camera.GetViewMatrix());

        if (enableDecorationLayer)
            tiledMapRenderer.Draw(decorationLayer, Game.Camera.GetViewMatrix());

        spriteBatch.Begin(transformMatrix: Game.Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

        Game.Player.Draw(gameTime, spriteBatch);

        spriteBatch.End();

        if (enableForegroundLayer)
            tiledMapRenderer.Draw(foregroundLayer, Game.Camera.GetViewMatrix());
    }

    #region "ImGUI Overlay"

    private void RegisterImGuiHandlers()
    {
        ImGui.Begin("Collision Detection");
        ImGui.Checkbox("Enabled", ref enableCollisionDetection);
        ImGui.End();

        ImGui.Begin("Tilemap");
        ImGui.Checkbox("Show Floor Layer", ref enableFloorLayer);
        ImGui.Checkbox("Show Decoration Layer", ref enableDecorationLayer);
        ImGui.Checkbox("Show Foreground Layer", ref enableForegroundLayer);
        ImGui.End();
    }

    #endregion
}
