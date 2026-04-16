using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Octokit
{
    /// <summary>
    /// A client for GitHub's Dependency Submission API.
    /// </summary>
    /// <remarks>
    /// See the <a href="https://docs.github.com/rest/dependency-graph/dependency-submission">Dependency Submission API documentation</a> for more details.
    /// </remarks>
    public class DependencySubmissionClient : ApiClient, IDependencySubmissionClient
    {
        /// <summary>
        /// Initializes a new GitHub Dependency Submission API client.
        /// </summary>
        /// <param name="apiConnection">An API connection</param>
        public DependencySubmissionClient(IApiConnection apiConnection) : base(apiConnection) { }

        /// <summary>
        /// Creates a new dependency snapshot.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://docs.github.com/rest/dependency-graph/dependency-submission">API documentation</a> for more information.
        /// </remarks>
        /// <param name="owner">The repository's owner</param>
        /// <param name="name">The repository's name</param>
        /// <param name="snapshot">The dependency snapshot to create</param>
        /// <exception cref="ApiException">Thrown when a general API error occurs</exception>
        /// <returns>A <see cref="DependencySnapshotSubmission"/> instance for the created snapshot</returns>
        [ManualRoute("POST", "/repos/{owner}/{repo}/dependency-graph/snapshots")]
        public Task<DependencySnapshotSubmission> Create(string owner, string name, NewDependencySnapshot snapshot)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, nameof(owner));
            Ensure.ArgumentNotNullOrEmptyString(name, nameof(name));
            Ensure.ArgumentNotNull(snapshot, nameof(snapshot));

            var newDependencySnapshotAsObject = ConvertToJsonObject(snapshot);

            return ApiConnection.Post<DependencySnapshotSubmission>(ApiUrls.DependencySubmission(owner, name), newDependencySnapshotAsObject);
        }

        /// <summary>
        /// Creates a new dependency snapshot.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://docs.github.com/rest/dependency-graph/dependency-submission">API documentation</a> for more information.
        /// </remarks>
        /// <param name="repositoryId">The Id of the repository</param>
        /// <param name="snapshot">The dependency snapshot to create</param>
        /// <exception cref="ApiException">Thrown when a general API error occurs</exception>
        /// <returns>A <see cref="DependencySnapshotSubmission"/> instance for the created snapshot</returns>
        [ManualRoute("POST", "/repositories/{id}/dependency-graph/snapshots")]
        public Task<DependencySnapshotSubmission> Create(long repositoryId, NewDependencySnapshot snapshot)
        {
            Ensure.ArgumentNotNull(snapshot, nameof(snapshot));

            var newDependencySnapshotAsObject = ConvertToJsonObject(snapshot);

            return ApiConnection.Post<DependencySnapshotSubmission>(ApiUrls.DependencySubmission(repositoryId), newDependencySnapshotAsObject);
        }

        /// <summary>
        /// Dependency snapshots dictionaries such as Manifests need to be passed as JObject in order to be serialized correctly
        /// </summary>
        private JObject ConvertToJsonObject(NewDependencySnapshot snapshot)
        {
            var newSnapshotAsObject = new JObject();
            newSnapshotAsObject["version"] = snapshot.Version;
            newSnapshotAsObject["sha"] = snapshot.Sha;
            newSnapshotAsObject["ref"] = snapshot.Ref;
            newSnapshotAsObject["scanned"] = snapshot.Scanned;
            newSnapshotAsObject["job"] = JToken.FromObject(snapshot.Job);
            newSnapshotAsObject["detector"] = JToken.FromObject(snapshot.Detector);

            if (snapshot.Metadata != null)
            {
                var metadataAsObject = new JObject();
                foreach (var kvp in snapshot.Metadata)
                {
                    metadataAsObject[kvp.Key] = JToken.FromObject(kvp.Value);
                }

                newSnapshotAsObject["metadata"] = metadataAsObject;
            }

            if (snapshot.Manifests != null)
            {
                var manifestsAsObject = new JObject();
                foreach (var manifestKvp in snapshot.Manifests)
                {
                    var manifest = manifestKvp.Value;

                    var manifestAsObject = new JObject();
                    manifestAsObject["name"] = manifest.Name;

                    if (manifest.File.SourceLocation != null)
                    {
                        var manifestFileAsObject = new JObject();
                        manifestFileAsObject["source_location"] = manifest.File.SourceLocation;
                        manifestAsObject["file"] = manifestFileAsObject;
                    }

                    if (manifest.Metadata != null)
                    {
                        manifestAsObject["metadata"] = JToken.FromObject(manifest.Metadata);
                    }

                    if (manifest.Resolved != null)
                    {
                        var resolvedAsObject = new JObject();
                        foreach (var resolvedKvp in manifest.Resolved)
                        {
                            resolvedAsObject[resolvedKvp.Key] = JToken.FromObject(resolvedKvp.Value);
                        }

                        manifestAsObject["resolved"] = resolvedAsObject;
                    }

                    manifestsAsObject[manifestKvp.Key] = manifestAsObject;
                }

                newSnapshotAsObject["manifests"] = manifestsAsObject;
            }

            return newSnapshotAsObject;
        }
    }
}
