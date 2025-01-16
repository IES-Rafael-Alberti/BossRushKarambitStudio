using UnityEngine;

[CreateAssetMenu(fileName = "DatosCartas", menuName = "BaseDatosCartas/List", order = 1)]
public class BaseDatosCartas : ScriptableObject
{
    [System.Serializable]
    public struct ObjetoColeccion
    {
        public int id;
        public int curacion;
        public int daño;
        public int proteccion;
        public bool esEsquiva;
        public Sprite spriteCarta;
        public string infoES;
        public string infoEN;
        public ActionType actionType;
        public SpecialAttackType specialAttackType;
    }

    public ObjetoColeccion[] baseDatos;
}