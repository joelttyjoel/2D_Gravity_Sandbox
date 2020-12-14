namespace Assets.Scripts.TilemapStuff
{
    public class MyTile
    {
        public bool isInteractable;
        public bool isDestructible;
        public bool isTurnedOn;
        public float mass;
        public float hardness;

        public void SetInteractable(bool inputBool)
        {
            isInteractable = inputBool;
        }
        public void SetIsDestructible(bool inputBool)
        {
            isDestructible = inputBool;
        }
        public void SetIsTurnedOn(bool inputBool)
        {
            isTurnedOn = inputBool;
        }
    }
}
