using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public static class NbkFunction
    {
        public static bool IsEither<T>(this T type, params T[] elements) where T : IComparable
        {
            for (int i = 0; i < elements.Length; i++)
                if (EqualityComparer<T>.Default.Equals(type, elements[i]))
                    return true;
            return false;
        }
    }
}