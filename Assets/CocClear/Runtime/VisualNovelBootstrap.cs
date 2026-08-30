using UnityEngine;

namespace CocClear.Runtime
{
    public static class VisualNovelBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Object.FindFirstObjectByType<VisualNovelController>() != null)
            {
                return;
            }

            var root = new GameObject("VisualNovelController");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<VisualNovelController>();
        }
    }
}
