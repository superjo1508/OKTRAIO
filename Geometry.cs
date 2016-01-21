using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using RectangleF = SharpDX.RectangleF;
//ReSharper disable InconsistentNaming
//ReSharper disable CompareOfFloatsByEqualityOperator

namespace OKTRAIO
{
    public class OKTRGeometry
    {
        public struct OptimizedLocation
        {
            public int ChampsHit;
            public Vector2 Position;

            public OptimizedLocation(Vector2 position, int champsHit)
            {
                Position = position;
                ChampsHit = champsHit;
            }
        }

        /// <summary>
        /// Uses MEC to get the perfect position on Circle Skillshots
        /// </summary>
        /// <param name="spell">Give it a spell and it will do the rest of the logic for you</param>
        /// <param name="targetHero">If you give it a target it will look around that target for other targets but will always focus that target</param>
        /// <returns></returns>
        internal static OptimizedLocation? GetOptimizedCircleLocation(Spell.Skillshot spell,
            AIHeroClient targetHero = null)
        {
            if (targetHero != null)
            {
                if (!targetHero.IsValidTarget(spell.Range + spell.Radius))
                    return null;

                var champs =
                    EntityManager.Heroes.Enemies.Where(e => e.Distance(targetHero) < spell.Radius)
                        .Select(
                            champ =>
                                Prediction.Position.PredictUnitPosition(champ,
                                    ((int) Player.Instance.Distance(champ)/spell.Speed) + spell.CastDelay))
                        .ToList();
                return GetOptimizedCircleLocation(champs, spell.Width, spell.Range);
            }
            if (EntityManager.Heroes.Enemies.Any(e => e.Distance(Player.Instance) < spell.Radius + spell.Range))
            {
                var champs =
                    EntityManager.Heroes.Enemies.Where(e => e.Distance(Player.Instance) < spell.Radius + spell.Range)
                        .Select(
                            champ =>
                                Prediction.Position.PredictUnitPosition(champ,
                                    ((int) Player.Instance.Distance(champ)/spell.Speed) + spell.CastDelay)).ToList();

                return GetOptimizedCircleLocation(champs, spell.Width, spell.Range);
            }
            return null;
        }

        /// <summary>
        /// Uses MEC to get the perfect position on Circle Skillshots
        /// </summary>
        /// <param name="targetPositions">Vector2's to target. Example could be all minions inside a range</param>
        /// <param name="spellWidth">Width of spell (Radius*2)</param>
        /// <param name="spellRange">Range of spell</param>
        /// <param name="useMECMax">Just leave this value at default if you don't know what you're doing.</param>
        /// <returns></returns>
        public static OptimizedLocation GetOptimizedCircleLocation(List<Vector2> targetPositions,
            float spellWidth,
            float spellRange,
            int useMECMax = 9)
        {
            var result = new Vector2();
            var targetsHit = 0;
            var startPos = Player.Instance.ServerPosition.To2D();

            spellRange = spellRange*spellRange;

            if (targetPositions.Count == 0)
            {
                return new OptimizedLocation(result, targetsHit);
            }

            if (targetPositions.Count <= useMECMax)
            {
                var subGroups = GetCombinations(targetPositions);
                foreach (var subGroup in subGroups)
                {
                    if (subGroup.Count > 0)
                    {
                        var circle = MEC.GetMec(subGroup);

                        if (circle.Radius <= spellWidth && circle.Center.Distance(startPos, true) <= spellRange)
                        {
                            targetsHit = subGroup.Count;
                            return new OptimizedLocation(circle.Center, targetsHit);
                        }
                    }
                }
            }
            else
            {
                foreach (var pos in targetPositions)
                {
                    if (pos.Distance(startPos, true) <= spellRange)
                    {
                        var count = targetPositions.Count(pos2 => pos.Distance(pos2, true) <= spellWidth*spellWidth);

                        if (count >= targetsHit)
                        {
                            result = pos;
                            targetsHit = count;
                        }
                    }
                }
            }

            return new OptimizedLocation(result, targetsHit);
        }

