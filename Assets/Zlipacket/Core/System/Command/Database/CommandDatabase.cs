using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zlipacket.Core.System.Command.Database
{
    public class CommandDatabase
    {
        private Dictionary<string, Delegate> database = new();

        public bool hasCommand(string commandName) => database.ContainsKey(commandName.ToLower());

        public void AddCommand(string commandName, Delegate commandDelegate)
        {
            commandName = commandName.ToLower();
            
            if (!database.TryAdd(commandName, commandDelegate))
                Debug.LogError($"Redundant database entries. Command already exists in database: {commandName}");
        }
        
        public Delegate GetCommand(string commandName)
        {
            commandName = commandName.ToLower();
            
            if (!database.ContainsKey(commandName))
            {
                Debug.LogError($"Command: {commandName} not found in database.");
                return null;
            }
            
            return database[commandName];
        }
    }
}