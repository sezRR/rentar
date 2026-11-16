using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rentar.Modules.Fleet.Application.CarImages;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Rentar.Modules.Fleet.Application.Common.Interfaces;

namespace Rentar.Modules.Fleet.Infrastructure.Storage;

/// <summary>
/// Keeps uploaded images in the configured uploads folder on local disk.
/// </summary>
public class LocalImageStorage(IOptions<ImageStorageOptions> options, IHostEnvironment environment) : IImageStorage
{
    public string UploadsPath { get; } = Path.GetFullPath(options.Value.DefaultCarImagePath, environment.ContentRootPath);

    public async Task<string> SaveAsync(FileUpload file, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(UploadsPath);

        var fileName = NewFileName(file);
        var fullPath = Path.Combine(UploadsPath, fileName);

        var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        try
        {
            await using (target)
            await using (var source = file.OpenReadStream())
            {
                await source.CopyToAsync(target, cancellationToken);
            }
        }
        catch
        {
            // A copy cut short, e.g. by a cancelled request, leaves no partial file behind.
            File.Delete(fullPath);
            throw;
        }

        OnTransactionEnd(committed =>
        {
            if (!committed)
            {
                File.Delete(fullPath);
            }
        });

        return fileName;
    }

    public async Task<string> ReplaceAsync(FileUpload file, string oldFileName, CancellationToken cancellationToken = default)
    {
        var fileName = await SaveAsync(file, cancellationToken);

        Delete(oldFileName);

        return fileName;
    }

    public void Delete(string fileName)
    {
        // The default image is shared by every car without an upload, so it is never removed.
        if (string.IsNullOrEmpty(fileName) || fileName == IImageStorage.DefaultImageName)
        {
            return;
        }

        // Only the file name is kept so a stored value can not point outside the uploads folder.
        var fullPath = Path.Combine(UploadsPath, Path.GetFileName(fileName));

        if (Transaction.Current == null)
        {
            File.Delete(fullPath);
            return;
        }

        OnTransactionEnd(committed =>
        {
            if (committed)
            {
                File.Delete(fullPath);
            }
        });
    }

    private static void OnTransactionEnd(Action<bool> action)
    {
        var transaction = Transaction.Current;
        if (transaction == null)
        {
            return;
        }

        transaction.TransactionCompleted += (_, e) =>
            action(e.Transaction.TransactionInformation.Status == TransactionStatus.Committed);
    }

    private static string NewFileName(FileUpload file)
    {
        return Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
    }
}
