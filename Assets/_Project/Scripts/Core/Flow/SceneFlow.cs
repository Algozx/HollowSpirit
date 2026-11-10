using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowSpirit.Core.Flow
{
    /// <summary>Owns scene flow; keeps Boot alive and loads the level scene additively.</summary>
    public sealed class SceneFlow : MonoBehaviour
    {
        [Tooltip("Build index of the scene loaded additively on start.")]
        [SerializeField] private int firstSceneBuildIndex = 1;

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);

            Scene scene = SceneManager.GetSceneByBuildIndex(firstSceneBuildIndex);
            if (!scene.isLoaded)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(firstSceneBuildIndex, LoadSceneMode.Additive);
                if (load == null)
                {
                    Debug.LogError($"SceneFlow: no scene at build index {firstSceneBuildIndex}.", this);
                    yield break;
                }

                yield return load;
                scene = SceneManager.GetSceneByBuildIndex(firstSceneBuildIndex);
            }

            SceneManager.SetActiveScene(scene);
        }
    }
}
