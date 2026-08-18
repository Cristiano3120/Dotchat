using DotchatServer.Subjects;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Interfaces;

public interface IEmailSubjectManager
{
    /// <summary>
    /// Retrieve the subject name from the <see cref="SubjectNames"/> class
    /// </summary>
    /// <param name="subjectName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public string GetSubject(string subjectName, Language language);

    /// <summary>
    /// Retrieve the subject name from the <see cref="SubjectNames"/> class
    /// </summary>
    /// <param name="subjectName"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public string GetSubject(string subjectName, string language);
}
