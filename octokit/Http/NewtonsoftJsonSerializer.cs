using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Octokit.Reflection;

namespace Octokit.Internal
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftJsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
            _settings.Converters.Add(new StringEnumConverter());
            _settings.Converters.Add(new StringEnumContentTypeConverter());
            _settings.Converters.Add(new RepositoryContentConverter());
            _settings.Converters.Add(new RepositoryContentListConverter());
            _settings.Converters.Add(new RepositoryConverter());
            _settings.Converters.Add(new BranchConverter());
            _settings.Converters.Add(new BranchListConverter());
            _settings.Converters.Add(new RepositoryTagConverter());
            _settings.Converters.Add(new RepositoryTagListConverter());
            _settings.Converters.Add(new ReleaseConverter());
            _settings.Converters.Add(new ReleaseListConverter());
            _settings.Converters.Add(new ReleaseAssetConverter());
            _settings.Converters.Add(new GitHubCommitConverter());
            _settings.Converters.Add(new GitHubCommitListConverter());
            _settings.Converters.Add(new GitReferenceConverter());
            _settings.Converters.Add(new UserConverter());
            _settings.Converters.Add(new AuthorConverter());
            _settings.Converters.Add(new CommitConverter());
            _settings.Converters.Add(new CommitterConverter());
            _settings.Converters.Add(new VerificationConverter());
            _settings.Converters.Add(new GitHubCommitStatsConverter());
            _settings.Converters.Add(new GitHubCommitFileConverter());
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, _settings);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        internal static string SerializeEnum(Enum value)
        {
            return value.ToParameter();
        }

        internal static object DeserializeEnum(string value, Type type)
        {
            return Enum.Parse(type, value, ignoreCase: true);
        }
    }

    internal class RepositoryContentListConverter : JsonConverter<List<RepositoryContent>>
    {
        public override List<RepositoryContent> ReadJson(JsonReader reader, Type objectType, List<RepositoryContent> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<RepositoryContent>();
            
            if (reader.TokenType == JsonToken.Null)
                return list;

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"Expected StartArray but got {reader.TokenType}");

            reader.Read();
            
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var item = ReadRepositoryContent(reader);
                    if (item != null)
                        list.Add(item);
                }
                else
                {
                    reader.Read();
                }
            }

            return list;
        }

        private RepositoryContent ReadRepositoryContent(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string name = null;
            string path = null;
            string sha = null;
            int size = 0;
            ContentType type = ContentType.File;
            string downloadUrl = null;
            string url = null;
            string gitUrl = null;
            string htmlUrl = null;
            string encoding = null;
            string encodedContent = null;
            string target = null;
            string submoduleGitUrl = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "path":
                        path = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    case "size":
                        size = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "type":
                        var typeStr = reader.Value?.ToString();
                        if (!string.IsNullOrEmpty(typeStr))
                            type = (ContentType)Enum.Parse(typeof(ContentType), typeStr, true);
                        break;
                    case "download_url":
                        downloadUrl = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "git_url":
                        gitUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "encoding":
                        encoding = reader.Value?.ToString();
                        break;
                    case "content":
                        encodedContent = reader.Value?.ToString();
                        break;
                    case "target":
                        target = reader.Value?.ToString();
                        break;
                    case "submodule_git_url":
                        submoduleGitUrl = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new RepositoryContent(name, path, sha, size, type, downloadUrl, url, gitUrl, htmlUrl, encoding, encodedContent, target, submoduleGitUrl);
        }

        public override void WriteJson(JsonWriter writer, List<RepositoryContent> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value != null)
            {
                foreach (var item in value)
                {
                    WriteRepositoryContent(writer, item);
                }
            }
            writer.WriteEndArray();
        }

        private void WriteRepositoryContent(JsonWriter writer, RepositoryContent value)
        {
            writer.WriteStartObject();
            
            if (value.Name != null)
            {
                writer.WritePropertyName("name");
                writer.WriteValue(value.Name);
            }
            
            if (value.Path != null)
            {
                writer.WritePropertyName("path");
                writer.WriteValue(value.Path);
            }
            
            if (value.Sha != null)
            {
                writer.WritePropertyName("sha");
                writer.WriteValue(value.Sha);
            }
            
            writer.WritePropertyName("size");
            writer.WriteValue(value.Size);
            
            if (value.Type != null)
            {
                writer.WritePropertyName("type");
                writer.WriteValue(value.Type.StringValue);
            }
            
            if (value.DownloadUrl != null)
            {
                writer.WritePropertyName("download_url");
                writer.WriteValue(value.DownloadUrl);
            }
            
            if (value.Url != null)
            {
                writer.WritePropertyName("url");
                writer.WriteValue(value.Url);
            }
            
            if (value.GitUrl != null)
            {
                writer.WritePropertyName("git_url");
                writer.WriteValue(value.GitUrl);
            }
            
            if (value.HtmlUrl != null)
            {
                writer.WritePropertyName("html_url");
                writer.WriteValue(value.HtmlUrl);
            }
            
            if (value.Encoding != null)
            {
                writer.WritePropertyName("encoding");
                writer.WriteValue(value.Encoding);
            }
            
            if (value.EncodedContent != null)
            {
                writer.WritePropertyName("content");
                writer.WriteValue(value.EncodedContent);
            }
            
            if (value.Target != null)
            {
                writer.WritePropertyName("target");
                writer.WriteValue(value.Target);
            }
            
            if (value.SubmoduleGitUrl != null)
            {
                writer.WritePropertyName("submodule_git_url");
                writer.WriteValue(value.SubmoduleGitUrl);
            }
            
            writer.WriteEndObject();
        }
    }

    internal class RepositoryContentConverter : JsonConverter<RepositoryContent>
    {
        public override RepositoryContent ReadJson(JsonReader reader, Type objectType, RepositoryContent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string name = null;
            string path = null;
            string sha = null;
            int size = 0;
            ContentType type = ContentType.File;
            string downloadUrl = null;
            string url = null;
            string gitUrl = null;
            string htmlUrl = null;
            string encoding = null;
            string encodedContent = null;
            string target = null;
            string submoduleGitUrl = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "path":
                        path = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    case "size":
                        size = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "type":
                        var typeStr = reader.Value?.ToString();
                        if (!string.IsNullOrEmpty(typeStr))
                            type = (ContentType)Enum.Parse(typeof(ContentType), typeStr, true);
                        break;
                    case "download_url":
                        downloadUrl = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "git_url":
                        gitUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "encoding":
                        encoding = reader.Value?.ToString();
                        break;
                    case "content":
                        encodedContent = reader.Value?.ToString();
                        break;
                    case "target":
                        target = reader.Value?.ToString();
                        break;
                    case "submodule_git_url":
                        submoduleGitUrl = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new RepositoryContent(name, path, sha, size, type, downloadUrl, url, gitUrl, htmlUrl, encoding, encodedContent, target, submoduleGitUrl);
        }

        public override void WriteJson(JsonWriter writer, RepositoryContent value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            if (value.Name != null)
            {
                writer.WritePropertyName("name");
                writer.WriteValue(value.Name);
            }
            
            if (value.Path != null)
            {
                writer.WritePropertyName("path");
                writer.WriteValue(value.Path);
            }
            
            if (value.Sha != null)
            {
                writer.WritePropertyName("sha");
                writer.WriteValue(value.Sha);
            }
            
            writer.WritePropertyName("size");
            writer.WriteValue(value.Size);
            
            if (value.Type != null)
            {
                writer.WritePropertyName("type");
                writer.WriteValue(value.Type.StringValue);
            }
            
            if (value.DownloadUrl != null)
            {
                writer.WritePropertyName("download_url");
                writer.WriteValue(value.DownloadUrl);
            }
            
            if (value.Url != null)
            {
                writer.WritePropertyName("url");
                writer.WriteValue(value.Url);
            }
            
            if (value.GitUrl != null)
            {
                writer.WritePropertyName("git_url");
                writer.WriteValue(value.GitUrl);
            }
            
            if (value.HtmlUrl != null)
            {
                writer.WritePropertyName("html_url");
                writer.WriteValue(value.HtmlUrl);
            }
            
            if (value.Encoding != null)
            {
                writer.WritePropertyName("encoding");
                writer.WriteValue(value.Encoding);
            }
            
            if (value.EncodedContent != null)
            {
                writer.WritePropertyName("content");
                writer.WriteValue(value.EncodedContent);
            }
            
            if (value.Target != null)
            {
                writer.WritePropertyName("target");
                writer.WriteValue(value.Target);
            }
            
            if (value.SubmoduleGitUrl != null)
            {
                writer.WritePropertyName("submodule_git_url");
                writer.WriteValue(value.SubmoduleGitUrl);
            }
            
            writer.WriteEndObject();
        }
    }

    internal class StringEnumContentTypeConverter : JsonConverter<StringEnum<ContentType>>
    {
        public override StringEnum<ContentType> ReadJson(JsonReader reader, Type objectType, StringEnum<ContentType> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var stringValue = reader.Value?.ToString();
            return new StringEnum<ContentType>(stringValue);
        }

        public override void WriteJson(JsonWriter writer, StringEnum<ContentType> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.StringValue);
        }
    }

    #region Repository Converter

    internal class RepositoryConverter : JsonConverter<Repository>
    {
        public override Repository ReadJson(JsonReader reader, Type objectType, Repository existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            long id = 0;
            string name = null;
            string fullName = null;
            string defaultBranch = null;
            bool isPrivate = false;
            string description = null;
            string htmlUrl = null;
            string cloneUrl = null;
            string gitUrl = null;
            string sshUrl = null;
            string svnUrl = null;
            User owner = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "id":
                        id = reader.Value != null ? Convert.ToInt64(reader.Value) : 0;
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "full_name":
                        fullName = reader.Value?.ToString();
                        break;
                    case "default_branch":
                        defaultBranch = reader.Value?.ToString();
                        break;
                    case "private":
                        isPrivate = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    case "description":
                        description = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "clone_url":
                        cloneUrl = reader.Value?.ToString();
                        break;
                    case "git_url":
                        gitUrl = reader.Value?.ToString();
                        break;
                    case "ssh_url":
                        sshUrl = reader.Value?.ToString();
                        break;
                    case "svn_url":
                        svnUrl = reader.Value?.ToString();
                        break;
                    case "owner":
                        owner = serializer.Deserialize<User>(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            var repo = new Repository(id);
            SetRepositoryProperty(repo, "Name", name);
            SetRepositoryProperty(repo, "FullName", fullName);
            SetRepositoryProperty(repo, "DefaultBranch", defaultBranch);
            SetRepositoryProperty(repo, "Private", (bool?)isPrivate);
            SetRepositoryProperty(repo, "Description", description);
            SetRepositoryProperty(repo, "HtmlUrl", htmlUrl);
            SetRepositoryProperty(repo, "CloneUrl", cloneUrl);
            SetRepositoryProperty(repo, "GitUrl", gitUrl);
            SetRepositoryProperty(repo, "SshUrl", sshUrl);
            SetRepositoryProperty(repo, "SvnUrl", svnUrl);
            SetRepositoryProperty(repo, "Owner", owner);
            return repo;
        }

        private static void SetRepositoryProperty(Repository repo, string propertyName, object value)
        {
            if (value == null) return;
            var prop = typeof(Repository).GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null)
            {
                prop.SetValue(repo, value);
            }
        }

        public override void WriteJson(JsonWriter writer, Repository value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            if (value.FullName != null) { writer.WritePropertyName("full_name"); writer.WriteValue(value.FullName); }
            if (value.DefaultBranch != null) { writer.WritePropertyName("default_branch"); writer.WriteValue(value.DefaultBranch); }
            writer.WritePropertyName("private");
            writer.WriteValue(value.Private);
            writer.WriteEndObject();
        }
    }

    #endregion

    #region Branch Converters

    internal class BranchConverter : JsonConverter<Branch>
    {
        public override Branch ReadJson(JsonReader reader, Type objectType, Branch existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string name = null;
            GitReference commit = null;
            bool isProtected = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "commit":
                        commit = ReadGitReferenceForBranch(reader);
                        break;
                    case "protected":
                        isProtected = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Branch(name, commit, isProtected);
        }

        private static GitReference ReadGitReferenceForBranch(JsonReader reader)
        {
            string nodeId = null;
            string url = null;
            string label = null;
            string @ref = null;
            string sha = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "ref":
                        @ref = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitReference(nodeId, url, label, @ref, sha, null, null);
        }

        public override void WriteJson(JsonWriter writer, Branch value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            if (value.Commit != null) { writer.WritePropertyName("commit"); serializer.Serialize(writer, value.Commit); }
            writer.WritePropertyName("protected");
            writer.WriteValue(value.Protected);
            writer.WriteEndObject();
        }
    }

    internal class BranchListConverter : JsonConverter<List<Branch>>
    {
        public override List<Branch> ReadJson(JsonReader reader, Type objectType, List<Branch> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<Branch>();
            if (reader.TokenType == JsonToken.Null)
                return list;

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"Expected StartArray but got {reader.TokenType}");

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var item = serializer.Deserialize<Branch>(reader);
                    if (item != null)
                        list.Add(item);
                }
                reader.Read();
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<Branch> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value != null)
            {
                foreach (var item in value)
                {
                    serializer.Serialize(writer, item);
                }
            }
            writer.WriteEndArray();
        }
    }

    #endregion

    #region RepositoryTag Converters

    internal class RepositoryTagConverter : JsonConverter<RepositoryTag>
    {
        public override RepositoryTag ReadJson(JsonReader reader, Type objectType, RepositoryTag existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string name = null;
            string nodeId = null;
            GitReference commit = null;
            string zipballUrl = null;
            string tarballUrl = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "commit":
                        commit = ReadGitReferenceForTag(reader);
                        break;
                    case "zipball_url":
                        zipballUrl = reader.Value?.ToString();
                        break;
                    case "tarball_url":
                        tarballUrl = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new RepositoryTag(name, nodeId, commit, zipballUrl, tarballUrl);
        }

        private static GitReference ReadGitReferenceForTag(JsonReader reader)
        {
            string nodeId = null;
            string url = null;
            string label = null;
            string @ref = null;
            string sha = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "ref":
                        @ref = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitReference(nodeId, url, label, @ref, sha, null, null);
        }

        public override void WriteJson(JsonWriter writer, RepositoryTag value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            if (value.NodeId != null) { writer.WritePropertyName("node_id"); writer.WriteValue(value.NodeId); }
            if (value.Commit != null) { writer.WritePropertyName("commit"); serializer.Serialize(writer, value.Commit); }
            if (value.ZipballUrl != null) { writer.WritePropertyName("zipball_url"); writer.WriteValue(value.ZipballUrl); }
            if (value.TarballUrl != null) { writer.WritePropertyName("tarball_url"); writer.WriteValue(value.TarballUrl); }
            writer.WriteEndObject();
        }
    }

    internal class RepositoryTagListConverter : JsonConverter<List<RepositoryTag>>
    {
        private static readonly RepositoryTagConverter _tagConverter = new RepositoryTagConverter();

        public override List<RepositoryTag> ReadJson(JsonReader reader, Type objectType, List<RepositoryTag> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<RepositoryTag>();
            if (reader.TokenType == JsonToken.Null)
                return list;

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"Expected StartArray but got {reader.TokenType}");

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var item = _tagConverter.ReadJson(reader, typeof(RepositoryTag), null, false, serializer);
                    if (item != null)
                        list.Add(item);
                }
                reader.Read();
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<RepositoryTag> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value != null)
            {
                foreach (var item in value)
                {
                    serializer.Serialize(writer, item);
                }
            }
            writer.WriteEndArray();
        }
    }

    #endregion

    #region Release Converters

    internal class ReleaseConverter : JsonConverter<Release>
    {
        public override Release ReadJson(JsonReader reader, Type objectType, Release existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string url = null;
            string htmlUrl = null;
            string assetsUrl = null;
            string uploadUrl = null;
            long id = 0;
            string nodeId = null;
            string tagName = null;
            string targetCommitish = null;
            string name = null;
            string body = null;
            bool draft = false;
            bool prerelease = false;
            DateTimeOffset createdAt = default;
            DateTimeOffset? publishedAt = null;
            Author author = null;
            string tarballUrl = null;
            string zipballUrl = null;
            List<ReleaseAsset> assets = new List<ReleaseAsset>();

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "assets_url":
                        assetsUrl = reader.Value?.ToString();
                        break;
                    case "upload_url":
                        uploadUrl = reader.Value?.ToString();
                        break;
                    case "id":
                        id = reader.Value != null ? Convert.ToInt64(reader.Value) : 0;
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "tag_name":
                        tagName = reader.Value?.ToString();
                        break;
                    case "target_commitish":
                        targetCommitish = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "body":
                        body = reader.Value?.ToString();
                        break;
                    case "draft":
                        draft = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    case "prerelease":
                        prerelease = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    case "created_at":
                        if (reader.Value != null)
                            createdAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "published_at":
                        if (reader.Value != null)
                            publishedAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "author":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            author = ReadAuthorForRelease(reader);
                        }
                        break;
                    case "tarball_url":
                        tarballUrl = reader.Value?.ToString();
                        break;
                    case "zipball_url":
                        zipballUrl = reader.Value?.ToString();
                        break;
                    case "assets":
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var asset = ReadReleaseAsset(reader);
                                    if (asset != null)
                                        assets.Add(asset);
                                }
                                reader.Read();
                            }
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Release(url, htmlUrl, assetsUrl, uploadUrl, id, nodeId, tagName, targetCommitish, name, body, draft, prerelease, createdAt, publishedAt, author, tarballUrl, zipballUrl, assets);
        }

        private static Author ReadAuthorForRelease(JsonReader reader)
        {
            string login = null;
            long id = 0;
            string nodeId = null;
            string avatarUrl = null;
            string htmlUrl = null;
            string type = null;
            bool siteAdmin = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "login":
                        login = reader.Value?.ToString();
                        break;
                    case "id":
                        if (reader.Value != null)
                            id = Convert.ToInt64(reader.Value);
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "avatar_url":
                        avatarUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "type":
                        type = reader.Value?.ToString();
                        break;
                    case "site_admin":
                        if (reader.Value != null)
                            siteAdmin = Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Author(login, id, nodeId, avatarUrl, htmlUrl, null, null, null, null, type, null, null, null, null, null, null, siteAdmin);
        }

        private static ReleaseAsset ReadReleaseAsset(JsonReader reader)
        {
            string url = null;
            int id = 0;
            string nodeId = null;
            string name = null;
            string label = null;
            string state = null;
            string contentType = null;
            int size = 0;
            int downloadCount = 0;
            DateTimeOffset createdAt = default;
            DateTimeOffset updatedAt = default;
            string browserDownloadUrl = null;
            Author uploader = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "id":
                        if (reader.Value != null)
                            id = Convert.ToInt32(reader.Value);
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "state":
                        state = reader.Value?.ToString();
                        break;
                    case "content_type":
                        contentType = reader.Value?.ToString();
                        break;
                    case "size":
                        if (reader.Value != null)
                            size = Convert.ToInt32(reader.Value);
                        break;
                    case "download_count":
                        if (reader.Value != null)
                            downloadCount = Convert.ToInt32(reader.Value);
                        break;
                    case "created_at":
                        if (reader.Value != null)
                            createdAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "updated_at":
                        if (reader.Value != null)
                            updatedAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "browser_download_url":
                        browserDownloadUrl = reader.Value?.ToString();
                        break;
                    case "uploader":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            uploader = ReadAuthorForRelease(reader);
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new ReleaseAsset(url, id, nodeId, name, label, state, contentType, size, downloadCount, createdAt, updatedAt, browserDownloadUrl, uploader);
        }

        public override void WriteJson(JsonWriter writer, Release value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Url != null) { writer.WritePropertyName("url"); writer.WriteValue(value.Url); }
            if (value.HtmlUrl != null) { writer.WritePropertyName("html_url"); writer.WriteValue(value.HtmlUrl); }
            if (value.TagName != null) { writer.WritePropertyName("tag_name"); writer.WriteValue(value.TagName); }
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            writer.WriteEndObject();
        }
    }

    internal class ReleaseListConverter : JsonConverter<List<Release>>
    {
        private static readonly ReleaseConverter _releaseConverter = new ReleaseConverter();

        public override List<Release> ReadJson(JsonReader reader, Type objectType, List<Release> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<Release>();
            if (reader.TokenType == JsonToken.Null)
                return list;

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"Expected StartArray but got {reader.TokenType}");

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var item = _releaseConverter.ReadJson(reader, typeof(Release), null, false, serializer);
                    if (item != null)
                        list.Add(item);
                }
                reader.Read();
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<Release> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value != null)
            {
                foreach (var item in value)
                {
                    serializer.Serialize(writer, item);
                }
            }
            writer.WriteEndArray();
        }
    }

    #endregion

    #region GitHubCommit Converters

    internal class GitHubCommitConverter : JsonConverter<GitHubCommit>
    {
        public override GitHubCommit ReadJson(JsonReader reader, Type objectType, GitHubCommit existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string nodeId = null;
            string url = null;
            string sha = null;
            Author author = null;
            Author committer = null;
            string htmlUrl = null;
            Commit commit = null;
            List<GitReference> parents = new List<GitReference>();
            GitHubCommitStats stats = null;
            List<GitHubCommitFile> files = new List<GitHubCommitFile>();

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    case "author":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            author = ReadAuthor(reader);
                        }
                        break;
                    case "committer":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            committer = ReadAuthor(reader);
                        }
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "commit":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            commit = ReadCommit(reader);
                        }
                        break;
                    case "parents":
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var parent = ReadGitReference(reader);
                                    if (parent != null)
                                        parents.Add(parent);
                                }
                                reader.Read();
                            }
                        }
                        break;
                    case "stats":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            stats = ReadGitHubCommitStats(reader);
                        }
                        break;
                    case "files":
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var file = ReadGitHubCommitFile(reader);
                                    if (file != null)
                                        files.Add(file);
                                }
                                reader.Read();
                            }
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitHubCommit(nodeId, url, null, null, sha, null, null, author, null, commit, committer, htmlUrl, stats, parents, files);
        }

        private static Author ReadAuthor(JsonReader reader)
        {
            string login = null;
            long id = 0;
            string nodeId = null;
            string avatarUrl = null;
            string htmlUrl = null;
            string type = null;
            bool siteAdmin = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "login":
                        login = reader.Value?.ToString();
                        break;
                    case "id":
                        if (reader.Value != null)
                            id = Convert.ToInt64(reader.Value);
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "avatar_url":
                        avatarUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "type":
                        type = reader.Value?.ToString();
                        break;
                    case "site_admin":
                        if (reader.Value != null)
                            siteAdmin = Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Author(login, id, nodeId, avatarUrl, htmlUrl, null, null, null, null, type, null, null, null, null, null, null, siteAdmin);
        }

        private static Commit ReadCommit(JsonReader reader)
        {
            string message = null;
            Committer author = null;
            Committer committer = null;
            GitReference tree = null;
            List<GitReference> parents = new List<GitReference>();
            int commentCount = 0;
            Verification verification = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "message":
                        message = reader.Value?.ToString();
                        break;
                    case "author":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            author = ReadCommitterForCommit(reader);
                        }
                        break;
                    case "committer":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            committer = ReadCommitterForCommit(reader);
                        }
                        break;
                    case "tree":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            tree = ReadGitReference(reader);
                        }
                        break;
                    case "parents":
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var parent = ReadGitReference(reader);
                                    if (parent != null)
                                        parents.Add(parent);
                                }
                                reader.Read();
                            }
                        }
                        break;
                    case "comment_count":
                        if (reader.Value != null)
                            commentCount = Convert.ToInt32(reader.Value);
                        break;
                    case "verification":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            verification = ReadVerificationForCommit(reader);
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Commit(null, null, null, null, null, null, null, message, author, committer, tree, parents, commentCount, verification);
        }

        private static Committer ReadCommitterForCommit(JsonReader reader)
        {
            string nodeId = null;
            string name = null;
            string email = null;
            DateTimeOffset date = default;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "email":
                        email = reader.Value?.ToString();
                        break;
                    case "date":
                        if (reader.Value != null)
                            date = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            if (!string.IsNullOrEmpty(nodeId))
                return new Committer(nodeId, name, email, date);
            return new Committer(name, email, date);
        }

        private static Verification ReadVerificationForCommit(JsonReader reader)
        {
            bool verified = false;
            string reason = null;
            string signature = null;
            string payload = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "verified":
                        if (reader.Value != null)
                            verified = Convert.ToBoolean(reader.Value);
                        break;
                    case "reason":
                        reason = reader.Value?.ToString();
                        break;
                    case "signature":
                        signature = reader.Value?.ToString();
                        break;
                    case "payload":
                        payload = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Verification(verified, new StringEnum<VerificationReason>(reason ?? "unsigned"), signature, payload);
        }

        private static GitReference ReadGitReference(JsonReader reader)
        {
            string nodeId = null;
            string url = null;
            string label = null;
            string @ref = null;
            string sha = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "ref":
                        @ref = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitReference(nodeId, url, label, @ref, sha, null, null);
        }

        private static GitHubCommitStats ReadGitHubCommitStats(JsonReader reader)
        {
            int additions = 0;
            int deletions = 0;
            int total = 0;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "additions":
                        if (reader.Value != null)
                            additions = Convert.ToInt32(reader.Value);
                        break;
                    case "deletions":
                        if (reader.Value != null)
                            deletions = Convert.ToInt32(reader.Value);
                        break;
                    case "total":
                        if (reader.Value != null)
                            total = Convert.ToInt32(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitHubCommitStats(additions, deletions, total);
        }

        private static GitHubCommitFile ReadGitHubCommitFile(JsonReader reader)
        {
            string filename = null;
            int additions = 0;
            int deletions = 0;
            int changes = 0;
            string status = null;
            string blobUrl = null;
            string contentsUrl = null;
            string rawUrl = null;
            string sha = null;
            string patch = null;
            string previousFileName = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "filename":
                        filename = reader.Value?.ToString();
                        break;
                    case "additions":
                        if (reader.Value != null)
                            additions = Convert.ToInt32(reader.Value);
                        break;
                    case "deletions":
                        if (reader.Value != null)
                            deletions = Convert.ToInt32(reader.Value);
                        break;
                    case "changes":
                        if (reader.Value != null)
                            changes = Convert.ToInt32(reader.Value);
                        break;
                    case "status":
                        status = reader.Value?.ToString();
                        break;
                    case "blob_url":
                        blobUrl = reader.Value?.ToString();
                        break;
                    case "contents_url":
                        contentsUrl = reader.Value?.ToString();
                        break;
                    case "raw_url":
                        rawUrl = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    case "patch":
                        patch = reader.Value?.ToString();
                        break;
                    case "previous_filename":
                        previousFileName = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitHubCommitFile(filename, additions, deletions, changes, status, blobUrl, contentsUrl, rawUrl, sha, patch, previousFileName);
        }

        public override void WriteJson(JsonWriter writer, GitHubCommit value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Sha != null) { writer.WritePropertyName("sha"); writer.WriteValue(value.Sha); }
            if (value.HtmlUrl != null) { writer.WritePropertyName("html_url"); writer.WriteValue(value.HtmlUrl); }
            writer.WriteEndObject();
        }
    }

    internal class GitHubCommitListConverter : JsonConverter<List<GitHubCommit>>
    {
        private static readonly GitHubCommitConverter _commitConverter = new GitHubCommitConverter();

        public override List<GitHubCommit> ReadJson(JsonReader reader, Type objectType, List<GitHubCommit> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<GitHubCommit>();
            if (reader.TokenType == JsonToken.Null)
                return list;

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"Expected StartArray but got {reader.TokenType}");

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var item = _commitConverter.ReadJson(reader, typeof(GitHubCommit), null, false, serializer);
                    if (item != null)
                        list.Add(item);
                }
                reader.Read();
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<GitHubCommit> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value != null)
            {
                foreach (var item in value)
                {
                    serializer.Serialize(writer, item);
                }
            }
            writer.WriteEndArray();
        }
    }

    #endregion

    #region GitReference Converter

    internal class GitReferenceConverter : JsonConverter<GitReference>
    {
        public override GitReference ReadJson(JsonReader reader, Type objectType, GitReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string nodeId = null;
            string url = null;
            string label = null;
            string @ref = null;
            string sha = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "ref":
                        @ref = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitReference(nodeId, url, label, @ref, sha, null, null);
        }

        public override void WriteJson(JsonWriter writer, GitReference value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Sha != null) { writer.WritePropertyName("sha"); writer.WriteValue(value.Sha); }
            if (value.Url != null) { writer.WritePropertyName("url"); writer.WriteValue(value.Url); }
            writer.WriteEndObject();
        }
    }

    #endregion

    #region User Converter

    internal class UserConverter : JsonConverter<User>
    {
        public override User ReadJson(JsonReader reader, Type objectType, User existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string login = null;
            long id = 0;
            string nodeId = null;
            string avatarUrl = null;
            string htmlUrl = null;
            string type = null;
            bool siteAdmin = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "login":
                        login = reader.Value?.ToString();
                        break;
                    case "id":
                        id = reader.Value != null ? Convert.ToInt64(reader.Value) : 0;
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "avatar_url":
                        avatarUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "type":
                        type = reader.Value?.ToString();
                        break;
                    case "site_admin":
                        siteAdmin = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            var user = new User();
            SetUserProperty(user, "Login", login);
            SetUserProperty(user, "Id", id);
            SetUserProperty(user, "NodeId", nodeId);
            SetUserProperty(user, "AvatarUrl", avatarUrl);
            SetUserProperty(user, "HtmlUrl", htmlUrl);
            SetUserProperty(user, "SiteAdmin", siteAdmin);
            return user;
        }

        private static void SetUserProperty(User user, string propertyName, object value)
        {
            if (value == null) return;
            var prop = typeof(User).GetProperty(propertyName);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(user, value);
            }
            else
            {
                var baseProp = typeof(User).BaseType?.GetProperty(propertyName);
                if (baseProp != null && baseProp.CanWrite)
                {
                    baseProp.SetValue(user, value);
                }
            }
        }

        public override void WriteJson(JsonWriter writer, User value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Login != null) { writer.WritePropertyName("login"); writer.WriteValue(value.Login); }
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);
            writer.WriteEndObject();
        }
    }

    #endregion

    #region Author Converter

    internal class AuthorConverter : JsonConverter<Author>
    {
        public override Author ReadJson(JsonReader reader, Type objectType, Author existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string login = null;
            long id = 0;
            string nodeId = null;
            string avatarUrl = null;
            string htmlUrl = null;
            string type = null;
            bool siteAdmin = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "login":
                        login = reader.Value?.ToString();
                        break;
                    case "id":
                        id = reader.Value != null ? Convert.ToInt64(reader.Value) : 0;
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "avatar_url":
                        avatarUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "type":
                        type = reader.Value?.ToString();
                        break;
                    case "site_admin":
                        siteAdmin = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Author(login, id, nodeId, avatarUrl, htmlUrl, null, null, null, null, type, null, null, null, null, null, null, siteAdmin);
        }

        public override void WriteJson(JsonWriter writer, Author value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Login != null) { writer.WritePropertyName("login"); writer.WriteValue(value.Login); }
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);
            writer.WriteEndObject();
        }
    }

    #endregion

    #region Commit Converter

    internal class CommitConverter : JsonConverter<Commit>
    {
        public override Commit ReadJson(JsonReader reader, Type objectType, Commit existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string message = null;
            Committer author = null;
            Committer committer = null;
            GitReference tree = null;
            List<GitReference> parents = new List<GitReference>();
            int commentCount = 0;
            Verification verification = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "message":
                        message = reader.Value?.ToString();
                        break;
                    case "author":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            author = ReadCommitter(reader);
                        }
                        break;
                    case "committer":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            committer = ReadCommitter(reader);
                        }
                        break;
                    case "tree":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            tree = ReadGitReferenceForCommit(reader);
                        }
                        break;
                    case "parents":
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var parent = ReadGitReferenceForCommit(reader);
                                    if (parent != null)
                                        parents.Add(parent);
                                }
                                reader.Read();
                            }
                        }
                        break;
                    case "comment_count":
                        if (reader.Value != null)
                            commentCount = Convert.ToInt32(reader.Value);
                        break;
                    case "verification":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            verification = ReadVerificationForCommit(reader);
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Commit(null, null, null, null, null, null, null, message, author, committer, tree, parents, commentCount, verification);
        }

        private static Committer ReadCommitter(JsonReader reader)
        {
            string nodeId = null;
            string name = null;
            string email = null;
            DateTimeOffset date = default;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "email":
                        email = reader.Value?.ToString();
                        break;
                    case "date":
                        if (reader.Value != null)
                            date = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            if (!string.IsNullOrEmpty(nodeId))
                return new Committer(nodeId, name, email, date);
            return new Committer(name, email, date);
        }

        private static GitReference ReadGitReferenceForCommit(JsonReader reader)
        {
            string nodeId = null;
            string url = null;
            string label = null;
            string @ref = null;
            string sha = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "ref":
                        @ref = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitReference(nodeId, url, label, @ref, sha, null, null);
        }

        private static Verification ReadVerificationForCommit(JsonReader reader)
        {
            bool verified = false;
            string reason = null;
            string signature = null;
            string payload = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "verified":
                        if (reader.Value != null)
                            verified = Convert.ToBoolean(reader.Value);
                        break;
                    case "reason":
                        reason = reader.Value?.ToString();
                        break;
                    case "signature":
                        signature = reader.Value?.ToString();
                        break;
                    case "payload":
                        payload = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Verification(verified, new StringEnum<VerificationReason>(reason ?? "unsigned"), signature, payload);
        }

        public override void WriteJson(JsonWriter writer, Commit value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Message != null) { writer.WritePropertyName("message"); writer.WriteValue(value.Message); }
            writer.WriteEndObject();
        }
    }

    #endregion

    #region Verification Converter

    internal class VerificationConverter : JsonConverter<Verification>
    {
        public override Verification ReadJson(JsonReader reader, Type objectType, Verification existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            bool verified = false;
            string reason = null;
            string signature = null;
            string payload = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "verified":
                        verified = reader.Value != null && Convert.ToBoolean(reader.Value);
                        break;
                    case "reason":
                        reason = reader.Value?.ToString();
                        break;
                    case "signature":
                        signature = reader.Value?.ToString();
                        break;
                    case "payload":
                        payload = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Verification(verified, new StringEnum<VerificationReason>(reason ?? "unsigned"), signature, payload);
        }

        public override void WriteJson(JsonWriter writer, Verification value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("verified");
            writer.WriteValue(value.Verified);
            writer.WriteEndObject();
        }
    }

    #endregion

    #region Committer Converter

    internal class CommitterConverter : JsonConverter<Committer>
    {
        public override Committer ReadJson(JsonReader reader, Type objectType, Committer existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string nodeId = null;
            string name = null;
            string email = null;
            DateTimeOffset date = default;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "email":
                        email = reader.Value?.ToString();
                        break;
                    case "date":
                        if (reader.Value != null)
                            date = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            if (!string.IsNullOrEmpty(nodeId))
                return new Committer(nodeId, name, email, date);
            return new Committer(name, email, date);
        }

        public override void WriteJson(JsonWriter writer, Committer value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            if (value.Email != null) { writer.WritePropertyName("email"); writer.WriteValue(value.Email); }
            writer.WriteEndObject();
        }
    }

    #endregion

    #region ReleaseAsset Converter

    internal class ReleaseAssetConverter : JsonConverter<ReleaseAsset>
    {
        public override ReleaseAsset ReadJson(JsonReader reader, Type objectType, ReleaseAsset existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string url = null;
            int id = 0;
            string nodeId = null;
            string name = null;
            string label = null;
            string state = null;
            string contentType = null;
            int size = 0;
            int downloadCount = 0;
            DateTimeOffset createdAt = default;
            DateTimeOffset updatedAt = default;
            string browserDownloadUrl = null;
            Author uploader = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "url":
                        url = reader.Value?.ToString();
                        break;
                    case "id":
                        id = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "name":
                        name = reader.Value?.ToString();
                        break;
                    case "label":
                        label = reader.Value?.ToString();
                        break;
                    case "state":
                        state = reader.Value?.ToString();
                        break;
                    case "content_type":
                        contentType = reader.Value?.ToString();
                        break;
                    case "size":
                        size = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "download_count":
                        downloadCount = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
                        break;
                    case "created_at":
                        if (reader.Value != null)
                            createdAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "updated_at":
                        if (reader.Value != null)
                            updatedAt = DateTimeOffset.Parse(reader.Value.ToString());
                        break;
                    case "browser_download_url":
                        browserDownloadUrl = reader.Value?.ToString();
                        break;
                    case "uploader":
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            uploader = ReadAuthorForAsset(reader);
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new ReleaseAsset(url, id, nodeId, name, label, state, contentType, size, downloadCount, createdAt, updatedAt, browserDownloadUrl, uploader);
        }

        private static Author ReadAuthorForAsset(JsonReader reader)
        {
            string login = null;
            long id = 0;
            string nodeId = null;
            string avatarUrl = null;
            string htmlUrl = null;
            string type = null;
            bool siteAdmin = false;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propName = reader.Value?.ToString();
                reader.Read();

                switch (propName)
                {
                    case "login":
                        login = reader.Value?.ToString();
                        break;
                    case "id":
                        if (reader.Value != null)
                            id = Convert.ToInt64(reader.Value);
                        break;
                    case "node_id":
                        nodeId = reader.Value?.ToString();
                        break;
                    case "avatar_url":
                        avatarUrl = reader.Value?.ToString();
                        break;
                    case "html_url":
                        htmlUrl = reader.Value?.ToString();
                        break;
                    case "type":
                        type = reader.Value?.ToString();
                        break;
                    case "site_admin":
                        if (reader.Value != null)
                            siteAdmin = Convert.ToBoolean(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new Author(login, id, nodeId, avatarUrl, htmlUrl, null, null, null, null, type, null, null, null, null, null, null, siteAdmin);
        }

        public override void WriteJson(JsonWriter writer, ReleaseAsset value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Name != null) { writer.WritePropertyName("name"); writer.WriteValue(value.Name); }
            if (value.BrowserDownloadUrl != null) { writer.WritePropertyName("browser_download_url"); writer.WriteValue(value.BrowserDownloadUrl); }
            writer.WriteEndObject();
        }
    }

    #endregion

    #region GitHubCommitStats Converter

    internal class GitHubCommitStatsConverter : JsonConverter<GitHubCommitStats>
    {
        public override GitHubCommitStats ReadJson(JsonReader reader, Type objectType, GitHubCommitStats existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            int additions = 0;
            int deletions = 0;
            int total = 0;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "additions":
                        if (reader.Value != null)
                            additions = Convert.ToInt32(reader.Value);
                        break;
                    case "deletions":
                        if (reader.Value != null)
                            deletions = Convert.ToInt32(reader.Value);
                        break;
                    case "total":
                        if (reader.Value != null)
                            total = Convert.ToInt32(reader.Value);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitHubCommitStats(additions, deletions, total);
        }

        public override void WriteJson(JsonWriter writer, GitHubCommitStats value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("additions");
            writer.WriteValue(value.Additions);
            writer.WritePropertyName("deletions");
            writer.WriteValue(value.Deletions);
            writer.WritePropertyName("total");
            writer.WriteValue(value.Total);
            writer.WriteEndObject();
        }
    }

    #endregion

    #region GitHubCommitFile Converter

    internal class GitHubCommitFileConverter : JsonConverter<GitHubCommitFile>
    {
        public override GitHubCommitFile ReadJson(JsonReader reader, Type objectType, GitHubCommitFile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"Expected StartObject but got {reader.TokenType}");

            string filename = null;
            int additions = 0;
            int deletions = 0;
            int changes = 0;
            string status = null;
            string blobUrl = null;
            string contentsUrl = null;
            string rawUrl = null;
            string sha = null;
            string patch = null;
            string previousFileName = null;

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value?.ToString();
                reader.Read();

                switch (propertyName)
                {
                    case "filename":
                        filename = reader.Value?.ToString();
                        break;
                    case "additions":
                        if (reader.Value != null)
                            additions = Convert.ToInt32(reader.Value);
                        break;
                    case "deletions":
                        if (reader.Value != null)
                            deletions = Convert.ToInt32(reader.Value);
                        break;
                    case "changes":
                        if (reader.Value != null)
                            changes = Convert.ToInt32(reader.Value);
                        break;
                    case "status":
                        status = reader.Value?.ToString();
                        break;
                    case "blob_url":
                        blobUrl = reader.Value?.ToString();
                        break;
                    case "contents_url":
                        contentsUrl = reader.Value?.ToString();
                        break;
                    case "raw_url":
                        rawUrl = reader.Value?.ToString();
                        break;
                    case "sha":
                        sha = reader.Value?.ToString();
                        break;
                    case "patch":
                        patch = reader.Value?.ToString();
                        break;
                    case "previous_filename":
                        previousFileName = reader.Value?.ToString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }

                reader.Read();
            }

            return new GitHubCommitFile(filename, additions, deletions, changes, status, blobUrl, contentsUrl, rawUrl, sha, patch, previousFileName);
        }

        public override void WriteJson(JsonWriter writer, GitHubCommitFile value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.Filename != null) { writer.WritePropertyName("filename"); writer.WriteValue(value.Filename); }
            writer.WriteEndObject();
        }
    }

    #endregion
}
