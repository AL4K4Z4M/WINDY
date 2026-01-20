namespace WindyFramework.Core
{
    public interface IWindyModule
    {
        void OnInitialize();
        void OnUpdate();
        void OnFixedUpdate();
        void OnLateUpdate();
        void OnGUI();
        void OnSceneWasLoaded(int buildIndex, string sceneName);
        void OnSceneWasInitialized(int buildIndex, string sceneName);
        void OnPreferencesSaved();
    }
}
