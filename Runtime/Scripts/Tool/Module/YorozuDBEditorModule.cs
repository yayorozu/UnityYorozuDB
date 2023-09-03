#if UNITY_EDITOR

using System;

namespace Yorozu.DB
{
    [Serializable]
    internal abstract class YorozuDBEditorModule
    {
        internal abstract bool OnGUI();
    }
}

#endif