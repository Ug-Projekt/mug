using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class BlockNode : INode
    {
        public IStatement[] Statements
        {
            get
            {
                return statements.ToArray();
            }
        }

        public Range Position { get; set; }

        List<IStatement> statements = new();
        public void Add(IStatement node)
        {
            statements.Add(node);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < statements.Count; i++)
                nodes += statements[i].Stringize(indent+"   ")+'\n';
            return indent+$"BlockNode: ({Position.Start}:{Position.End})"+(nodes != "" ? '\n'+nodes : "(empty)\n"+indent.Remove(indent.Length-3));
        }
    }
}
