using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Zlipacket.Core.Camera
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private List<CinemachineCamera> cameraList;
        public CinemachineCamera currentCamera;
        public bool allowSwitching = true;
        public void SetAllowSwitching(bool value) => allowSwitching = value;
        
        public void ChangeToCamera(int cameraIndex)
        {
            CinemachineCamera targetCamera = cameraList[cameraIndex];

            if (targetCamera == null)
            {
                Debug.LogError($"Target camera not found at camera number: {cameraIndex}");
                return;
            }
            
            ChangeToCamera(targetCamera);
        }
        
        public void ChangeToCamera(CinemachineCamera targetCamera)
        {
            if (!allowSwitching)
                return;
            
            currentCamera.Priority--;
            targetCamera.Priority++;
            
            currentCamera = targetCamera;
        }
    }
}
