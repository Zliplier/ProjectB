using System;
using System.Collections.Generic;
using System.Text;

namespace Zlipacket.Core.System.Command
{
    public class CommandData
    {
        public List<Command> commands;

        private const char COMMANDSPLITER_ID = ',';
        private const char ARGUMENTCONTAINER_ID = '(';
        private const string WAITCOMMAND_ID = "[wait]";
        
        public struct Command
        {
            public string name;
            public string[] arguments;
            public bool waitForCompletion;
        }

        public CommandData(string rawCommand)
        {
            commands = RipCommands(rawCommand);
        }

        public List<Command> RipCommands(string rawCommand)
        {
            List<Command> results = new List<Command>();
            
            if (string.IsNullOrWhiteSpace(rawCommand)) return results;
            
            string[] data = rawCommand.Split(COMMANDSPLITER_ID, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string cmd in data)
            {
                Command command = new Command();
                int index = cmd.IndexOf(ARGUMENTCONTAINER_ID);
                command.name = cmd.Substring(0, index).Trim();

                if (command.name.ToLower().StartsWith(WAITCOMMAND_ID))
                {
                    command.name = command.name.Substring(WAITCOMMAND_ID.Length);
                    command.waitForCompletion = true;
                }
                else
                {
                    command.waitForCompletion = false;
                }
                
                command.arguments = GetArgs(cmd.Substring(index + 1, cmd.Length - index - 2));
                results.Add(command);
            }
            
            return results;
        }

        private string[] GetArgs(string args)
        {
            List<string> argsList = new List<string>();
            StringBuilder currentArgs = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && args[i] == ' ')
                {
                    argsList.Add(currentArgs.ToString());
                    currentArgs.Clear();
                    continue;
                }
                
                currentArgs.Append(args[i]);
            }

            if (currentArgs.Length > 0)
                argsList.Add(currentArgs.ToString());
            
            return argsList.ToArray();
        }
    }
}