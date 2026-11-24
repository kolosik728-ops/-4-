using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TacticalArena {
    public sealed class GameController : MonoBehaviour {
        [SerializeField] UnitView unitPrefab;
        Battle battle; Unit selected; bool skill, saved; string screen="menu", notice="";
        readonly Dictionary<int,UnitView> views=new Dictionary<int,UnitView>();
        AudioSource audioSource; Texture2D white, stone; GUIStyle h1,h2,body,small,button,center;
        Coroutine ai; float fps; float flashUntil; Cell flashCell; float volume;
        void Awake() {white=Texture2D.whiteTexture;stone=Resources.Load<Texture2D>("Art/stone");audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;volume=SaveSystem.Volume;Application.targetFrameRate=60;}
        void Update() {fps=Mathf.Lerp(fps,1f/Mathf.Max(Time.unscaledDeltaTime,.001f),.05f);}
        void Styles() {if(h1!=null)return; h1=Style(44,true);h2=Style(24,true);body=Style(18,false);small=Style(14,false);center=Style(14,true);center.alignment=TextAnchor.MiddleCenter;button=new GUIStyle(GUI.skin.button){fontSize=19,fontStyle=FontStyle.Bold,wordWrap=true};}
        GUIStyle Style(int size,bool bold) {var s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,wordWrap=true};s.normal.textColor=new Color(.91f,.94f,.98f);return s;}
        void Box(Rect r,Color c) {GUI.color=c;GUI.DrawTexture(r,white);GUI.color=Color.white;}
        Color Dark => new Color(.08f,.12f,.18f); Color Panel => new Color(.13f,.18f,.25f); Color Blue => new Color(.18f,.45f,.61f);
        void Text(float x,float y,float w,float h,string s,GUIStyle st) => GUI.Label(new Rect(x,y,w,h),s,st);
        bool Button(float x,float y,float w,float h,string s,bool enabled=true) {GUI.enabled=enabled;bool hit=GUI.Button(new Rect(x,y,w,h),s,button);GUI.enabled=true;return hit;}
        void Sound(string name) {var clip=Resources.Load<AudioClip>("Audio/"+name);if(clip!=null)audioSource.PlayOneShot(clip,volume);}
        void StartBattle() {if(ai!=null){StopCoroutine(ai);ai=null;} foreach(var v in views.Values)Destroy(v.gameObject);views.Clear();battle=new Battle();selected=null;skill=false;saved=false;notice="Выберите бойца синего отряда.";flashUntil=0;screen="battle";
            foreach(var u in battle.Units) {var v=unitPrefab!=null?Instantiate(unitPrefab,transform):new GameObject("UnitView").AddComponent<UnitView>(); v.transform.SetParent(transform);v.name=(u.Enemy?"Enemy_":"Player_")+u.Role;v.Bind(u);views[u.Id]=v;} Sound("move");}
        void Menu() {if(ai!=null){StopCoroutine(ai);ai=null;}screen="menu";}
        void Finish() {if(battle.Result==Outcome.Playing || saved)return;saved=true;SaveSystem.Record(battle.Result,battle.Round);Sound(battle.Result==Outcome.Victory?"win":"hit");}
        void EndTurn() {if(battle.Result!=Outcome.Playing || battle.EnemyTurn)return;selected=null;skill=false;battle.EndTurn();notice="Противник принимает решение...";ai=StartCoroutine(EnemyRoutine());}
        IEnumerator EnemyRoutine() {yield return new WaitForSeconds(.35f);foreach(var u in battle.Units.Where(x=>x.Enemy).ToArray()) {for(int i=0;i<2 && battle.Result==Outcome.Playing;i++){var before=u.Pos;bool acted=u.Acted;if(!EnemyAI.Step(battle,u))break; if(!acted && u.Acted){Sound("hit");flashCell=u.Pos;flashUntil=Time.unscaledTime+.3f;}else if(!before.Equals(u.Pos))Sound("move");Finish();yield return new WaitForSeconds(.4f);}}
            if(battle.Result==Outcome.Playing){battle.EndTurn();notice="Ваш ход. Выберите бойца.";}Finish();ai=null;}
        void Click(Cell p) {if(battle.EnemyTurn || battle.Result!=Outcome.Playing)return;var u=battle.At(p);
            if(u!=null && !u.Enemy){selected=u;skill=false;notice=u.Name+": выберите клетку или цель.";return;}
            if(selected==null){notice="Сначала выберите синего бойца.";return;}
            if(u!=null && u.Enemy){if(battle.Attack(selected,u,skill)){Sound("hit");flashCell=p;flashUntil=Time.unscaledTime+.35f;skill=false;notice="Атака выполнена.";Finish();}else notice="Атака недоступна: проверьте дальность, действие и способность.";}
            else if(battle.Move(selected,p)){Sound("move");notice="Перемещение выполнено.";}
            else notice="Недоступная клетка или перемещение уже использовано.";
        }
        void OnGUI() {Styles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);if(scale<=0)return;var old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));Box(new Rect(0,0,1280,800),Dark);
            if(screen=="menu")DrawMenu();else if(screen=="help")DrawHelp();else if(battle!=null)DrawBattle();GUI.matrix=old;}
        void DrawMenu() {Box(new Rect(46,46,1188,708),Panel);Text(86,82,1100,36,"ТЕМА 04   /   ПОШАГОВАЯ ТАКТИКА",small);Text(82,132,1110,70,"TACTICAL ARENA",h1);Text(86,210,950,42,"Тактическая арена · 3 на 3 · один игрок против компьютера",h2);
            for(int i=0;i<3;i++){var role=(Role)i;var t=Resources.Load<Texture2D>("Art/"+role.ToString().ToLowerInvariant());Box(new Rect(690+i*160,315,145,180),Dark);if(t!=null)GUI.DrawTexture(new Rect(707+i*160,328,110,120),t,ScaleMode.ScaleToFit);Text(690+i*160,452,145,35,new Unit(0,false,role,new Cell()).Name,center);}
            if(Button(86,315,505,65,"Начать бой"))StartBattle();if(Button(86,394,505,55,"Как играть"))screen="help";if(Button(86,461,505,55,"Выход"))Application.Quit();
            Text(86,548,500,30,"Громкость",body);float v=GUI.HorizontalSlider(new Rect(86,589,505,25),volume,0,1);if(Mathf.Abs(v-volume)>.001f){volume=v;SaveSystem.Volume=v;}
            Text(690,540,510,36,"СТАТИСТИКА",h2);Text(690,590,510,90,$"Победы: {SaveSystem.Wins}    Поражения: {SaveSystem.Losses}\nНичьи: {SaveSystem.Draws}    Рекорд: {(SaveSystem.Best==0?"—":SaveSystem.Best.ToString())} раундов",body);}
        void DrawHelp() {Text(55,45,1150,65,"Как играть",h1);Text(60,140,1150,490,"1. Нажмите на синего бойца. Зелёные клетки доступны для движения.\n\n2. ЛКМ по свободной клетке — переместиться. ЛКМ по красному бойцу — атаковать, если он в радиусе.\n\n3. Каждый боец может переместиться и атаковать один раз за свой ход, в любом порядке.\n\n4. Кнопка способности включает особую атаку. Перезарядка — два собственных хода.\n\n5. Нажмите «Завершить ход», чтобы передать управление компьютеру.\n\n6. Победа — все враги выбыли; поражение — потеря своего отряда. После 50 раундов — ничья. Камни мешают движению, но не атакам.",h2);if(Button(60,680,300,60,"В меню"))Menu();}
        Rect Tile(Cell p)=>new Rect(54+p.X*60,115+p.Y*60,58,58);
        void DrawBattle() {Text(54,25,650,58,"TACTICAL ARENA",h1);Text(690,40,550,45,$"Раунд {battle.Round} / {Battle.RoundLimit}   ·   {(battle.EnemyTurn?"Ход противника":"Ваш ход")}",h2);
            var reachable=selected!=null && battle.Active(selected) && !selected.Moved?battle.Reachable(selected,selected.Speed):new Dictionary<Cell,int>();
            for(int y=0;y<10;y++)for(int x=0;x<10;x++){var p=new Cell(x,y);var r=Tile(p);var u=battle.At(p);Color c=(x+y)%2==0?new Color(.20f,.28f,.31f):new Color(.24f,.32f,.35f);if(reachable.ContainsKey(p))c=new Color(.22f,.49f,.37f);if(selected!=null && battle.CanAttack(selected,u,skill))c=new Color(.66f,.35f,.23f);if(selected!=null && selected.Pos.Equals(p))c=Blue;Box(r,c);if(battle.Rocks.Contains(p)){if(stone!=null)GUI.DrawTexture(r,stone,ScaleMode.ScaleToFit);else Text(r.x,r.y,r.width,r.height,"■",center);}if(u!=null && views.ContainsKey(u.Id))views[u.Id].Draw(r,white,center,selected==u);if(Time.unscaledTime<flashUntil && p.Equals(flashCell))Box(r,new Color(1,.73f,.26f,.5f));if(battle.Result==Outcome.Playing && !battle.EnemyTurn && GUI.Button(r,GUIContent.none,GUIStyle.none))Click(p);}
            Box(new Rect(690,112,545,608),Panel);Text(710,127,500,35,"УПРАВЛЕНИЕ ОТРЯДОМ",h2);
            if(selected!=null){Text(710,174,505,72,$"{selected.Name}   ·   HP {selected.HP}/{selected.MaxHP}\nДвижение: {(selected.Moved?"использовано":selected.Speed.ToString())}   |   Атака: {(selected.Acted?"использована":selected.Damage.ToString())}   |   Радиус: {selected.Range}",body);}else Text(710,174,505,72,"Выберите синего бойца на поле.",body);
            if(Button(710,255,505,55,selected==null?"Способность":(skill?"Отмена способности":selected.Skill)+(selected.Cooldown>0?" (КД "+selected.Cooldown+")":""),selected!=null && battle.Active(selected) && !selected.Acted && selected.Cooldown==0))skill=!skill;
            if(Button(710,322,505,55,"Завершить ход",!battle.EnemyTurn && battle.Result==Outcome.Playing))EndTurn();Text(710,388,505,62,notice,body);Text(710,459,500,34,"ЖУРНАЛ БОЯ",h2);for(int i=0;i<Mathf.Min(5,battle.Log.Count);i++)Text(710,501+i*29,505,28,battle.Log[i],small);
            if(Button(710,655,235,47,"Новый бой",!battle.EnemyTurn && battle.Result==Outcome.Playing))StartBattle();if(Button(957,655,258,47,"В меню",battle.Result==Outcome.Playing))Menu();Text(54,724,630,43,"Синий — ваш отряд   /   Красный — противник   /   FPS: "+Mathf.RoundToInt(fps),small);
            if(battle.Result!=Outcome.Playing){Box(new Rect(0,0,1280,800),new Color(0,0,0,.78f));Box(new Rect(330,235,620,330),Panel);Text(365,266,555,72,battle.Result==Outcome.Victory?"ПОБЕДА":battle.Result==Outcome.Defeat?"ПОРАЖЕНИЕ":"НИЧЬЯ",h1);Text(365,345,555,45,"Раунд: "+battle.Round+". Результат сохранён.",body);if(Button(365,410,550,60,"Играть снова"))StartBattle();if(Button(365,484,550,53,"Главное меню"))Menu();}}
    }
}
