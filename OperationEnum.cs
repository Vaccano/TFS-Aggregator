using System;

namespace TFSAggregator
{
    public static class Operations
    {
    
    public static double Perform(this OperationEnum operation, double aggregateValue, double sourceValue)
        {
            if (operation == OperationEnum.Sum)
                return aggregateValue + sourceValue;
            if (operation == OperationEnum.Subtract)
                return aggregateValue - sourceValue;
        if (operation == OperationEnum.Multiply)
        {
            if  (aggregateValue ==0.0)
            {
                aggregateValue = +1;
            }
            return (aggregateValue*sourceValue);
        }
        if (operation == OperationEnum.Divide)
        {
            if (aggregateValue == 0.0)
            {
                aggregateValue = +1;
                return (sourceValue / aggregateValue);
            }
            else
            {
                return (aggregateValue / sourceValue);
                
            }


        }
        throw new InvalidOperationException("Unsupported Aggregation Operation");
        }

    }
    public enum OperationEnum
    {
        Sum,
        Subtract,
        Multiply,
        Divide
    }
}