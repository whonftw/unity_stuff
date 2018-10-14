namespace input
{
    public struct ActionInfo
    {
        public UnityEngine.Vector3 InitialMousePosition { get; set; }
        public UnityEngine.Vector3 LastMousePosition { get; set; }
        public ActionState State { get; set; }
    }
}