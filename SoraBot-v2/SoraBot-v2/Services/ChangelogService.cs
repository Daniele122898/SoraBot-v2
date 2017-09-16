using System.IO;

namespace SoraBot_v2.Services
{
    public static class ChangelogService
    {
        private const string  _changelogFile = "CHANGELOG.txt";

        public static string GetChangelog()
        {
            return !File.Exists(_changelogFile) ? "My creator forgot to add the new changelogs.. Gomen" : File.ReadAllText(_changelogFile);
        }
    }
}