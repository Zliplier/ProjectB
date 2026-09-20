using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Scene
{
    public class SceneManager : Singleton<SceneManager>
    {
        [SerializeField] private string loadingSceneName;
        [SerializeField] private SceneTransition defaultTransition;
        public float fakeLoadingTime = 0.1f;

        private Coroutine co_Loading = null;
        public bool isLoading => co_Loading != null;
        
        public string currentScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        public bool LoadScene(string sceneName = "", string loadingName = "", SceneTransition sceneTransition = null, float inDuration = 0.5f, float outDuration = 0.5f)
        {
            if (isLoading)
            {
                Debug.Log("Another Scene is already loading.");
                return false;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"Scene name cannot be null or empty.");
                return false;
            }
            
            co_Loading = StartCoroutine(TransitionToScene(sceneName, loadingName, sceneTransition, inDuration, outDuration));
            
            Debug.Log("Scene " + sceneName + " loaded.");
            return true;
        }

        public bool LoadSceneAdditive(string sceneName)
        {
            if (isLoading)
            {
                Debug.Log("Another Scene is loading.");
                return false;
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            Debug.Log("Scene" + sceneName + " loaded async.");
            return true;
        }

        public bool UnloadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.Log("Another Scene is loading.");
                return false;
            }
            
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            Debug.Log("Scene" + sceneName + " unloaded async.");
            return true;
        }
        
        private IEnumerator TransitionToScene(string sceneName, string loadingName, SceneTransition sceneTransition = null, float inDuration = 0.5f, float outDuration = 0.5f)
        {
            if (defaultTransition != null && sceneTransition == null)
                sceneTransition = defaultTransition;
            
            if (sceneTransition != null)
                yield return new WaitForSeconds(sceneTransition.TransitionIn(gameObject, inDuration));
            
            //Loading Screen
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadingName == "" ? loadingSceneName : loadingName);
            
            yield return new WaitForSeconds(fakeLoadingTime);
            
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (asyncOperation != null && !asyncOperation.isDone)
                yield return null;
            
            if (sceneTransition != null)
                yield return new WaitForSeconds(sceneTransition.TransitionOut(gameObject, outDuration));
            
            co_Loading = null;
        }
    }
}