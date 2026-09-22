namespace Zlipacket.Core.System.Command.Database
{
    //=CAUTION=
    //All Extension Command MUST be static!
    public abstract class DatabaseExtension
    {
        protected static CommandManager commandManager => CommandManager.Instance;
        
        public static void Extend(CommandDatabase database) { }

        public static CommandParameters ConvertDataToParameters(string[] data, int startingIndex = 0) => new CommandParameters(data, startingIndex);
    }
}