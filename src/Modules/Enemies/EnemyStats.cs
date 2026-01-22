namespace WindyFramework.Modules.Enemies
{
    public class EnemyStats
    {
        public float BaseHealth { get; set; } = 100f;
        public float HealthMultiplier { get; set; } = 1f;

        public float MoveSpeed { get; set; } = 5f;
        public float Damage { get; set; } = 10f;
        public float AttackSpeed { get; set; } = 1f;

        // Aliases for readability in Assembler
        public float MeleeDamage => Damage;
        public float MeleeCooldown => (AttackSpeed > 0) ? 1f / AttackSpeed : 1f;

        public float GetCalculatedHealth()
        {
            return BaseHealth * HealthMultiplier;
        }
    }
}
