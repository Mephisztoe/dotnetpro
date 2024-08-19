using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevConfGame;

public class NonPlayer(Game game, List<Vector2> waypoints) :
    GameCharacter(game, new Vector2(100, 100), 85)
{
    private int currentWaypointIndex = 0;
    private float waypointRadius = 5f; // Radius um einen Wegpunkt, in dem er als erreicht gilt

    public override void LoadContent()
    {
        var spriteTexture = Game.Content.Load<Texture2D>("Player/SpriteSheet");
        var spriteAtlas = Texture2DAtlas.Create("spriteAtlas", spriteTexture, 16, 16);
        var spriteSheet = new SpriteSheet("spriteSheet", spriteAtlas);

        AddAnimationCycle(spriteSheet, "walkLeft", [0, 4, 0, 8]);
        AddAnimationCycle(spriteSheet, "walkRight", [3, 7, 3, 11]);
        AddAnimationCycle(spriteSheet, "walkUp", [2, 6, 2, 10]);
        AddAnimationCycle(spriteSheet, "walkDown", [1, 5, 1, 9]);
        AddAnimationCycle(spriteSheet, "idleDown", [1, 12, 13, 14, 14, 13, 12, 1], frameDuration: .08f);

        sprite = new AnimatedSprite(spriteSheet, "idleDown")
        {
            OriginNormalized = new Vector2(0, 0)
        };
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Prüfe, ob der aktuelle Wegpunkt erreicht wurde
        if (Vector2.Distance(position, waypoints[currentWaypointIndex]) < waypointRadius)
        {
            // Gehe zum nächsten Wegpunkt
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        // Bestimme die Richtung zum nächsten Wegpunkt
        Vector2 directionToWaypoint = waypoints[currentWaypointIndex] - position;

        // Bestimme die Bewegungsrichtung basierend auf der dominanten Komponente
        if (Math.Abs(directionToWaypoint.X) > Math.Abs(directionToWaypoint.Y))
        {
            // Bewege horizontal
            if (directionToWaypoint.X < 0)
            {
                direction = Direction.Left;
                position.X -= speed * dt;
            }
            else if (directionToWaypoint.X > 0)
            {
                direction = Direction.Right;
                position.X += speed * dt;
            }
        }
        else
        {
            // Bewege vertikal
            if (directionToWaypoint.Y < 0)
            {
                direction = Direction.Up;
                position.Y -= speed * dt;
            }
            else if (directionToWaypoint.Y > 0)
            {
                direction = Direction.Down;
                position.Y += speed * dt;
            }
        }

        var animation = direction switch
        {
            Direction.Up => "walkUp",
            Direction.Down => "walkDown",
            Direction.Left => "walkLeft",
            Direction.Right => "walkRight",
            _ => sprite.CurrentAnimation // falls direction nicht passt, bleibt die aktuelle Animation
        };

        if (!sprite.CurrentAnimation.Equals(animation))
        {
            sprite.SetAnimation(animation);
        }


        // Wenn keine Bewegung stattfindet, spiele die Idle-Animation ab
        if (directionToWaypoint.LengthSquared() == 0)
        {
            if (!sprite.CurrentAnimation.Equals("idleDown"))
                sprite.SetAnimation("idleDown");
        }

        position = Vector2.Round(position * 5) / 5.0f;

        sprite.Update(gameTime);
    }
}