        private static IEnumerable<List<Vector2>> GetCombinations(IReadOnlyCollection<Vector2> allValues)
        {
            var collection = new List<List<Vector2>>();
            for (var counter = 0; counter < (1 << allValues.Count); ++counter)
            {
                var combination = allValues.Where((t, i) => (counter & (1 << i)) == 0).ToList();

                collection.Add(combination);
            }
            return collection;
        }

        /// <summary>
        /// Provides method to calculate the minimum enclosing circle.
        /// </summary>
        public static class MEC
        {
            public static Vector2[] g_MinMaxCorners;
            public static RectangleF g_MinMaxBox;
            public static Vector2[] g_NonCulledPoints;

            public static MecCircle GetMec(List<Vector2> points)
            {
                Vector2 center;
                float radius;

                var ConvexHull = MakeConvexHull(points);
                FindMinimalBoundingCircle(ConvexHull, out center, out radius);
                return new MecCircle(center, radius);
            }

            private static void GetMinMaxCorners(IReadOnlyList<Vector2> points,
                ref Vector2 ul,
                ref Vector2 ur,
                ref Vector2 ll,
                ref Vector2 lr)
            {
                // Start with the first point as the solution.
                ul = points[0];
                ur = ul;
                ll = ul;
                lr = ul;

                // Search the other points.
                foreach (var pt in points)
                {
                    if (-pt.X - pt.Y > -ul.X - ul.Y)
                    {
                        ul = pt;
                    }
                    if (pt.X - pt.Y > ur.X - ur.Y)
                    {
                        ur = pt;
                    }
                    if (-pt.X + pt.Y > -ll.X + ll.Y)
                    {
                        ll = pt;
                    }
                    if (pt.X + pt.Y > lr.X + lr.Y)
                    {
                        lr = pt;
                    }
                }

                g_MinMaxCorners = new[] {ul, ur, lr, ll}; // For debugging.
            }

            // Find a box that fits inside the MinMax quadrilateral.
            private static RectangleF GetMinMaxBox(List<Vector2> points)
            {
                // Find the MinMax quadrilateral.
                Vector2 ul = new Vector2(0, 0), ur = ul, ll = ul, lr = ul;
                GetMinMaxCorners(points, ref ul, ref ur, ref ll, ref lr);

                // Get the coordinates of a box that lies inside this quadrilateral.
                var xmin = ul.X;
                var ymin = ul.Y;

                var xmax = ur.X;
                if (ymin < ur.Y)
                {
                    ymin = ur.Y;
                }

                if (xmax > lr.X)
                {
                    xmax = lr.X;
                }
                var ymax = lr.Y;

                if (xmin < ll.X)
                {
                    xmin = ll.X;
                }
                if (ymax > ll.Y)
                {
                    ymax = ll.Y;
                }

                var result = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
                g_MinMaxBox = result; // For debugging.
                return result;
            }

            private static List<Vector2> HullCull(List<Vector2> points)
            {
                // Find a culling box.
                var culling_box = GetMinMaxBox(points);

                // Cull the points.
                var results =
                    points.Where(
                        pt =>
                            pt.X <= culling_box.Left || pt.X >= culling_box.Right || pt.Y <= culling_box.Top ||
                            pt.Y >= culling_box.Bottom).ToList();

                g_NonCulledPoints = new Vector2[results.Count]; // For debugging.
                results.CopyTo(g_NonCulledPoints); // For debugging.
                return results;
            }

