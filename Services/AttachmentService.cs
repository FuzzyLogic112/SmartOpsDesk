using System.IO;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class AttachmentService
{
    private readonly string _attachmentDirectory;

    public AttachmentService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _attachmentDirectory = Path.Combine(appData, "SmartOpsDesk", "attachments");
        Directory.CreateDirectory(_attachmentDirectory);
    }

    public TicketAttachment CopyIntoStore(string sourceFile, int ticketId, string uploadedBy)
    {
        var extension = Path.GetExtension(sourceFile);
        var safeName = $"{ticketId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        var target = Path.Combine(_attachmentDirectory, safeName);
        File.Copy(sourceFile, target, overwrite: false);

        return new TicketAttachment
        {
            FileName = Path.GetFileName(sourceFile),
            StoredPath = target,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.Now
        };
    }
}
