using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    [Serializable]
    public class NbkData
    {
        public string playerName;
        public Dictionary<string, NbkVariable> variables; 

        public NbkVariable GetVariable(string key)
        {
            return variables[key];
        }

        public void SetVariable(string key, string val) => variables[key].value = val;

        public void CreateVariable(string key, string value)
        {
            if (!variables.ContainsKey(key))
                variables.Add(key, new NbkVariable(key, value));
        }
    }
}
