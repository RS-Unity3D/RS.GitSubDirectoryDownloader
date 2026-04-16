using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Octokit.Internal;

namespace Octokit
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryContentInfo
    {
        public RepositoryContentInfo() { }

        [JsonConstructor]
        public RepositoryContentInfo(string name, string path, string sha, int size, ContentType type, string downloadUrl, string url, string gitUrl, string htmlUrl)
        {
            Name = name;
            Path = path;
            Sha = sha;
            Size = size;
            Type = type;
            DownloadUrl = downloadUrl;
            Url = url;
            GitUrl = gitUrl;
            HtmlUrl = htmlUrl;
        }

        [JsonProperty("name")]
        public string Name { get; protected set; }

        [JsonProperty("path")]
        public string Path { get; protected set; }

        [JsonProperty("sha")]
        public string Sha { get; protected set; }

        [JsonProperty("size")]
        public int Size { get; protected set; }

        [JsonProperty("type")]
        public StringEnum<ContentType> Type { get; protected set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; protected set; }

        [JsonProperty("url")]
        public string Url { get; protected set; }

        [JsonProperty("git_url")]
        public string GitUrl { get; protected set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; protected set; }

        internal string DebuggerDisplay => $"Name: {Name} Path: {Path} Type:{Type}";
    }
}
