#nullable enable
namespace Barjonas.Common.Model;

public record SettingsFileSpecification(object Key, string FileName, bool IsWithinLayoutSubfolder);

/// <summary>
/// A standardized way to store and retreive a set of settings file names against consistent keys.
/// If the key is a <see cref="Type"/>, additional functionlity is available.
/// </summary>
public class SettingsFileManager
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private readonly ImmutableDictionary<object, SettingsFileSpecification> _fileNames;

    /// <summary>
    /// Construct a <see cref="SettingsFileManager"/> with the given data directory and keyed list of files.
    /// </summary>
    /// <param name="rootFolder">The system folder in which the data directory will be rooted.</param>
    /// <param name="organization">The name of the first level of folder within the system folder.</param>
    /// <param name="project">The name of the second level of folder within the system folder.</param>
    /// <param name="fileNames">A dictionory of file names keyed by their associated <see cref="Type"/> or more complex identifier (in the case of multiple files for one type)</param>
    public SettingsFileManager(
        Environment.SpecialFolder rootFolder,
        string? organization,
        string? project,
        params SettingsFileSpecification[] fileSpecifications
    )
    : this(rootFolder, rootFolder, organization, project, fileSpecifications)
    { }

    /// <summary>
    /// Construct a <see cref="SettingsFileManager"/> with the given data directory and keyed list of files.
    /// </summary>
    /// <param name="rootFolder">The system folder in which the data directory will be rooted.</param>
    /// <param name="legacyFolder">A system folder in which the data directory may have been previously rooted and be in need of migration.</param>
    /// <param name="organization">The name of the first level of folder within the system folder.</param>
    /// <param name="project">The name of the second level of folder within the system folder.</param>
    /// <param name="fileNames">A dictionory of file names keyed by their associated <see cref="Type"/> or more complex identifier (in the case of multiple files for one type)</param>
    public SettingsFileManager(
        Environment.SpecialFolder rootFolder,
        Environment.SpecialFolder legacyFolder,
        string? organization,
        string? project,
        params SettingsFileSpecification[] fileSpecifications
    )
    {
        DataDirectory = GetDirectory(rootFolder, organization, project);
        if (legacyFolder != rootFolder)
        {
            string legacyDirectory = GetDirectory(legacyFolder, organization, project);
            MigrateDirectory(DataDirectory, legacyDirectory);
        }
        DataDirectoryUri = new(DataDirectory + @"\", UriKind.Absolute);
        EnsureDataDirectory();
        _fileNames = fileSpecifications
            .ToImmutableDictionary(
                (fs) => fs.Key,
                (fs) => fs
            );

        static string GetDirectory(Environment.SpecialFolder folder, string? organization, string? project)
        {
            string rootPath = Environment.GetFolderPath(folder);
            if (organization == null || project == null)
            {
                return rootPath;
            }
            else
            {
                return Path.Combine(rootPath, organization, project);
            }
        }
    }

    /// <summary>
    /// /Copy any files found in legacy directory but not present in the current directory
    /// </summary>
    /// <param name="current">The destination</param>
    /// <param name="legacy">The possible original</param>
    private static void MigrateDirectory(string current, string legacy)
    {
        DirectoryInfo? legacyDir = new(legacy);
        if (!legacyDir.Exists)
        {
            return;
        }
        DirectoryInfo[] legacySubDirs = legacyDir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(current);

        bool allFilesMoved = true;
        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in legacyDir.GetFiles())
        {
            string currentFilePath = Path.Combine(current, file.Name);
            try
            {
                file.MoveTo(currentFilePath, false);
                s_logger.Info("Migrated \"{legacyDir}\" to \"{currentFilePath}\"", file.FullName, currentFilePath);
            }
            catch (Exception ex)
            {
                allFilesMoved = false;
                s_logger.Warn(ex, "Exception while migrating \"{legacyDir}\" to \"{currentFilePath}\"", file.FullName, currentFilePath);
            }
        }


        foreach (DirectoryInfo legacySubDir in legacySubDirs)
        {
            string currentSubDir = Path.Combine(current, legacySubDir.Name);
            MigrateDirectory(currentSubDir, legacySubDir.FullName);
        }
        if (allFilesMoved)
        {
            try
            {
                legacyDir.Delete();
                s_logger.Info("Deleted \"{legacyDir}\" because all files had been moved", legacyDir.FullName);
            }
            catch (Exception ex)
            {
                s_logger.Warn(ex, "Exception while deleting \"{legacyDir}\" because all files had been moved", legacyDir.FullName);
            }
        }
    }

    /// <summary>
    /// The absolute path the the data directory.
    /// </summary>
    public string DataDirectory { get; }
    public Func<string?>? LayoutSubdirectoryGetter { get; set; }
    public Uri DataDirectoryUri { get; }

    public void EnsureDataDirectory()
        => Directory.CreateDirectory(DataDirectory);

    /// <summary>
    /// Get the abosolute path associated with the given key, which is most commonly a <see cref="Type"/>.
    /// </summary>
    /// <param name="key">The key associated with the path, which is most commonly a <see cref="Type"/></param>
    public string? GetPath(object key)
    {
        if (TryGetPath(key, out string? path))
        {
            return path;
        }
        return null;
    }


    /// <summary>
    /// Try to get the abosolute path associated with the given key, which is most commonly a <see cref="Type"/>.
    /// </summary>
    /// <param name="key">The key associated with the path, which is most commonly a <see cref="Type"/></param>
    public bool TryGetPath(object key, [NotNullWhen(true)] out string? path)
    {
        if (_fileNames.TryGetValue(key, out SettingsFileSpecification? settingsFile))
        {
            if (settingsFile.IsWithinLayoutSubfolder)
            {
                string? layoutSubdirectory = LayoutSubdirectoryGetter?.Invoke();
                if (string.IsNullOrWhiteSpace(layoutSubdirectory))
                {
                    path = null;
                    return false;
                }
                path = Path.Combine(DataDirectory, layoutSubdirectory, settingsFile.FileName);
                return true;
            }
            else
            {
                path = Path.Combine(DataDirectory, settingsFile.FileName);
                return true;
            }
        }
        path = null;
        return false;
    }

    /// <summary>
    /// Deperist an object from file path which keyed by somethign other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public T Depersist<T>(object key) where T : class, new()
        => Utils.Depersist<T>(GetPath(key), out _);

    /// <summary>
    /// Deperist an object from file path which keyed by somethign other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    public T Depersist<T>(object key, out bool isNew) where T : class, new()
        => Utils.Depersist<T>(GetPath(key), out isNew);

    /// <summary>
    /// Deperist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    public T Depersist<T>(out bool isNew) where T : class, new()
        => Utils.Depersist<T>(GetPath(typeof(T)), out isNew);

    /// <summary>
    /// Deperist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted</typeparam>
    public T Depersist<T>() where T : class, new()
         => Utils.Depersist<T>(GetPath(typeof(T)), out _);

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
#nullable restore
