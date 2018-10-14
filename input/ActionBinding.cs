namespace input
{
    public struct ActionBinding
    {
        public UnityEngine.KeyCode Key { get; set; }

        // whether or not to fire key hold event
        public bool ShouldTick { get; set; }

        public UnityEngine.KeyCode? Modifier { get; set; }
    }
}