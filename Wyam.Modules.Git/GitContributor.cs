﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using LibGit2Sharp;
using System.IO;

namespace Wyam.Modules.Git
{
    public class GitContributor : GitBase
    {
        private readonly string _metadataName;

        public GitContributor(string metadataName)
        {
            _metadataName = metadataName;
        }

        public GitContributor() : this("Contributors")
        {

        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var repositoryLocation = Repository.Discover(context.InputFolder);
            if (repositoryLocation == null)
                throw new ArgumentException("No git repository found");

            using (Repository repository = new Repository(repositoryLocation))
            {

                var data = GetCommitInformation(repository);
                var lookup = data.ToLookup(x => x.Path.ToLower());
                return inputs.Select(x =>
                {
                    string relativePath = GetRelativePath(Path.GetDirectoryName(Path.GetDirectoryName(repositoryLocation.ToLower())), x.Source.ToLower()); // yes we need to do it twice
                    if (!lookup.Contains(relativePath))
                        return x;

                    var commitsOfFile = lookup[relativePath].Distinct(new SingelUserDistinction()).ToArray();
                    return x.Clone(new []
                    {
                        new KeyValuePair<string, object>(_metadataName, commitsOfFile)
                    });
                }).ToArray(); // Don't do it lazy or Commit is disposed.
            }
        }

        private string GetRelativePath(string reposotoryLocation, string source)
        {
            FilePath repositoryLocationPath = new FilePath(reposotoryLocation);
            FilePath sourcePath = new FilePath(source);
            FilePath relativePath = sourcePath.RelativeTo(repositoryLocationPath);
            return relativePath.ToWindowsPath();
        }


        private class SingelUserDistinction : IEqualityComparer<CommitInformation>
        {
            public bool Equals(CommitInformation x, CommitInformation y)
            {
                return x.Author.Name == y.Author.Name;
            }

            public int GetHashCode(CommitInformation obj)
            {
                return obj.Author.Name.GetHashCode();
            }
        }

    }
}
