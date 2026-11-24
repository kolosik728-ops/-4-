using System;
using System.Collections.Generic;
using System.Linq;

namespace TacticalArena {
    public enum Role { Warrior, Archer, Mage }
    public enum Outcome { Playing, Victory, Defeat, Draw }
    public struct Cell : IEquatable<Cell> {
        public readonly int X, Y;
        public Cell(int x, int y) { X=x; Y=y; }
        public bool Equals(Cell p) => X==p.X && Y==p.Y;
        public override bool Equals(object o) => o is Cell p && Equals(p);
        public override int GetHashCode() => X*397 ^ Y;
        public int Distance(Cell p) => Math.Abs(X-p.X)+Math.Abs(Y-p.Y);
    }
    public sealed class Unit {
        public readonly int Id; public readonly bool Enemy; public readonly Role Role;
        public Cell Pos; public int HP; public bool Moved, Acted; public int Cooldown;
        public int MaxHP => Role==Role.Warrior ? 100 : Role==Role.Archer ? 70 : 60;
        public int Speed => Role==Role.Mage ? 2 : 3;
        public int Range => Role==Role.Warrior ? 1 : Role==Role.Archer ? 4 : 3;
        public int Damage => Role==Role.Warrior ? 25 : Role==Role.Archer ? 19 : 21;
        public string Name => Role==Role.Warrior ? "Воин" : Role==Role.Archer ? "Лучник" : "Маг";
        public string Skill => Role==Role.Warrior ? "Мощный удар" : Role==Role.Archer ? "Двойной выстрел" : "Огненный круг";
        public bool Alive => HP>0;
        public Unit(int id, bool enemy, Role role, Cell pos) { Id=id; Enemy=enemy; Role=role; Pos=pos; HP=MaxHP; }
    }
    public sealed class Battle {
        public const int Size=10, RoundLimit=50;
        public readonly List<Unit> Units=new List<Unit>();
        public readonly HashSet<Cell> Rocks=new HashSet<Cell>();
        public readonly List<string> Log=new List<string>();
        public int Round {get;private set;}=1;
        public bool EnemyTurn {get;private set;}
        public Outcome Result {get;private set;}
        public Battle() {
            for(int i=0;i<3;i++) { Units.Add(new Unit(i,false,(Role)i,new Cell(1,2+i*2))); Units.Add(new Unit(i+3,true,(Role)i,new Cell(8,2+i*2))); }
            foreach(var p in new[]{new Cell(4,2),new Cell(5,2),new Cell(4,7),new Cell(5,7),new Cell(3,5),new Cell(6,4)}) Rocks.Add(p);
            AddLog("Раунд 1. Ваш ход.");
        }
        public void AddLog(string s) { Log.Insert(0,s); if(Log.Count>30) Log.RemoveAt(Log.Count-1); }
        public bool Inside(Cell p) => p.X>=0 && p.X<Size && p.Y>=0 && p.Y<Size;
        public Unit At(Cell p) => Units.FirstOrDefault(u=>u.Alive && u.Pos.Equals(p));
        public bool Free(Cell p) => Inside(p) && !Rocks.Contains(p) && At(p)==null;
        public bool Active(Unit u) => u!=null && Units.Contains(u) && u.Alive && u.Enemy==EnemyTurn && Result==Outcome.Playing;
        IEnumerable<Cell> Neighbors(Cell p) { yield return new Cell(p.X+1,p.Y); yield return new Cell(p.X-1,p.Y); yield return new Cell(p.X,p.Y+1); yield return new Cell(p.X,p.Y-1); }
        // BFS: расстояние по проходимым клеткам, а не по прямой.
        public Dictionary<Cell,int> Reachable(Unit u, int max) {
            var dist=new Dictionary<Cell,int>(); if(u==null || !u.Alive) return dist;
            var q=new Queue<Cell>(); q.Enqueue(u.Pos); dist[u.Pos]=0;
            while(q.Count>0) { var p=q.Dequeue(); if(dist[p]>=max) continue;
                foreach(var n in Neighbors(p)) if(Free(n) && !dist.ContainsKey(n)) {dist[n]=dist[p]+1;q.Enqueue(n);} }
            dist.Remove(u.Pos); return dist;
        }
        public bool CanMove(Unit u, Cell p) => Active(u) && !u.Moved && Reachable(u,u.Speed).ContainsKey(p);
        public bool Move(Unit u, Cell p) { if(!CanMove(u,p)) return false; u.Pos=p; u.Moved=true; AddLog(Label(u)+" перемещается."); return true; }
        public bool CanAttack(Unit u, Unit target, bool special) => Active(u) && !u.Acted && target!=null && Units.Contains(target) && target.Alive && target.Enemy!=u.Enemy && u.Pos.Distance(target.Pos)<=u.Range && (!special || u.Cooldown==0);
        public bool Attack(Unit u, Unit target, bool special) {
            if(!CanAttack(u,target,special)) return false;
            u.Acted=true; if(special) u.Cooldown=3; // два следующих собственных хода недоступно
            int damage= special ? (u.Role==Role.Warrior?42:u.Role==Role.Archer?38:30) : u.Damage;
            var center=target.Pos; Hit(target,damage);
            if(special && u.Role==Role.Mage) foreach(var other in Units.Where(x=>x.Alive && x.Enemy!=u.Enemy && x!=target && x.Pos.Distance(center)==1).ToArray()) Hit(other,15);
            AddLog(Label(u)+": "+(special?u.Skill:"атака")+"."); Check(); return true;
        }
        void Hit(Unit u,int dmg) {u.HP=Math.Max(0,u.HP-dmg); AddLog(Label(u)+" −"+dmg+" HP"+(u.Alive?"":" (выбыл)"));}
        string Label(Unit u) => (u.Enemy?"Враг: ":"Союзник: ")+u.Name;
        void Check() { if(!Units.Any(u=>u.Alive && u.Enemy)) Result=Outcome.Victory; else if(!Units.Any(u=>u.Alive && !u.Enemy)) Result=Outcome.Defeat; }
        public void EndTurn() {
            if(Result!=Outcome.Playing) return;
            if(EnemyTurn && Round>=RoundLimit) { Result=Outcome.Draw; return; }
            EnemyTurn=!EnemyTurn; if(!EnemyTurn) Round++;
            foreach(var u in Units.Where(x=>x.Alive && x.Enemy==EnemyTurn)) {u.Moved=false;u.Acted=false;u.Cooldown=Math.Max(0,u.Cooldown-1);}
            AddLog(EnemyTurn?"Ход противника.":"Раунд "+Round+". Ваш ход.");
        }
    }
}
