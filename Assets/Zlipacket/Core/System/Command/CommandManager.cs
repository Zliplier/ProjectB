using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Zlipacket.Core.System.Command.Database;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.System.Command
{
    public class CommandManager : Singleton<CommandManager>
    {
        private const char SUB_COMMAND_IDENTIFIER = '.';
        
        private CommandDatabase database;
        private Dictionary<string, CommandDatabase> subDatabases;
        
        private List<CommandProcess> activeProcesses = new();
        private CommandProcess topProcess => activeProcesses.LastOrDefault();
        
        public override void Awake()
        {
            base.Awake();
            database = new CommandDatabase();
            subDatabases = new Dictionary<string, CommandDatabase>();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DatabaseExtension))).ToArray();

            foreach (Type type in extensionTypes)
            {
                MethodInfo extendMethod = type.GetMethod("Extend");
                extendMethod.Invoke(null, new object[] { database });
            }
        }

        public CoroutineWrapper Excute(string commandName, params string[] args)
        {
            if (commandName.Contains(SUB_COMMAND_IDENTIFIER))
                return ExcuteSubCommand(commandName, args);
            
            Delegate command = database.GetCommand(commandName);
            
            if (command == null)
                return null;

            return StartProcess(commandName, command, args);
        }

        private CoroutineWrapper ExcuteSubCommand(string commandName, string[] args)
        {
            string[] parts = commandName.Split(SUB_COMMAND_IDENTIFIER);
            string databaseName = string.Join(SUB_COMMAND_IDENTIFIER, parts.Take(parts.Length - 1));
            string subCommandName = parts.Last();

            if (subDatabases.ContainsKey(databaseName))
            {
                Delegate command = subDatabases[databaseName].GetCommand(subCommandName);
                if (command != null)
                    return StartProcess(commandName, command, args);
                else
                {
                    Debug.LogError($"Sub-Database: {databaseName} does not contains command: {subCommandName}.");
                    return null;
                }
            }
            
            Debug.LogError($"Sub-Database: {databaseName} does not existed. Command: {subCommandName} could not be found.");
            return null;
        }

        private CoroutineWrapper StartProcess(string commandName, Delegate command, params string[] args)
        {
            Guid processId = Guid.NewGuid();
            CommandProcess cmd = new(processId, commandName, command, args, null, null);
            activeProcesses.Add(cmd);

            Coroutine co = StartCoroutine(RunningProcess(cmd));
            cmd.runningProcess = new(this, co);

            return cmd.runningProcess;
        }

        public void StopCurrentProcess()
        {
            if (topProcess != null)
                KillProcess(topProcess);
        }

        private IEnumerator RunningProcess(CommandProcess process)
        {
            yield return WaitForProcessToComplete(process.command, process.args);
            
            KillProcess(process);
        }

        public void KillProcess(CommandProcess cmd)
        {
            activeProcesses.Remove(cmd);

            if (cmd.runningProcess != null && !cmd.runningProcess.isDone)
                cmd.runningProcess.Stop();
            
            cmd.onTerminateAction?.Invoke();
        }
        
        public void StopAllProcess()
        {
            foreach (var c in activeProcesses)
            {
                if (c.runningProcess != null && !c.runningProcess.isDone)
                     c.runningProcess.Stop();
                
                c.onTerminateAction?.Invoke();
            }
            
            activeProcesses.Clear();
        }
        
        private IEnumerator WaitForProcessToComplete(Delegate command, string[] args)
        {
            if (command is Action)
                command.DynamicInvoke();
            
            else if(command is Action<string>)
                command.DynamicInvoke(args[0]);
            
            else if(command is Action<string[]>)
                command.DynamicInvoke((object)args);
            
            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();
            
            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args[0]);
            
            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command)(args);
        }

        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            CommandProcess process = topProcess;
            
            if (process == null)
                return;
            
            process.onTerminateAction = new();
            process.onTerminateAction.AddListener(action);
        }
        
        public CommandDatabase GetSubDatabase(string databaseName) => subDatabases.GetValueOrDefault(databaseName.ToLower());
        
        public CommandDatabase CreateSubDatabase(string databaseName)
        {
            databaseName = databaseName.ToLower();
            CommandDatabase subDatabase = GetSubDatabase(databaseName);

            if (subDatabase != null)
            {
                Debug.LogError($"Sub-Database: {databaseName} already exists.");
                return null;
            }
            
            subDatabase = new CommandDatabase();
            subDatabases.Add(databaseName, subDatabase);
            
            return subDatabase;
        }
    }
}