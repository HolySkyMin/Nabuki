using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public enum NbkTokenType { Value, Tuple, TupleString, Function }

    public class NbkToken
    {
        public string content;

        public NbkToken(string token)
        {
            content = token;
        }
    }

    public class NbkTupleToken : NbkToken
    {
        public string[] items;

        public NbkTupleToken(string token, params char[] separator) : base(token)
        {
            items = content.Split(separator);
            for (int i = 0; i < items.Length; i++)
                items[i] = items[i].Trim();
        }
    }

    public class NbkFuncToken : NbkToken
    {
        public string function;
        public NbkTupleToken parameter;

        public NbkFuncToken(string token) : base(token)
        {
            var partition = content.Split(':');
            function = partition[0];
            if (partition.Length > 1)
                parameter = new NbkTupleToken(partition[1].Trim(), ',');
        }
    }

    public class NbkTokenizer
    {
        string[] rawTokens;
        int tokenIndex;

        public NbkTokenizer(string line)
        {
            rawTokens = line.Split('\t');
            tokenIndex = 0;
        }

        public NbkToken GetToken(NbkTokenType type = NbkTokenType.Value)
        {
            if (tokenIndex >= rawTokens.Length)
                return null;

            switch(type)
            {
                case NbkTokenType.Value: return new NbkToken(rawTokens[tokenIndex++]);
                case NbkTokenType.Tuple: return new NbkTupleToken(rawTokens[tokenIndex++], ',');
                case NbkTokenType.TupleString: return new NbkTupleToken(rawTokens[tokenIndex++], '|');
                case NbkTokenType.Function: return new NbkFuncToken(rawTokens[tokenIndex++]);
                default: return null;
            }
        }

        public List<NbkToken> GetParameter(params NbkTokenType[] types)
        {
            var list = new List<NbkToken>();
            foreach(var type in types)
            {
                var token = GetToken(type);
                if (token == null)
                    throw new NbkWrongSyntaxException("Cannot find essential token.");
                list.Add(token);
            }
            return list;
        }

        public List<NbkFuncToken> GetTag()
        {
            var list = new List<NbkFuncToken>();
            while (tokenIndex < rawTokens.Length)
                list.Add((NbkFuncToken)GetToken(NbkTokenType.Function));
            return list;
        }
    }
}