using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator
{
    public class Condition
    {
        public string LeftFieldName { get; set; }
        public ComparisionOperator CompOperator { get; set; }
        public string RightValue { get; set; }
        public string RightFieldName { get; set; }

        /// <summary>
        /// Perform a comparison.  If anything goes wrong then we just assume that everything checks out
        /// This is because if a value is null, we don't want to fail it.
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns></returns>
        public bool Compare(WorkItem workItem)
        {
            try
            {
                // Null is a bit of a special case.
                if (workItem[LeftFieldName] == null)
                {
                    // If they were checking for Null then that is true.
                    if (RightValue == "$NULL$")
                        return true;
                    // If we have a valid right field then we may have a null == null comparison
                    if ((RightFieldName != null) && (CompOperator == ComparisionOperator.EqualTo))
                        return workItem[LeftFieldName] == workItem[RightFieldName];
                    
                    // Nothing else is going to work out comparing to null.
                    return false;
                }

                // Get the type of the work item.
                Type leftType = workItem[LeftFieldName].GetType();
            
                // Compare each type.
                if (leftType == typeof(string))
                {
                    string leftValue, rightValue;
                    GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare(leftValue, rightValue);
                }
                if (leftType == typeof(int))
                {
                    int leftValue, rightValue;
                    GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare(leftValue, rightValue);
                }
                if (leftType == typeof(double))
                {
                    double leftValue, rightValue;
                    GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare(leftValue, rightValue);
                }
                if (leftType == typeof(DateTime))
                {
                    DateTime leftValue, rightValue;
                    GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare(leftValue, rightValue);
                }

                // No other types are supported
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void GetValues<T>(WorkItem workItem, out T leftValue, out T rightValue) where T: IConvertible
        {
            leftValue = (T)workItem[LeftFieldName];
            if (RightValue != null)
                rightValue = ExpandMacro<T>(RightValue);
            else
                rightValue = (T) workItem[RightFieldName];
        }

        private T ExpandMacro<T>(String macro) where T : IConvertible
        {
            if (macro == "$NOW$")
                return (T)Convert.ChangeType(DateTime.Now, typeof(T));

            return (T)Convert.ChangeType(macro, typeof(T));
        }
    }
}