            public static List<Vector2> MakeConvexHull(List<Vector2> points)
            {
                // Cull.
                points = HullCull(points);

                // Find the remaining point with the smallest Y value.
                // if (there's a tie, take the one with the smaller X value.
                Vector2[] best_pt = {points[0]};
                foreach (
                    var pt in
                        points.Where(pt => (pt.Y < best_pt[0].Y) || ((pt.Y == best_pt[0].Y) && (pt.X < best_pt[0].X)))
                    )
                {
                    best_pt[0] = pt;
                }

                // Move this point to the convex hull.
                var hull = new List<Vector2> {best_pt[0]};
                points.Remove(best_pt[0]);

                // Start wrapping up the other points.
                float sweep_angle = 0;
                for (;;)
                {
                    // If all of the points are on the hull, we're done.
                    if (points.Count == 0)
                    {
                        break;
                    }

                    // Find the point with smallest AngleValue
                    // from the last point.
                    var X = hull[hull.Count - 1].X;
                    var Y = hull[hull.Count - 1].Y;
                    best_pt[0] = points[0];
                    float best_angle = 3600;

                    // Search the rest of the points.
                    foreach (var pt in points)
                    {
                        var test_angle = AngleValue(X, Y, pt.X, pt.Y);
                        if ((test_angle >= sweep_angle) && (best_angle > test_angle))
                        {
                            best_angle = test_angle;
                            best_pt[0] = pt;
                        }
                    }

                    // See if the first point is better.
                    // If so, we are done.
                    var first_angle = AngleValue(X, Y, hull[0].X, hull[0].Y);
                    if ((first_angle >= sweep_angle) && (best_angle >= first_angle))
                    {
                        // The first point is better. We're done.
                        break;
                    }

                    // Add the best point to the convex hull.
                    hull.Add(best_pt[0]);
                    points.Remove(best_pt[0]);

                    sweep_angle = best_angle;
                }

                return hull;
            }

            private static float AngleValue(float x1, float y1, float x2, float y2)
            {
                float t;

                var dx = x2 - x1;
                var ax = Math.Abs(dx);
                var dy = y2 - y1;
                var ay = Math.Abs(dy);
                if (ax + ay == 0)
                {
                    // if (the two points are the same, return 360.
                    t = 360f/9f;
                }
                else
                {
                    t = dy/(ax + ay);
                }
                if (dx < 0)
                {
                    t = 2 - t;
                }
                else if (dy < 0)
                {
                    t = 4 + t;
                }
                return t*90;
            }

            public static void FindMinimalBoundingCircle(List<Vector2> points, out Vector2 center, out float radius)
            {
                // Find the convex hull.
                var hull = MakeConvexHull(points);

                // The best solution so far.
                var best_center = points[0];
                var best_radius2 = Single.MaxValue;

                // Look at pairs of hull points.
                for (var i = 0; i < hull.Count - 1; i++)
                {
                    for (var j = i + 1; j < hull.Count; j++)
                    {
                        // Find the circle through these two points.
                        var test_center = new Vector2((hull[i].X + hull[j].X)/2f, (hull[i].Y + hull[j].Y)/2f);
                        var dx = test_center.X - hull[i].X;
                        var dy = test_center.Y - hull[i].Y;
                        var test_radius2 = dx*dx + dy*dy;

                        // See if this circle would be an improvement.
                        if (test_radius2 < best_radius2)
                        {
                            // See if this circle encloses all of the points.
                            if (CircleEnclosesPoints(test_center, test_radius2, points, i, j, -1))
                            {
                                // Save this solution.
                                best_center = test_center;
                                best_radius2 = test_radius2;
                            }
                        }
                    } // for i
                } // for j

                // Look at triples of hull points.
                for (var i = 0; i < hull.Count - 2; i++)
                {
                    for (var j = i + 1; j < hull.Count - 1; j++)
                    {
                        for (var k = j + 1; k < hull.Count; k++)
                        {
                            // Find the circle through these three points.
                            Vector2 test_center;
                            float test_radius2;
                            FindCircle(hull[i], hull[j], hull[k], out test_center, out test_radius2);

                            // See if this circle would be an improvement.
                            if (test_radius2 < best_radius2)
                            {
                                // See if this circle encloses all of the points.
                                if (CircleEnclosesPoints(test_center, test_radius2, points, i, j, k))
                                {
                                    // Save this solution.
                                    best_center = test_center;
                                    best_radius2 = test_radius2;
                                }
                            }
                        } // for k
                    } // for i
                } // for j

                center = best_center;
                if (best_radius2 == Single.MaxValue)
                {
                    radius = 0;
                }
                else
                {
                    radius = (float) Math.Sqrt(best_radius2);
                }
            }

