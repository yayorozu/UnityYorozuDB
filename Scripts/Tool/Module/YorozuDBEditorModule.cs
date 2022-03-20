using System;

namespace Yorozu.DB
{
    [Serializable]
    internal abstract class YorozuDBEditorModule
    {
        private YorozuDBEditorWindow _window;

        internal void Initialize(YorozuDBEditorWindow manager)
        {
            _window = manager;
            OnInitialize();
        }

        protected virtual void OnInitialize(){}

        protected void ChangeModule<T>(object param = null) where T : YorozuDBEditorModule
        {
            _window.ChangeModule(typeof(T), param);
        }

        internal abstract void OnGUI();
        internal abstract void OnEnter(object param);
        internal abstract void OnExit();
    }
}