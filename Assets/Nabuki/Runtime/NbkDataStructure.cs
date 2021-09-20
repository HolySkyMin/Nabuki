using System;
using System.Collections.Generic;

namespace Nabuki
{
    [Serializable]
    public class NbkVariable
    {
        public readonly string key;
        public string value;
        public readonly NbkVariableType type;

        public NbkVariable(string k, string v)
        {
            key = k;
            value = v;

            if (IsInt())
                type = NbkVariableType.Int;
            else if (IsFloat())
                type = NbkVariableType.Float;
            else if (IsBool())
                type = NbkVariableType.Bool;
            else
                type = NbkVariableType.String;
        }

        #region Operators

        public static bool operator ==(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() == right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() == right.ToFloat();
            else if (left.type == NbkVariableType.Bool)
                return left.ToBool() == right.ToBool();
            else
                return left.value == right.value;
        }

        public static bool operator !=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() != right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() != right.ToFloat();
            else if (left.type == NbkVariableType.Bool)
                return left.ToBool() != right.ToBool();
            else
                return left.value != right.value;
        }

        public static bool operator >(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBig);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() > right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() > right.ToFloat();
            else
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBig);
        }

        public static bool operator <(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBig);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() < right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() < right.ToFloat();
            else
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBig);
        }

        public static bool operator >=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBigSame);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() >= right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() >= right.ToFloat();
            else
                throw new NbkTypeCompareException(left.type, NbkCompareType.LeftBigSame);
        }

        public static bool operator <=(NbkVariable left, NbkVariable right)
        {
            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
            if (left.type.IsEither(NbkVariableType.Bool, NbkVariableType.String))
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBigSame);

            if (left.type == NbkVariableType.Int)
                return left.ToInt() <= right.ToInt();
            else if (left.type == NbkVariableType.Float)
                return left.ToFloat() <= right.ToFloat();
            else
                throw new NbkTypeCompareException(left.type, NbkCompareType.RightBigSame);
        }

        #endregion

        public bool IsInt()
        {
            return int.TryParse(value, out _);
        }

        public bool IsFloat()
        {
            return float.TryParse(value, out _);
        }

        public bool IsBool()
        {
            return bool.TryParse(value, out _);
        }

        public int ToInt() => int.Parse(value);
        public float ToFloat() => float.Parse(value);
        public bool ToBool() => bool.Parse(value);

        public void SetValue(string v)
        {
            if (type == NbkVariableType.Int && !int.TryParse(v, out _)
                || type == NbkVariableType.Float && !float.TryParse(v, out _)
                || type == NbkVariableType.Bool && !bool.TryParse(v, out _))
                throw new NbkValueTypeException(type);
            value = v;
        }
    }

    [Serializable]
    public enum NbkVariableType { Int, Float, Bool, String }

    public class NbkCondition
    {
        IFeatureVariable manager;
        NbkVariable left;
        NbkVariable right;
        string leftKey, rightKey;
        NbkCompareType compare;

        public NbkCondition(IFeatureVariable target, string l, string r, NbkCompareType c)
        {
            manager = target;
            leftKey = l;
            rightKey = r;
            compare = c;
        }

        public void Link()
        {
            left = manager.VariableData.GetVariable(leftKey);
            try { right = manager.VariableData.GetVariable(rightKey); }
            catch { right = new NbkVariable("hotvalue", rightKey); }

            if (left.type != right.type)
                throw new NbkTypeCompareException(left.type, right.type);
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