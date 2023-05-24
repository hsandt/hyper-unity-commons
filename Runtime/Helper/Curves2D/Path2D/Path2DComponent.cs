using UnityEngine;
using UnityEngine.Serialization;

namespace HyperUnityCommons
{
    public abstract class Path2DComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("Is the path relative to the game object's position?")]
        [FormerlySerializedAs("isRelative")]
        protected bool m_IsRelative = true;

        public bool IsRelative => m_IsRelative;

        public abstract Path2D Path { get; }

        // Proxy methods to take world position into account if m_IsRelative

        public Vector2 InterpolatePathByParameter(float pathT)
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return Path.InterpolatePathByParameter(pathT) + offset;
        }
        public Vector2 InterpolatePathByNormalizedParameter(float normalizedT)
        {
            Vector2 offset = m_IsRelative ? (Vector2)transform.position : Vector2.zero;
            return Path.InterpolatePathByNormalizedParameter(normalizedT) + offset;
        }
    }
}