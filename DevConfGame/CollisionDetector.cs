using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevConfGame;

public class CollisionDetector(TiledMap tiledMap)
{
    public bool CollisionCheck(TiledMapTileLayer layer, Vector2 position, Direction direction)
    {
        var tilePositions = GetRelevantTiles(position, direction);

        foreach (var tilePos in tilePositions)
        {
            if (CollisionDetected(layer, tilePos, position))
            {
                return true;
            }
        }

        return false;
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

    private bool CollisionDetected(TiledMapTileLayer layer, Point tilePos, Vector2 playerPos)
    {
        var tw = tiledMap.TileWidth;
        var th = tiledMap.TileHeight;

        TiledMapTile? collisionTile = null;

        bool found = layer.TryGetTile((ushort)tilePos.X, (ushort)tilePos.Y, out collisionTile);

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
                    Game1.DebugRects.Add(new Tuple<RectangleF, Color>(globalRect, Color.Red));
                }
                else
                {
                    Game1.DebugRects.Add(new Tuple<RectangleF, Color>(globalRect, Color.Green));
                }

                return collision;
            }
        }

        return false;
    }
}
