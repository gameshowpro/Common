﻿namespace GameshowPro.Common;

public record SettingsFileSpecification(object Key, string FileName, bool IsWithinLayoutSubfolder);

/// <summary>
/// A standardized way to store and retrieve a set of settings file names against consistent keys.
/// If the key is a <see cref="Type"/>, additional functionality is available.
/// This is the legacy implementation of <see cref="ISettingsFileManager"/>, based on <see cref="Newtonsoft.Json"/>.
/// </summary>
public class SettingsFileManager
{
    private readonly ILogger _logger;
    private readonly FrozenDictionary<object, SettingsFileSpecification> _fileNames;
    private readonly IPersistence _persistence;

    /// <summary>
    /// Construct a <see cref="SettingsFileManager"/> with the given data directory and keyed list of files.
    /// </summary>
    /// <param name="rootFolder">The system folder in which the data directory will be rooted.</param>
    /// <param name="organization">The name of the first level of folder within the system folder.</param>
    /// <param name="project">The name of the second level of folder within the system folder.</param>
    /// <param name="fileNames">A dictionary of file names keyed by their associated <see cref="Type"/> or more complex identifier (in the case of multiple files for one type)</param>
    public SettingsFileManager(
        Environment.SpecialFolder rootFolder,
        string? organization,
        string? project,
        ILogger logger,
        params SettingsFileSpecification[] fileSpecifications
    )
    : this(rootFolder, rootFolder, organization, project, new Persistence(), logger, fileSpecifications)
    { }

    /// <summary>
    /// Construct a <see cref="SettingsFileManager"/> with the given data directory and keyed list of files.
    /// </summary>
    /// <param name="rootFolder">The system folder in which the data directory will be rooted.</param>
    /// <param name="legacyFolder">A system folder in which the data directory may have been previously rooted and be in need of migration.</param>
    /// <param name="organization">The name of the first level of folder within the system folder.</param>
    /// <param name="project">The name of the second level of folder within the system folder.</param>
    /// <param name="fileNames">A dictionary of file names keyed by their associated <see cref="Type"/> or more complex identifier (in the case of multiple files for one type)</param>
    public SettingsFileManager(
        Environment.SpecialFolder rootFolder,
        Environment.SpecialFolder legacyFolder,
        string? organization,
        string? project,
        IPersistence persistence,
        ILogger logger,
        params SettingsFileSpecification[] fileSpecifications
    )
    {
        _logger = logger;
        _persistence = persistence;
        DataDirectory = GetDirectory(rootFolder, organization, project);
        if (legacyFolder != rootFolder)
        {
            string legacyDirectory = GetDirectory(legacyFolder, organization, project);
            MigrateDirectory(DataDirectory, legacyDirectory, logger);
        }
        DataDirectoryUri = new(DataDirectory + @"\", UriKind.Absolute);
        EnsureDataDirectory();
        _fileNames = fileSpecifications
            .ToFrozenDictionary(
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
    private static void MigrateDirectory(string current, string legacy, ILogger logger)
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
                logger.LogInformation("Migrated \"{legacyDir}\" to \"{currentFilePath}\"", file.FullName, currentFilePath);
            }
            catch (Exception ex)
            {
                allFilesMoved = false;
                logger.LogWarning(ex, "Exception while migrating \"{legacyDir}\" to \"{currentFilePath}\"", file.FullName, currentFilePath);
            }
        }


        foreach (DirectoryInfo legacySubDir in legacySubDirs)
        {
            string currentSubDir = Path.Combine(current, legacySubDir.Name);
            MigrateDirectory(currentSubDir, legacySubDir.FullName, logger);
        }
        if (allFilesMoved)
        {
            try
            {
                legacyDir.Delete();
                logger.LogInformation("Deleted \"{legacyDir}\" because all files had been moved", legacyDir.FullName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception while deleting \"{legacyDir}\" because all files had been moved", legacyDir.FullName);
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
    /// Get the absolute path associated with the given key, which is most commonly a <see cref="Type"/>.
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
    /// Try to get the absolute path associated with the given key, which is most commonly a <see cref="Type"/>.
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
    /// Depersist an object from file path which keyed by something other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public async Task<T?> DepersistAsync<T>(object key, CancellationToken? cancellationToken) where T : new()
        => await _persistence.Depersist<T>(GetPath(key), false, true, _logger, cancellationToken);

    /// <summary>
    /// Depersist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public async Task<T?> DepersistAsync<T>(CancellationToken? cancellationToken) where T : new()
        => await DepersistAsync<T>(typeof(T), cancellationToken);


    /// <summary>
    /// Depersist an object from file path which keyed by something other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public async Task<T> DepersistAsyncOrCreate<T>(object key, CancellationToken? cancellationToken) where T : new()
    {
        T? result = await _persistence.Depersist<T>(GetPath(key), false, true, _logger, cancellationToken);
        if (result is null)
        {
            result = new T();
            if (result is IJsonOnDeserialized jsonOnDeserialized)
            {
                jsonOnDeserialized.OnDeserialized();
            }
        }
        return result;
    }

    /// <summary>
    /// Depersist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// If depersisting fails, a new object will be created and OnDeserialized() will be called.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public async Task<T> DepersistAsyncOrCreate<T>(CancellationToken? cancellationToken) where T : new()
        => await DepersistAsyncOrCreate<T>(typeof(T), cancellationToken);

    /// <summary>
    /// Persist an object to a file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being persisted</typeparam>
    /// <param name="obj">The object being persisted. If null, there will be no exception and no operation.</param>
    public async Task PersistAsync<T>(T? obj, CancellationToken? cancellationToken) where T : new()
    {
        if (obj is not null)
        {
            await _persistence.Persist(obj, GetPath(typeof(T)), _logger, cancellationToken);
        }
    }

    #region Legacy blocking methods
    /// <summary>
    /// Depersist an object from file path which keyed by something other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    public T Depersist<T>(object key) where T : new()
    {
        Task<T?> task = DepersistAsync<T>(key, null);
        Task.Run(() => task).Wait();
        return task.Result ?? new T();
    }

    /// <summary>
    /// Depersist an object from file path which keyed by something other than its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <param name="key">The key which was associated with this object's file.</param>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    public T Depersist<T>(object key, out bool isNew) where T : new()
    {
        Task<T?> task = DepersistAsync<T>(key, null);
        Task.Run(() => task).Wait();
        isNew = task.Result is null;
        return task.Result ?? new T();
    }

    /// <summary>
    /// Depersist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted.</typeparam>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    public T Depersist<T>(out bool isNew) where T : new()
    {
        Task<T?> task = DepersistAsync<T>(null);
        Task.Run(() => task).Wait();
        isNew = task.Result is null;
        return task.Result ?? new T();
    }

    /// <summary>
    /// Depersist an object from file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being depersisted</typeparam>
    public T Depersist<T>() where T : new()
    {
        Task<T?> task = DepersistAsync<T>(null);
        Task.Run(() => task).Wait();
        return task.Result ?? new T();
    }

    /// <summary>
    /// Persist an object to a file path which was keyed by its <see cref="Type"/> when this <see cref="SettingsFileManager"/> was constructed.
    /// </summary>
    /// <typeparam name="T">The type of the object being persisted</typeparam>
    /// <param name="obj">The object being persisted. If null, there will be no exception and no operation.</param>
    public void Persist<T>(T? obj) where T : new()
    {
        Task task = PersistAsync(obj, null);
        Task.Run(() => task).Wait();
    }
    #endregion
}