            private static bool CircleEnclosesPoints(Vector2 center,
                float radius2,
                IEnumerable<Vector2> points,
                int skip1,
                int skip2,
                int skip3)
            {
                return (from point in points.Where((t, i) => (i != skip1) && (i != skip2) && (i != skip3))
                    let dx = center.X - point.X
                    let dy = center.Y - point.Y
                    select dx*dx + dy*dy).All(test_radius2 => !(test_radius2 > radius2));
            }

            private static void FindCircle(Vector2 a, Vector2 b, Vector2 c, out Vector2 center, out float radius2)
            {
                // Get the perpendicular bisector of (x1, y1) and (x2, y2).
                var x1 = (b.X + a.X)/2;
                var y1 = (b.Y + a.Y)/2;
                var dy1 = b.X - a.X;
                var dx1 = -(b.Y - a.Y);

                // Get the perpendicular bisector of (x2, y2) and (x3, y3).
                var x2 = (c.X + b.X)/2;
                var y2 = (c.Y + b.Y)/2;
                var dy2 = c.X - b.X;
                var dx2 = -(c.Y - b.Y);

                // See where the lines intersect.
                var cx = (y1*dx1*dx2 + x2*dx1*dy2 - x1*dy1*dx2 - y2*dx1*dx2)/(dx1*dy2 - dy1*dx2);
                var cy = (cx - x1)*dy1/dx1 + y1;
                center = new Vector2(cx, cy);

                var dx = cx - a.X;
                var dy = cy - a.Y;
                radius2 = dx*dx + dy*dy;
            }

            public struct MecCircle
            {
                public Vector2 Center;
                public float Radius;

                public MecCircle(Vector2 center, float radius)
                {
                    Center = center;
                    Radius = radius;
                }
            }
        }

        /// <summary>
        /// Angle Deviation which will output a Vector2 that represents the new Position.
        /// </summary>
        /// <param name="point1">Source location for the deviation</param>
        /// <param name="point2">Target location which will be turned</param>
        /// <param name="angle">The angle that point2 will be turned</param>
        public static Vector2 Deviation(Vector2 point1, Vector2 point2, double angle)
        {
            angle *= Math.PI/180.0;
            var temp = Vector2.Subtract(point2, point1);
            var result = new Vector2(0)
            {
                X = (float) (temp.X*Math.Cos(angle) - temp.Y*Math.Sin(angle))/4,
                Y = (float) (temp.X*Math.Sin(angle) + temp.Y*Math.Cos(angle))/4
            };
            result = Vector2.Add(result, point1);
            return result;
        }

        /// <summary>
        /// Gets a specified amount of Rotated Positions. Useful for getting safe dash positions.
        /// </summary>
        /// <param name="rotateAround">The source position</param>
        /// <param name="rotateTowards">The target position</param>
        /// <param name="degrees">It will get rotated positions from -degrees to +degrees</param>
        /// <param name="positionAmount">The amount of positions to get. Can be left blank</param>
        /// <param name="distance">Can be left blan</param>
        /// <returns></returns>
        public static List<Vector3> RotatedPositions(Vector3 rotateAround, Vector3 rotateTowards, int degrees,
            int positionAmount = 0, float distance = 0)
        {
            if (distance == 0) distance = rotateAround.Distance(rotateTowards);
            if (positionAmount == 0) positionAmount = degrees/10;
            var realRotateTowards = rotateAround.Extend(rotateTowards, distance);
            var posList = new List<Vector3>();
            var step = (degrees*2)/positionAmount;
            for (var i = -degrees; i <= degrees; i += step)
            {
                var rotatedPosition = OKTRGeometry.Deviation(rotateAround.To2D(), realRotateTowards, i);
                posList.Add(rotatedPosition.To3D());
            }
            return posList;
        }


