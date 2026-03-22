using System.CommandLine;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class CommandExtensions
    {
        public static void AddCreateAliases(this Command command)
        {
            command.Aliases.Add("add");
            command.Aliases.Add("a");
            command.Aliases.Add("c");
        }

        public static void AddReadAliases(this Command command)
        {
            command.Aliases.Add("get");
            command.Aliases.Add("show");
            command.Aliases.Add("g");
            command.Aliases.Add("s");
            command.Aliases.Add("r");
            command.Aliases.Add("cat");
        }

        public static void AddUpdateAliases(this Command command)
        {
            command.Aliases.Add("edit");
            command.Aliases.Add("u");
            command.Aliases.Add("e");
        }

        public static void AddDeleteAliases(this Command command)
        {
            command.Aliases.Add("remove");
            command.Aliases.Add("d");
            command.Aliases.Add("rm");
        }

        public static void AddListAliases(this Command command)
        {
            command.Aliases.Add("ls");
            command.Aliases.Add("l");
        }
    }
}
