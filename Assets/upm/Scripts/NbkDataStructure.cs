using System;
using System.Collections.Generic;

namespace Nabuki
{
    [Serializable]
    public class NbkVariable
    {
        public string key;

        NbkVariableType type;
        dynamic value;

        #region Operators

        public static bool operator ==(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);

            return left.GetValue() == right.GetValue();
        }

        public static bool operator !=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);

            return left.GetValue() != right.GetValue();
        }

        public static bool operator >(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBig);

            return left.GetValue() > right.GetValue();
        }

        public static bool operator <(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBig);

            return left.GetValue() < right.GetValue();
        }

        public static bool operator >=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBigSame);

            return left.GetValue() >= right.GetValue();
        }

        public static bool operator <=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBigSame);

            return left.GetValue() <= right.GetValue();
        }

        #endregion

        public NbkVariable(NbkVariableType t, string k, dynamic v)
        {
            type = t;
            key = k;

            SetValue(v);
        }

        public NbkVariableType GetNbkType() => type;

        public dynamic GetValue() => value;

        public T GetValue<T>() => (T)value;

        public void SetValue(dynamic v)
        {
            CheckValueType(v);

            value = v;
        }

        public void SetValue(NbkVariableType t, dynamic v)
        {
            if (type != t)
                throw new NbkValueTypeException(type, t);

            value = v;
        }

        void CheckValueType(dynamic v)
        {
            switch (type)
            {
                case NbkVariableType.Int:
                    if (v.GetType() != typeof(int))
                        throw new NbkValueTypeException(type);
                    break;
                case NbkVariableType.Float:
                    if (v.GetType() != typeof(float))
                        throw new NbkValueTypeException(type);
                    break;
                case NbkVariableType.Bool:
                    if (v.GetType() != typeof(bool))
                        throw new NbkValueTypeException(type);
                    break;
                case NbkVariableType.String:
                    if (v.GetType() != typeof(string))
                        throw new NbkValueTypeException(type);
                    break;
            }
        }
    }

    [Serializable]
    public enum NbkVariableType { Int, Float, Bool, String }

    public class NbkCondition
    {
        NbkVariable left;
        NbkVariable right;
        string leftKey, rightKey;
        NbkCompareType compare;

        public NbkCondition(string l, string r, NbkCompareType c)
        {
            leftKey = l;
            rightKey = r;
            compare = c;
        }

        public void Link()
        {
            left = DialogueManager.GetVariable(leftKey);

            try { right = DialogueManager.GetVariable(rightKey); }
            catch 
            {
                dynamic rv = null;
                switch(left.GetNbkType())
                {
                    case NbkVariableType.Int:
                        rv = int.Parse(rightKey); break;
                    case NbkVariableType.Float:
                        rv = float.Parse(rightKey); break;
                    case NbkVariableType.Bool:
                        rv = bool.Parse(rightKey); break;
                    case NbkVariableType.String:
                        rv = rightKey; break;
                }
                right = new NbkVariable(left.GetNbkType(), "hotvalue", rv);
            }
        }

        public bool Get()
        {
            switch(compare)
            {
                case NbkCompareType.Same: return left == right;
                case NbkCompareType.NotSame: return left != right;
                case NbkCompareType.LeftBig: return left > right;
                case NbkCompareType.RightBig: return left < right;
                case NbkCompareType.LeftBigSame: return left >= right;
                case NbkCompareType.RightBigSame: return left <= right;
                default: return false;
            }
        }
    }

    public enum NbkCompareType { Same, NotSame, LeftBig, RightBig, LeftBigSame, RightBigSame }

    public class NbkConditionSet
    {
        public bool isElse;
        public List<NbkCondition> conditions;
        public List<NbkConditionJointLogic> jointLogics;

        public void Link()
        {
            foreach (var cond in conditions)
                cond.Link();
        }

        public bool Get()
        {
            if (isElse)
                return true;

            var result = conditions[0].Get();
            for(int i = 0; i < jointLogics.Count; i++)
            {
                switch(jointLogics[i])
                {
                    case NbkConditionJointLogic.AND:
                        result = result && conditions[i + 1].Get();
                        break;
                    case NbkConditionJointLogic.OR:
                        result = result || conditions[i + 1].Get();
                        break;
                }

                if (result == false)
                    break;
            }

            return result;
        }
    }

    public enum NbkConditionJointLogic { AND, OR }
}