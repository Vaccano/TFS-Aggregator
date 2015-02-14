﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace TFSAggregator
{
    /// <summary>
    /// Static class used to get settings from the disk.
    /// This class will only get the settings once.  To have it get the settings again,
    /// the dll must be dropped in again or TFS must be re-started
    /// </summary>
    public static class TFSAggregatorSettings
    {
        private static string _tfsUri;
        private static string _tfsUserName;
        private static string _tfsDomain;
        private static string _tfsPassword;
        private static MiscHelpers.LoggingLevel _loggingVerbosity;
        private static List<ConfigAggregatorItem> _configAggregatorItems = new List<ConfigAggregatorItem>();
        private static bool _settingsRetrieved = false;

        public const string EventLogSource = "TFSAggregator";
        public const string eventLog = "Application";


        // Load in the settings from file
        private static void GetSettings()
        {
            _configAggregatorItems = new List<ConfigAggregatorItem>();

            // Load the options
            string xmlFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "AggregatorItems.xml");
            XDocument doc = XDocument.Load(xmlFileName);

            // Save off the TFS server name and user settings
            _tfsUri = doc.Element("AggregatorItems").Attribute("tfsServerUrl").Value;
            _tfsUserName = doc.Element("AggregatorItems").LoadAttribute("username", String.Empty);
            _tfsDomain = doc.Element("AggregatorItems").LoadAttribute("domain", String.Empty);
            _tfsPassword = doc.Element("AggregatorItems").LoadAttribute("password", String.Empty);

            //Save off the Logging level
            _loggingVerbosity = doc.Element("AggregatorItems").LoadEnumAttribute("loggingVerbosity", MiscHelpers.LoggingLevel.None);

            // Get all the AggregatorItems loaded in
            foreach (XElement xmlAggItem in doc.Element("AggregatorItems").Elements())
            {
                // Setup the actual item
                ConfigAggregatorItem aggItem = new ConfigAggregatorItem();

                aggItem.Operation = xmlAggItem.LoadEnumAttribute("operation", OperationEnum.Sum);
                aggItem.LinkType = xmlAggItem.LoadEnumAttribute("linkType", ConfigLinkTypeEnum.Self);
                aggItem.OperationType = xmlAggItem.LoadEnumAttribute("operationType", OperationTypeEnum.Numeric);
                aggItem.LinkLevel = xmlAggItem.LoadAttribute("linkLevel", 1);
                aggItem.Name = xmlAggItem.LoadAttribute("name", "Name not set");
                aggItem.WorkItemType = xmlAggItem.Attribute("workItemType").Value;
                foreach (var type in xmlAggItem.LoadAttribute("siblingType", aggItem.WorkItemType).Split(','))
                {
                    aggItem.SiblingTypes.Add(type.Trim());
                }

                // Iterate through all the sub items (Mappings, source and target items.))
                foreach (XElement xmlConfigItem in xmlAggItem.Elements())
                {
                    // If this is a target item then add it as such
                    if (xmlConfigItem.Name == "TargetItem")
                    {
                        aggItem.TargetItem = new ConfigItemType { Name = xmlConfigItem.Attribute("name").Value };

                    }
                    // If this is a source item then add it as such
                    if (xmlConfigItem.Name == "SourceItem")
                    {
                        aggItem.SourceItems.Add(new ConfigItemType { Name = xmlConfigItem.Attribute("name").Value });
                    }

                    // If this is conditions (for the target) then read them in.
                    if (xmlConfigItem.Name == "Conditions")
                    {
                        // Iterate all the conditions we have.
                        foreach (XElement xmlCondition in xmlConfigItem.Elements())
                        {
                            Condition condition = new Condition();
                            condition.LeftFieldName = xmlCondition.LoadAttribute("leftField", "");
                            condition.CompOperator = xmlCondition.LoadEnumAttribute("operator",
                                                                                    ComparisionOperator.EqualTo);
                            condition.RightValue = xmlCondition.LoadAttribute<string>("rightValue", null);
                            condition.RightFieldName = xmlCondition.LoadAttribute<string>("rightField", null);

                            aggItem.Conditions.Add(condition);
                        }

                    }

                    // If this is the mappings then read them in
                    if (xmlConfigItem.Name == "Mappings")
                    {
                        // Iterate all the mappings we have.
                        foreach (XElement xmlMappingItem in xmlConfigItem.Elements())
                        {
                            Mapping mapping = new Mapping();
                            // Read in the target and source values (we set the target field
                            // to this value if all of the source fields are in the list of source values)
                            mapping.TargetValue = xmlMappingItem.Attribute("targetValue").Value;
                            // load in the inclusivity of the mapping (default to "And")
                            mapping.Inclusive = xmlMappingItem.LoadAttribute("inclusive", "And") == "And";
                            foreach (XElement xmlSourceValue in xmlMappingItem.Elements())
                            {
                                mapping.SourceValues.Add(xmlSourceValue.Value);
                            }

                            aggItem.Mappings.Add(mapping);
                        }
                    }
                }
                // add this one to the list
                _configAggregatorItems.Add(aggItem);
            }

            // Indicate that we don't need to load the settings again.
            _settingsRetrieved = true;
        }



        private static void VerifySettings()
        {
            if (!_settingsRetrieved)
            {
                GetSettings();
                ConfigureEventLog();
            }
            _settingsRetrieved = true;
        }

        private static void ConfigureEventLog()
        {
            if (!EventLog.SourceExists(EventLogSource))
                EventLog.CreateEventSource(EventLogSource, eventLog);
        }

        /// <summary>
        /// The location of the tfs server that we are connecting to
        /// </summary>
        public static string TFSUri { get { VerifySettings();  return _tfsUri; } }
        public static string TFSUserName { get { VerifySettings(); return _tfsUserName; } }
        public static string TFSDomain { get { VerifySettings(); return _tfsDomain; } }
        public static string TFSPassword { get { VerifySettings(); return _tfsPassword; } }

        /// <summary>
        /// The various aggregations to be done.
        /// </summary>
        public static List<ConfigAggregatorItem> ConfigAggregatorItems { get { VerifySettings(); return _configAggregatorItems; } }

        /// <summary>
        /// The logging level to use. Valid values: None, Diagnostic. Default is None.
        /// </summary>
        public static MiscHelpers.LoggingLevel LoggingVerbosity { get { return _loggingVerbosity; } }

        public static bool LoggingIsEnabled
        {
            get { return _loggingVerbosity == MiscHelpers.LoggingLevel.Diagnostic; }
        }

    }
}