using System;
using System.Collections.Generic;
using System.Text;
using Mug.Models.Parser.NodeKinds;

namespace Mug.Models.Parser.NodeKinds.Directives
{
    public enum ImportMode
    {
        FromPackages,
        FromLocal,
    }
    public class ImportDirective : INode
    {
        public INode Member { get; set; }
        public ImportMode Mode { get; set; }
        public Range Position { get; set; }
    }
}
