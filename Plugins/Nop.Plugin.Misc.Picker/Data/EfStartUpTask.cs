﻿using Nop.Core.Infrastructure;
using System.Data.Entity;

namespace Nop.Plugin.Misc.Picker.Data
{
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<PickerObjectContext>(null);
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
