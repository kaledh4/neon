using UnityEngine;

namespace NeonSplash.V0_1
{
    public enum Team
    {
        None,
        Red,
        Blue
    }

    public class PlayerTeam : MonoBehaviour
    {
        public Team team = Team.Blue; // Default for testing
    }
}
