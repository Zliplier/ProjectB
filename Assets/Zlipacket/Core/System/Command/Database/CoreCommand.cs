using System;
using UnityEngine;

namespace Zlipacket.Core.System.Command.Database
{
    public class CoreCommand : DatabaseExtension
    {
        public new static void Extend(CommandDatabase database)
        {
            database.AddCommand("Print", new Action<string>(Print));
        }

        public static void Print(string data)
        {
            Debug.Log(data);
        }
    }
}