﻿using System;
using System.Collections.Generic;
using System.IO;
using Rant;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.TextGeneration
{
    public abstract class RantModule : ContentModule
    {
        private RantEngine _engine;
        private long? _seed;
        private bool _incrementSeed;

        protected RantModule(object template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(ContextConfig template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(DocumentConfig template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(params IModule[] modules) : base(modules)
        {
            SetEngine();
        }

        private void SetEngine()
        {
            _engine = new RantEngine();
            using (Stream stream = typeof(RantModule).Assembly
                .GetManifestResourceStream(typeof(RantModule).Assembly.GetName().Name + ".Rantionary.rantpkg"))
            {
                _engine.LoadPackage(RantPackage.Load(stream));
            }
        }

        // Allows you to set a seed for repeatability and testing
        public RantModule WithSeed(long seed)
        {
            _seed = seed;
            _incrementSeed = true;
            return this;
        }

        // This indicates if the seed should be incremented for each document
        // Setting this to false with always generate the same output for the same pattern
        public RantModule IncrementSeed(bool increment = true)
        {
            _incrementSeed = increment;
            return this;
        }

        public RantModule IncludeNsfw(bool includeNsfw = true)
        {
            if (includeNsfw)
            {
                _engine.Dictionary.IncludeHiddenClass("nsfw");
            }
            else
            {
                _engine.Dictionary.ExcludeHiddenClass("nsfw");
            }
            return this;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            string output;
            if(_seed.HasValue)
            {
                output = _engine.Do(content.ToString(), _seed.Value);
                if (_incrementSeed)
                {
                    _seed++;
                }
            }
            else
            {
                output = _engine.Do(content.ToString());
            }
            return new[] {Execute(output, input)};
        }

        protected abstract IDocument Execute(string content, IDocument input);
    }
}