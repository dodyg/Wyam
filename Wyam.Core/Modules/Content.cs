﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Overwrites the existing content with the specified content
    public class Content : ContentModule
    {
        public Content(object content)
            : base(content)
        {
        }

        public Content(ContextConfig content)
            : base(content)
        {
        }

        public Content(DocumentConfig content) 
            : base(content)
        {
        }

        public Content(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new [] { content == null ? input : input.Clone(content.ToString()) };
        }
    }
}
