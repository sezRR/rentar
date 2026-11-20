using Rentar.Modules.Fleet.Application.CarImages;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Common.Interfaces;

/// <summary>
/// Where car image files live. Inside an ambient transaction the files follow the database: a deleted file
/// is only removed once the transaction commits, and a saved file is removed again if it rolls back.
/// </summary>
public interface IImageStorage
{
    /// <summary>Name of the shared placeholder image used by cars without an upload. It is never deleted.</summary>
    const string DefaultImageName = "default.png";

    /// <summary>Stores the file under a new unique name and returns that name.</summary>
    Task<string> SaveAsync(FileUpload file, CancellationToken cancellationToken = default);

    /// <summary>Stores the file and removes the one it replaces; returns the new name.</summary>
    Task<string> ReplaceAsync(FileUpload file, string oldFileName, CancellationToken cancellationToken = default);

    /// <summary>Synchronous: file systems offer no asynchronous delete.</summary>
    void Delete(string fileName);
}
