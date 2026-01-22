namespace WindyFramework.Modules.Enemies
{
    public class EnemyBlueprint
    {
        public string ID { get; set; }
        public string BasePrefabName { get; set; }
        public EnemyStats BaseStats { get; set; }

        public EnemyBlueprint()
        {
            BaseStats = new EnemyStats();
        }
    }
}
