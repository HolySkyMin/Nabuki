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

        public NbkVariable GetVariable(string key) => variables[key];

        public dynamic GetVariableValue(string key) => variables[key].GetValue();

        public T GetVariableValue<T>(string key) => variables[key].GetValue<T>();

        public void SetVariable(string key, dynamic value) => variables[key].SetValue(value);

        public void CreateVariable(string key, NbkVariableType type, dynamic value)
        {
            if (!variables.ContainsKey(key))
                variables.Add(key, new NbkVariable(type, key, value));
        }
    }
}
