namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Decides whether a hit interrupts an enemy; a hit never interrupts a swing that is already hurting.</summary>
    public static class PoiseRule
    {
        public static bool Interrupts(bool insideDamageWindow)
        {
            return !insideDamageWindow;
        }
    }
}
