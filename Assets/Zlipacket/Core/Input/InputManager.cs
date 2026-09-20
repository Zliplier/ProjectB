using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Input
{
    public class InputManager : Singleton<InputManager>
    {
        public InputActionAsset inputActionAsset;

        public List<InputMapContext> inputMapContexts;

        public T GetInputMapContext<T>(string mapName) where T : InputMapContext
            => inputMapContexts.Find(m => string.Equals(m.ActionMapName, mapName)) as T;

        public override void Awake()
        {
            base.Awake();
            
            if (inputActionAsset == null)
                return;
            foreach(var map in inputMapContexts)
                map?.Initialize(inputActionAsset);
        }

        public void OnEnable()
        {
            foreach (var map in inputMapContexts)
                map?.EnableMap();
        }

        private void OnDisable()
        {
            foreach (var map in inputMapContexts)
                map?.DisableMap();
        }
    }
}