        public static Vector3 SafeDashLogic(float range)
        {
            if (Variables.CloseEnemies().Count <= 1)
            {
                var dashPos = (Player.Instance.ServerPosition.To2D() + range * Player.Instance.Direction.To2D()).To3D();
                if (!dashPos.IsUnderTurret() && (!Variables.JinxTrap(dashPos) || !Variables.CaitTrap(dashPos)))
                {
                    return dashPos;
                }
            }
            if (Variables.CloseAllies().Count == 0 &&
                Variables.CloseEnemies(Player.Instance.AttackRange + range * 1.1f).Count <= 2)
            {
                if (Variables.CloseEnemies(Player.Instance.AttackRange + range * 1.1f).Any(
                    t => t.Health + 15 <
                         Player.Instance.GetAutoAttackDamage(t) + Player.Instance.GetSpellDamage(t, SpellSlot.Q)
                         && t.Distance(Player.Instance) < Player.Instance.AttackRange + 80f))
                {
                    Chat.Print("1 high 1 low");
                    var dashPos =
                        Player.Instance.ServerPosition.Extend(
                            Variables.CloseEnemies()
                                .Where(e => e.HealthPercent > 10)
                                .OrderBy(t => t.Health)
                                .First()
                                .ServerPosition, range);

                    if (!dashPos.IsUnderTurret() && (!Variables.JinxTrap(dashPos.To3D()) || !Variables.CaitTrap(dashPos.To3D())))
                    {
                        return dashPos.To3D();
                    }
                }
            }
            if (Variables.CloseAllies().Count == 0 &&
                Variables.CloseEnemies(Player.Instance.AttackRange + range * 1.1f).Count(e => e.HealthPercent > 10) <=
                2)
            {
                Chat.Print("2 high");
                var dashPos = (Player.Instance.ServerPosition.To2D() + range * Player.Instance.Direction.To2D()).To3D();
                if (!dashPos.IsUnderTurret() && (!Variables.JinxTrap(dashPos) || !Variables.CaitTrap(dashPos)))
                {
                    return dashPos;
                }
            }
            var closestEnemy =
                Variables.CloseEnemies()
                    .OrderBy(e => e.Distance(Player.Instance))
                    .FirstOrDefault(e => e.Distance(Player.Instance) < e.AttackRange);
            if (closestEnemy != null
                && !closestEnemy.IsMelee)
            {
                if (SafeCheckTwo((Player.Instance.ServerPosition.Extend(Game.CursorPos, range).To3D()))) 
                    return Player.Instance.ServerPosition.Extend(Game.CursorPos, range).To3D();
            }
            var bestWeight = 0f;
            var bestPos = Player.Instance.ServerPosition.Extend(Game.CursorPos, range).To3D();
            foreach (
                var pos in
                    OKTRGeometry.RotatedPositions(Player.Instance.ServerPosition, Game.CursorPos, 20, 0, range)
                        .Where(p => !EnemyPoints().Contains(p.To2D())))
            {
                if (pos.CountEnemiesInRange(1150) == 0 ||
                    Variables.CloseEnemies().FirstOrDefault(e => e.Distance(pos) < 1150) == null)
                    continue;
                var enemies =
                    EntityManager.Heroes.Enemies.Where(
                        e =>
                            e.IsValidTarget(1200, true, pos) &&
                            e.TotalShieldHealth() > Variables.GetChampionDamage(e)).ToList();
                var enemiesEx =
                    EntityManager.Heroes.Enemies.Where(en => en.IsValidTarget(1200f, true, pos)).ToList();
                var totalDistance = (enemiesEx.Count() - enemies.Count() > 1 && enemiesEx.Count() > 2)
                    ? enemiesEx.Sum(en => en.Distance(Player.Instance.ServerPosition))
                    : enemies.Sum(en => en.Distance(Player.Instance.ServerPosition));
                var avg = totalDistance / pos.CountEnemiesInRange(1150);
                var closest =
                    Player.Instance.ServerPosition.Distance(
                        Variables.CloseEnemies().FirstOrDefault(e => e.Distance(pos) < 1150));
                var weightedAvg = closest * .4f + avg * .6f;
                if (!(weightedAvg > bestWeight) || !SafeCheckTwo(pos)) continue;
                bestWeight = weightedAvg;
                bestPos = pos;
            }
            var finalDash = (SafeCheck(bestPos) ? bestPos : Vector3.Zero);
            if (finalDash == Vector3.Zero)
            {
                if (Variables.CloseAllies().Any() && Variables.CloseEnemies().Any())
                {
                    var safestAlly =
                        Variables.CloseAllies()
                            .OrderBy(a => a.Distance(Player.Instance.ServerPosition))
                            .ThenByDescending(a => a.Health).FirstOrDefault();
                    if (safestAlly != null &&
                        safestAlly.Distance(
                            Variables.CloseEnemies().OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault()) >
                        Player.Instance.Distance(
                            Variables.CloseEnemies().OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault()))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var dashPos = Player.Instance.ServerPosition.Extend(safestAlly.ServerPosition,
                            range).To3D();
                        if (SafeCheckTwo(dashPos)) finalDash = dashPos;
                    }
                }
            }
            if (finalDash != Vector3.Zero) return finalDash;
            if (SafeCheck(Player.Instance.ServerPosition.Extend(Game.CursorPos, range).To3D()))
            {
                finalDash = Player.Instance.ServerPosition.Extend(Game.CursorPos, range).To3D();
            }
            return finalDash;
        }


