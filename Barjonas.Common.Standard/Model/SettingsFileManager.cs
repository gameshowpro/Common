using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
#nullable enable
namespace Barjonas.Common.Model
{
    /// <summary>
    /// A standardized way to store and retreive a set of settings file names against consistent keys.
    /// If the key is a <see cref="Type"/>, additional functionlity is available.
    /// </summary>
    public class SettingsFileManager
    {
        private readonly ImmutableDictionary<object, string> _fileNames;

        /// <summary>
        /// Construct a <see cref="SettingsFileManager"/> with the given data directory and keyed list of files.
        /// </summary>
        /// <param name="rootFolder">The system folder in which the data directory will be rooted.</param>
        /// <param name="organization">The name of the first level of folder within the system folder.</param>
        /// <param name="project">The name of the second level of folder within the system folder.</param>
        /// <param name="fileNames">A dictionory of file names keyed by their associated <see cref="Type"/> or more complex identifier (in the case of multiple files for one type)</param>
        public SettingsFileManager(
            Environment.SpecialFolder rootFolder,
            string organization,
            string project,
            Dictionary<object, string> fileNames
        )
        {
            DataDirectory = Path.Combine(Environment.GetFolderPath(rootFolder), organization, project);
            DataDirectoryUri = new(DataDirectory + @"\", UriKind.Absolute);
            EnsureDataDirectory();
            _fileNames = fileNames
                .ToImmutableDictionary(
                    (kv) => kv.Key, 
                    (kv) => Path.Combine(DataDirectory, kv.Value)
                );
        }

        /// <summary>
        /// The absolute path the the data directory.
        /// </summary>
        public string DataDirectory { get; }
        public Uri DataDirectoryUri { get; }

        public void EnsureDataDirectory()
            => Directory.CreateDirectory(DataDirectory);

        /// <summary>
        /// Get the abosolute path associated with the given key, which is most commonly a <see cref="Type"/>.
        /// </summary>
        /// <param name="key">The key associated with the path, which is most commonly a <see cref="Type"/></param>
        public string GetPath(object key)
            => _fileNames[key];

        /// <summary>
        /// Deperist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
        /// </summary>
        /// <typeparam name="T">The type of the object being depersisted</typeparam>
        /// <param name="obj">The object being depersisted</param>
        /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
        public T Depersist<T>(out bool isNew) where T : class, new()
            => Utils.Depersist<T>(GetPath(typeof(T)), out isNew);

        /// <summary>
        /// Persist an object to a file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
        /// </summary>
        /// <typeparam name="T">The type of the object being persisted</typeparam>
        /// <param name="obj">The object being persisted. If null, there will be no exception and no operation.</param>
        public void Persist<T>(T? obj) where T : class, new()
        {
            if (obj is not null)
            {
                Utils.Persist(obj, GetPath(typeof(T)));
            }
        }
    }
}
#nullable restore
