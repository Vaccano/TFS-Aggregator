using System;
using System.Collections.Generic;

namespace TFSAggregator
{
    public class ConfigAggregatorItem
    {

        public ConfigAggregatorItem()
        {
            SourceItems = new List<ConfigItemType>();
            Mappings = new List<Mapping>();
            Conditions = new List<Condition>();
            SiblingTypes = new List<string>();
        }

        /// <summary>
        /// The name of the aggregation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the work item type to apply this aggregation to.
        /// </summary>
        public string WorkItemType { get; set; }

        /// <summary>
        /// The allowed other child work item types if ConfigLinkTypeEnum is Parent. Default value is the WorkItemType. Use "All" for all work item types 
        /// /// </summary>
        public List<string> SiblingTypes { get; set; }

        /// <summary>
        /// The type of link that we are using
        /// (Currently only "self" is supported
        /// </summary>
        public ConfigLinkTypeEnum LinkType { get; set; }

        /// <summary>
        /// Field to be updated
        /// </summary>
        public ConfigItemType TargetItem { get; set; }

        /// <summary>
        /// Fields to get the data for the updating from
        /// </summary>
        public List<ConfigItemType> SourceItems { get; set; }

        /// <summary>
        /// Operation to perform on the Source Items before putting the aggregate in TargetItem
        /// </summary>
        public OperationEnum Operation { get; set; }

        /// <summary>
        /// Used to determine if the aggregation operation is numeric or not.
        /// </summary>
        public OperationTypeEnum OperationType { get; set; }

        /// <summary>
        /// Used to apply aggregations for non-numeric operations.
        /// </summary>
        public List<Mapping> Mappings { get; set; }

        /// <summary>
        /// List of conditions on the target.
        /// For example you can only run the aggregation if date field's value has not past.
        /// </summary>
        public List<Condition> Conditions { get; set; }

        /// <summary>
        /// If LinkType is "Parent" then this indicates how many levels up should be looked.
        /// (For example: 1 is the direct parent.  2 is the parent of the parent.)
        /// </summary>
        public int LinkLevel { get; set; }


    }
}