using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ToothPick.Extensions
{
    /// <summary>
    /// A set of <see cref="DbSet{Setting}"/> extensions for settings, to quickly retrieve or save them. Manages default values for pre-defined settings.
    /// </summary>
    public static class DbSetExtensions
    {
        #region DbSet<Setting> extensions

        /// <summary>
        /// Retrieves a setting with the given <paramref name="name"/>. If not found, returns the default value or an empty string if no default value defined.
        /// </summary>
        /// <param name="dbSet">From extension, the settings <see cref="DbSet{Setting}"/>.</param>
        /// <param name="name">The name of the setting to retrieve.</param>
        /// <param name="service">The service that the setting is associaated with.</param>
        /// <returns>The setting, from the database or the default dictionary.</returns>
        public static async Task<Setting> GetSettingAsync(this DbSet<Setting> dbSet, string name, CancellationToken cancellationToken = default)
        {
            Setting returnSetting = await dbSet.FirstOrDefaultAsync(setting => setting.Name.Equals(name), cancellationToken);

            if (returnSetting == null)
            {
                if (Defaults.Settings.TryGetValue(name, out string value) && value != null)
                    returnSetting = await dbSet.SetSettingAsync(name, value, cancellationToken);
                else
                    returnSetting = await dbSet.SetSettingAsync(name, "", cancellationToken);

                return returnSetting;
            }
            else
                return returnSetting;
        }

        /// <summary>
        /// Saves a DB <see cref="Setting"/> with the given <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="dbSet">From extension, the settings <see cref="DbSet{Setting}"/>.</param>
        /// <param name="name">The name of the setting to create or update.</param> 
        /// <param name="service">The service that the setting is associaated with.</param>
        /// <param name="description">The description to set.</param>
        /// <param name="value">The value to set.</param>
        public static async Task<Setting> SetSettingAsync(this DbSet<Setting> dbSet, string name, string value, CancellationToken cancellationToken = default)
        {
            Setting setting = await dbSet.FirstOrDefaultAsync(setting => setting.Name.Equals(name), cancellationToken);

            if (setting == null)
            {
                setting = new Setting
                {
                    Name = name,
                    Value = value
                };

                EntityEntry<Setting> entity = await dbSet.AddAsync(setting, cancellationToken);
                await entity.Context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                setting.Value = value;

                EntityEntry<Setting> entity = dbSet.Update(setting);
                await entity.Context.SaveChangesAsync(cancellationToken);
            }

            return setting;
        }

        /// <summary>
        /// Populates the DB with the default string value for any default settings that are missing.
        /// </summary>
        /// <param name="dbSet">From extension, the settings <see cref="DbSet{Setting}"/>.</param>
        public static async Task PopulateDefaultsAsync(this DbSet<Setting> dbSet, CancellationToken cancellationToken = default)
        {
            foreach (string name in Defaults.Settings.Keys)
                await dbSet.GetSettingAsync(name, cancellationToken);
        }

        #endregion
    }
}