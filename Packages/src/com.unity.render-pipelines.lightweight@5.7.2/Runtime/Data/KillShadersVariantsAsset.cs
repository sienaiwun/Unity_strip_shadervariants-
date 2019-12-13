#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace UnityEngine.Rendering.LWRP
{

    [CreateAssetMenu(menuName = "Shader/killShaderVariants")]
    internal class KillShadersVariantsAsset : ScriptableObject
    {
        public StripVariantsInfo m_stripVariantsInfo;
    }
}
