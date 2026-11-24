using System.Linq;
namespace TacticalArena {
    public static class EnemyAI {
        // Выбирает слабую доступную цель; иначе подходит по кратчайшему маршруту BFS.
        public static bool Step(Battle b, Unit u) {
            if(!b.EnemyTurn || !b.Active(u) || u.Acted) return false;
            var foes=b.Units.Where(x=>x.Alive && !x.Enemy).ToArray();
            if(foes.Length==0) return false;
            var target=foes.Where(x=>b.CanAttack(u,x,false)).OrderBy(x=>x.HP).ThenBy(x=>x.Id).FirstOrDefault();
            if(target!=null) return b.Attack(u,target,u.Cooldown==0);
            if(!u.Moved) {
                var reach=b.Reachable(u,u.Speed);
                // Расстояние до достижимой позиции атаки по маршруту, с учётом препятствий.
                var all=b.Reachable(u,Battle.Size*Battle.Size);
                var attackCells=all.Where(p=>foes.Any(f=>p.Key.Distance(f.Pos)<=u.Range)).OrderBy(p=>p.Value).ThenBy(p=>p.Key.X).ThenBy(p=>p.Key.Y).ToArray();
                if(attackCells.Length>0) {
                    var goal=attackCells[0].Key;
                    // Восстановление пути от цели по убывающим расстояниям.
                    while(all[goal]>u.Speed) { int d=all[goal]-1; goal=all.Where(p=>p.Value==d && p.Key.Distance(goal)==1).OrderBy(p=>p.Key.X).ThenBy(p=>p.Key.Y).First().Key; }
                    if(reach.ContainsKey(goal)) return b.Move(u,goal);
                }
                if(reach.Count>0) { var best=reach.Keys.OrderBy(p=>foes.Min(f=>p.Distance(f.Pos))).ThenBy(p=>p.X).ThenBy(p=>p.Y).First(); return b.Move(u,best); }
            }
            return false;
        }
    }
}
