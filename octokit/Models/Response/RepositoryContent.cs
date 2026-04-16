using System.Diagnostics;
using Newtonsoft.Json;
using Octokit.Internal;

namespace Octokit
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryContent : RepositoryContentInfo
    {
        public RepositoryContent() { }

        [JsonConstructor]
        public RepositoryContent(string name, string path, string sha, int size, ContentType type, string downloadUrl, string url, string gitUrl, string htmlUrl, string encoding, string encodedContent, string target, string submoduleGitUrl)
            : base(name, path, sha, size, type, downloadUrl, url, gitUrl, htmlUrl)
        {
            Encoding = encoding;
            EncodedContent = encodedContent;
            Target = target;
            SubmoduleGitUrl = submoduleGitUrl;
        }

        [JsonProperty("encoding")]
        public string Encoding { get; private set; }

        [JsonProperty("content")]
        public string EncodedContent { get; private set; }

        [JsonIgnore]
        public string Content
        {
            get
            {
                return EncodedContent != null
                    ? EncodedContent.FromBase64String()
                    : null;
            }
        }

        [JsonProperty("target")]
        public string Target { get; private set; }

        [JsonProperty("submodule_git_url")]
        public string SubmoduleGitUrl { get; private set; }
    }
}
