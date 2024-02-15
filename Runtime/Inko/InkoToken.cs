using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki.Inko
{
    public class InkoToken
    {
        public string Key => _key;

        public IReadOnlyList<string> Values => _values;
        
        private string _key;
        private List<string> _values;
        
        public InkoToken(string plainText, bool splitValue)
        {
            // First split
            var firstSplit = plainText.Split(':');
            _key = firstSplit[0].Trim().ToLower();
            
            _values = new List<string>();
            if (firstSplit.Length == 1)  // No value text.
                return;

            // Concat string for possible three-or-more first splits
            var valueText = firstSplit[1];
            if (firstSplit.Length > 2)
                for (int i = 2; i < firstSplit.Length; i++)
                    valueText = string.Concat(valueText, firstSplit[i]);
            
            if (splitValue)
            {
                // Second split
                var secondSplit = valueText.Split(',');
                foreach (var value in secondSplit)
                    _values.Add(value.Trim());
            }
            else
                _values.Add(valueText.Trim());
        }
    }
}
