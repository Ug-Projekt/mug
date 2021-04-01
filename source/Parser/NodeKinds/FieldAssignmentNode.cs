﻿using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldAssignmentNode : IStatement
    {
        public string NodeKind => "FieldAssignment";
        public String Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
