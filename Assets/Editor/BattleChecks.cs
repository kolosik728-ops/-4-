using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TacticalArena;
public static class BattleChecks {
    [MenuItem("Task4/Проверить логику боя")]
    public static void Run() {
        int count=0, failed=0;
        Action<string,Action> test=(name,action)=>{try{action();count++;Debug.Log("PASS: "+name);}catch(Exception e){failed++;Debug.LogError("FAIL: "+name+" — "+e.Message);}};
        test("01: начальное состояние",()=>{var b=new Battle();Require(b.Units.Count==6 && b.Rocks.Count==6 && b.Round==1 && !b.EnemyTurn);});
        test("02: движение BFS",()=>{var b=new Battle();var u=b.Units[0];Require(b.Move(u,new Cell(2,2)) && u.Pos.Equals(new Cell(2,2)));});
        test("03: запрет занятой клетки, камня и выхода",()=>{var b=new Battle();var u=b.Units[0];Require(!b.Move(u,new Cell(1,4))&&!b.Move(u,new Cell(4,2))&&!b.Move(u,new Cell(-1,2)));});
        test("04: повторное движение",()=>{var b=new Battle();var u=b.Units[0];Require(b.Move(u,new Cell(2,2))&&!b.Move(u,new Cell(3,2)));});
        test("05: дальность и дружественный огонь",()=>{var b=new Battle();var u=b.Units[0];Require(!b.Attack(u,b.Units[1],false)&&!b.Attack(u,b.Units[2],false));});
        test("06: обычная атака",()=>{var b=new Battle();var u=b.Units[0];var e=b.Units[1];e.Pos=new Cell(2,2);Require(b.Attack(u,e,false)&&e.HP==75&&!b.Attack(u,e,false));});
        test("07: особая атака и перезарядка",()=>{var b=new Battle();var u=b.Units[0];var e=b.Units[1];e.Pos=new Cell(2,2);Require(b.Attack(u,e,true)&&e.HP==58&&u.Cooldown==3);for(int i=0;i<2;i++){b.EndTurn();b.EndTurn();Require(!b.CanAttack(u,e,true));}b.EndTurn();b.EndTurn();Require(b.CanAttack(u,e,true));});
        test("08: магический урон по площади",()=>{var b=new Battle();var m=b.Units[4];var a=b.Units[1];var c=b.Units[3];a.Pos=new Cell(2,6);c.Pos=new Cell(2,7);Require(b.Attack(m,a,true)&&a.HP==70&&c.HP==55);});
        test("09: смена ходов и запрет хода игрока",()=>{var b=new Battle();var u=b.Units[0];b.Move(u,new Cell(2,2));b.EndTurn();Require(b.EnemyTurn&&!b.CanMove(u,new Cell(3,2)));b.EndTurn();Require(!b.EnemyTurn&&b.Round==2&&!u.Moved);});
        test("10: алгоритм противника",()=>{var b=new Battle();b.EndTurn();var e=b.Units[1];var p=e.Pos;Require(EnemyAI.Step(b,e)&&!e.Pos.Equals(p)&&e.Moved);});
        test("11: победа",()=>{var b=new Battle();var u=b.Units[0];var e=b.Units[1];foreach(var x in b.Units.Where(x=>x.Enemy))x.HP=0;e.HP=1;e.Pos=new Cell(2,2);Require(b.Attack(u,e,false)&&b.Result==Outcome.Victory);});
        test("12: поражение",()=>{var b=new Battle();var e=b.Units[1];var u=b.Units[0];foreach(var x in b.Units.Where(x=>!x.Enemy))x.HP=0;u.HP=1;u.Pos=new Cell(7,2);b.EndTurn();Require(b.Attack(e,u,false)&&b.Result==Outcome.Defeat);});
        test("13: ничья",()=>{var b=new Battle();for(int i=0;i<100;i++)b.EndTurn();Require(b.Result==Outcome.Draw&&b.Round==50);});
        test("14: новый экземпляр не наследует состояние",()=>{var b=new Battle();b.Move(b.Units[0],new Cell(2,2));var n=new Battle();Require(n.Round==1&&!n.Units[0].Moved&&n.Units[0].Pos.Equals(new Cell(1,2)));});
        Debug.Log($"Task4: успешно {count}, ошибок {failed}");if(failed>0)throw new Exception("Есть непройденные проверки.");
    }
    static void Require(bool value){if(!value)throw new Exception("Условие не выполнено");}
}
