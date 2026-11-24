using UnityEngine;
namespace TacticalArena {
    // Повторяемый префаб: графическое представление одного бойца.
    public sealed class UnitView : MonoBehaviour {
        public Unit Model {get;private set;}
        Texture2D art;
        public void Bind(Unit u) {Model=u;art=Resources.Load<Texture2D>("Art/"+u.Role.ToString().ToLowerInvariant());}
        public void Draw(Rect r, Texture2D white, GUIStyle text, bool selected) {
            if(Model==null || !Model.Alive) return;
            GUI.color=Model.Enemy?new Color(.95f,.34f,.32f):new Color(.25f,.76f,.92f);
            GUI.DrawTexture(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),white); GUI.color=new Color(.10f,.14f,.20f);
            GUI.DrawTexture(new Rect(r.x+7,r.y+7,r.width-14,r.height-14),white); GUI.color=Color.white;
            if(art!=null) GUI.DrawTexture(new Rect(r.x+10,r.y+7,r.width-20,r.height-19),art,ScaleMode.ScaleToFit);
            GUI.color=new Color(.12f,.16f,.22f); GUI.DrawTexture(new Rect(r.x+7,r.yMax-10,r.width-14,5),white);
            GUI.color=Model.Enemy?new Color(1,.35f,.35f):new Color(.35f,.9f,.65f);GUI.DrawTexture(new Rect(r.x+7,r.yMax-10,(r.width-14)*Model.HP/Model.MaxHP,5),white);
            GUI.color=Color.white; if(selected) GUI.Label(new Rect(r.x+3,r.y+2,20,20),"◆",text);
            if(Model.Acted && Model.Moved) {GUI.color=new Color(0,0,0,.4f);GUI.DrawTexture(r,white);GUI.color=Color.white;}
        }
    }
}
