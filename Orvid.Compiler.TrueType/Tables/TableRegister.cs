using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Compiler.TrueType.Tables
{
	internal static class TableRegister
	{
		private static Dictionary<string, ITable> Tables = new Dictionary<string, ITable>();

		static TableRegister()
		{
			foreach (Type t in typeof(TableRegister).Assembly.GetTypes())
			{
				if (t.GetInterface("ITable") != null)
				{
					ITable tbl = (ITable)Activator.CreateInstance(t);
					Tables.Add(tbl.TableTag, tbl);
				}
			}
		}

		public static ITable GetTable(string tab)
		{
			if (Tables.ContainsKey(tab))
			{
				return Tables[tab];
			}
			else
			{
				throw new Exception("Unknown table!");
			}
		}

		public static Queue<TableRecordEntry> DelayedTables = new Queue<TableRecordEntry>();

		public static void DelayTable(TableRecordEntry entry)
		{
			DelayedTables.Enqueue(entry);
		}


	}
}
