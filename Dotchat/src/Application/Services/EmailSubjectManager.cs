using DotchatServer.src.Application.Interfaces;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Services;

internal sealed class EmailSubjectManager(ResxManager resxManager) : IEmailSubjectManager
{
    private const string FolderName = "Subjects";
    private const string ResxFileName = "Subjects";

    public string GetSubject(string subjectName, Language language)
        => resxManager.Go(FolderName).File(ResxFileName).GetString(subjectName, language.ToString());

    public string GetSubject(string subjectName, string language)
        => resxManager.Go(FolderName).File(ResxFileName).GetString(subjectName, language);
}