        public static bool SafeCheck(Vector3 pos)
        {
            return SafeCheckTwo(pos)
                   && SafeCheckThree(pos)
                   && EntityManager.Heroes.Enemies.All(e => e.Distance(pos) > 350f)
                   && (!pos.IsUnderTurret() && (!Variables.JinxTrap(pos) || !Variables.CaitTrap(pos))) || Player.Instance.IsUnderTurret() && pos.IsUnderTurret() && Player.Instance.HealthPercent > 10 && (!Variables.JinxTrap(pos) || !Variables.CaitTrap(pos));
        }

        public static bool SafeCheckTwo(Vector3 pos)
        {
            return (!pos.IsUnderTurret() && (!Variables.JinxTrap(pos) || !Variables.CaitTrap(pos)) || Player.Instance.IsUnderTurret()) && (!Variables.JinxTrap(pos) || !Variables.CaitTrap(pos)) &&
                   Variables.CloseAllies(1000).Count(a => a.HealthPercent > 10) +
                   EntityManager.Turrets.Allies.Count(t => t.Distance(Player.Instance) < 1000) * 2 >=
                   Variables.CloseEnemies(1000).Count(e => e.HealthPercent > 10) +
                   (Player.Instance.IsUnderTurret()
                       ? EntityManager.Turrets.Enemies.Count(t => t.Distance(Player.Instance) < 1000) * 2
                       : 0);
        }

        public static bool SafeCheckThree(Vector3 pos)
        {
            return (!EnemyPoints().Contains(pos.To2D()) ||
                    EnemyPoints().Contains(Player.Instance.ServerPosition.To2D())) &&
                   !EntityManager.Heroes.Enemies.FindAll(
                       e =>
                           e.IsValidTarget(1500) &&
                           !(e.Distance(Player.Instance.ServerPosition) < e.GetAutoAttackRange() + 65))
                       .All(e => pos.CountEnemiesInRange(e.GetAutoAttackRange()) <= 1);
        }

        public static List<Vector2> EnemyPoints()
        {
            return Geometry.ClipPolygons(EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(1500))
                .Select(
                    e =>
                        new Geometry.Polygon.Circle(e.ServerPosition,
                            (e.IsMelee ? e.AttackRange * 1.5f : e.AttackRange) +
                            e.BoundingRadius + 20))
                .ToList()).SelectMany(path => path, (path, point) => new Vector2(point.X, point.Y))
                .Where(point => !point.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall))
                .ToList();
        }
    }
}