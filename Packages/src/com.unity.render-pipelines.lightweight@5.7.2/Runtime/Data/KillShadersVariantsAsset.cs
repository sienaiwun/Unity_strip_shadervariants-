#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace UnityEngine.Rendering.LWRP
{
   internal class KillShadersVariantsAsset : ScriptableObject
    {
        public StripVariantsInfo m_stripVariantsInfo;
    }
}
