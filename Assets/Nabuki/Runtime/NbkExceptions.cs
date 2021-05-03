using System;

namespace Nabuki
{
    [Serializable]
    public class NbkTypeCompareException : Exception
    {
        public NbkTypeCompareException() { }

        public NbkTypeCompareException(NbkVariableType left, NbkVariableType right)
            : base(string.Format("Cannot compare between {0} and {1}.", left, right)) { }

        public NbkTypeCompareException(NbkVariableType left, NbkVariableType right, Exception inner)
            : base(string.Format("Cannot compare between {0} and {1}.", left, right), inner) { }

        public NbkTypeCompareException(NbkVariableType type, NbkCompareType comparer)
            : base(string.Format("Cannot make {1} comparison for {0} type.", type, comparer)) { }

        public NbkTypeCompareException(NbkVariableType type, NbkCompareType comparer, Exception inner)
            : base(string.Format("Cannot make {1} comparison for {0} type.", type, comparer), inner) { }

        protected NbkTypeCompareException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NbkValueTypeException : Exception
    {
        public NbkValueTypeException() { }

        public NbkValueTypeException(NbkVariableType type)
            : base(string.Format("Cannot set other type of value into {0} type variable.", type)) { }

        public NbkValueTypeException(NbkVariableType type, Exception inner)
            : base(string.Format("Cannot set other type of value into {0} type variable.", type), inner) { }

        public NbkValueTypeException(NbkVariableType exist, NbkVariableType rolling)
            : base(string.Format("Cannot set {1} type value into {0} type variable.", exist, rolling)) { }

        public NbkValueTypeException(NbkVariableType exist, NbkVariableType rolling, Exception inner)
            : base(string.Format("Cannot set {1} type value into {0} type variable.", exist, rolling), inner) { }

        protected NbkValueTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NbkDialogueParseException : Exception
    {
        public NbkDialogueParseException() { }
        public NbkDialogueParseException(int line) 
            : base(string.Format("Parse error occured in line {0}.", line)) { }
        public NbkDialogueParseException(int line, Exception inner) 
            : base(string.Format("Parse error occured in line {0}, which is caused by this: {1}", line, inner.Message), inner) { }
        protected NbkDialogueParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class NbkWrongSyntaxException : Exception
    {
        public NbkWrongSyntaxException() { }
        public NbkWrongSyntaxException(string message) : base(message) { }
        public NbkWrongSyntaxException(string message, Exception inner) : base(message, inner) { }
        protected NbkWrongSyntaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}