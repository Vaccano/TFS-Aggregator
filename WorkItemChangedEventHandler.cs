﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System.Text;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace TFSAggregator
{
    public class WorkItemChangedEventHandler : ISubscriber
    {

        public WorkItemChangedEventHandler()
        {
            //DON"T ADD ANYTHING HERE UNLESS YOU REALLY KNOW WHAT YOU ARE DOING.
            //TFS DOES NOT LIKE CONSTRUCTORS HERE AND SEEMS TO FREEZE WHEN YOU TRY :(
        }

        public Type[] SubscribedTypes()
        {
            return new Type[1] { typeof(WorkItemChangedEvent) };
        }

        /// <summary>
        /// This is the one where all the magic starts.  Main() so to speak.  I will load the settings, connect to tfs and apply the aggregation rules.
        /// </summary>
        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, NotificationType notificationType, object notificationEventArgs,
                                                    out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
        {
            statusCode = 0;
            properties = null;
            statusMessage = String.Empty;
            int currentAggregationId = 0;
            int workItemId = 0;
            string currentAggregationName = string.Empty;

            try
            {
                if (notificationType == NotificationType.Notification && notificationEventArgs is WorkItemChangedEvent)
                {
                    // Change this object to be a type we can easily get into
                    WorkItemChangedEvent ev = notificationEventArgs as WorkItemChangedEvent;
                    // Connect to the setting file and load the location of the TFS server
                    string tfsUri = TFSAggregatorSettings.TFSUri;
                    // Connect to TFS so we are ready to get and send data.
                    Store store;
                    if(TFSAggregatorSettings.TFSUserName != String.Empty)
                        store = new Store(tfsUri, TFSAggregatorSettings.TFSUserName, TFSAggregatorSettings.TFSPassword, TFSAggregatorSettings.TFSDomain);
                    else
                        store = new Store(tfsUri);

                    // Get the id of the work item that was just changed by the user.
                    workItemId = ev.CoreFields.IntegerFields[0].NewValue;
                    // Download the work item so we can update it (if needed)
                    WorkItem eventWorkItem = store.Access.GetWorkItem(workItemId);
                    string workItemTypeName = eventWorkItem.Type.Name;
                    List<WorkItem> workItemsToSave = new List<WorkItem>();

                    if (TFSAggregatorSettings.LoggingIsEnabled)
                    {
                        MiscHelpers.LogMessage(String.Format("Change detected to {0} [{1}]", workItemTypeName, workItemId));
                        MiscHelpers.LogMessage(String.Format("{0}Processing {1} AggregationItems", "  ", TFSAggregatorSettings.ConfigAggregatorItems.Count.ToString()));
                    }

                    // Apply the aggregation rules to the work item
                    foreach (ConfigAggregatorItem configAggregatorItem in TFSAggregatorSettings.ConfigAggregatorItems)
                    {
                        IEnumerable<WorkItem> sourceWorkItems = null;
                        WorkItem targetWorkItem = null;
                        currentAggregationName = configAggregatorItem.Name;

                        // Check to make sure that the rule applies to the work item type we have
                        if (eventWorkItem.Type.Name == configAggregatorItem.WorkItemType)
                        {
                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}[Entry {2}] Aggregation '{3}' applies to {1} work items", "    ", workItemTypeName, currentAggregationId, currentAggregationName)); 

                            // Use the link type to see what the work item we are updating is
                            if (configAggregatorItem.LinkType == ConfigLinkTypeEnum.Self)
                            {
                                // We are updating the same workitem that was sent in the event.
                                sourceWorkItems = new List<WorkItem> {eventWorkItem};
                                targetWorkItem = eventWorkItem;

                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Aggregation applies to SELF. ({1} {2})", "  ", workItemTypeName, workItemId));

                                // Make sure that all conditions are true before we do the aggregation
                                // If any fail then we don't do this aggregation.
                                if (!configAggregatorItem.Conditions.AreAllConditionsMet(targetWorkItem))
                                {
                                    if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for aggregation are not met.", "    "));
                                    currentAggregationId++;
                                    continue;
                                }

                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for aggregation are met.", "    "));

                            }
                                // We are aggregating to the parent
                            else if (configAggregatorItem.LinkType == ConfigLinkTypeEnum.Parent)
                            {
                                bool parentLevelFound = true;

                                // Go up the tree till we find the level of parent that we are looking for.
                                WorkItem iterateToParent = eventWorkItem;
                                for (int i = 0; i < configAggregatorItem.LinkLevel; i++)
                                {

                                    // Load the parent from the saved list (if we have it in there) or just load it from the store.
                                    WorkItem nullCheck = iterateToParent.GetParentFromListOrStore(workItemsToSave, store);
                                    //
                                    if (nullCheck != null)
                                    {
                                        iterateToParent = nullCheck;
                                    }
                                    else
                                        parentLevelFound = false;

                                }
                                // If we did not find the correct parent then we are done with this aggregation.
                                if (!parentLevelFound)
                                {
                                    if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Couldn't find a PARENT {2} {4} up from {3} [{1}]. This aggregation will not continue.", "  ", workItemId, configAggregatorItem.LinkLevel, workItemTypeName, configAggregatorItem.LinkLevel > 1 ? "levels" : "level"));
                                    currentAggregationId++;
                                    continue;
                                }

                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Found {1} [{2}] {3} {4} up from {5} [{6}].  Aggregation continues.", "  ", iterateToParent.Type.Name, iterateToParent.Id, configAggregatorItem.LinkLevel, configAggregatorItem.LinkLevel > 1 ? "levels" : "level", workItemTypeName, workItemId));

                                targetWorkItem = iterateToParent;

                                // Make sure that all conditions are true before we do the aggregation
                                // If any fail then we don't do this aggregation.
                                if (!configAggregatorItem.Conditions.AreAllConditionsMet(targetWorkItem))
                                {
                                    if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are not met", "    "));
                                    currentAggregationId++;
                                    continue;
                                }

                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are met", "    "));

                                // Get the children down how ever many link levels were specified.
                                List<WorkItem> iterateFromParents = new List<WorkItem> {targetWorkItem};
                                for (int i = 0; i < configAggregatorItem.LinkLevel; i++)
                                {
                                    List<WorkItem> thisLevelOfKids = new List<WorkItem>();
                                    // Iterate all the parents to find the children of current set of parents
                                    foreach (WorkItem iterateFromParent in iterateFromParents)
                                    {
                                        thisLevelOfKids.AddRange(iterateFromParent.GetChildrenFromListOrStore(workItemsToSave, store));
                                    }

                                    iterateFromParents = thisLevelOfKids;
                                }

                                // remove the kids that are not the right type that we are working with
                                ConfigAggregatorItem configAggregatorItemClosure = configAggregatorItem;
                                iterateFromParents.RemoveAll(x => x.Type.Name != configAggregatorItemClosure.WorkItemType);

                                sourceWorkItems = iterateFromParents;
                            }

                            // Do the actual aggregation now
                            bool changeMade = Aggregator.Aggregate(sourceWorkItems, targetWorkItem, configAggregatorItem);

                            // If we made a change then add this work item to the list of items to save.
                            if (changeMade)
                            {
                                // Add the target work item to the list of work items to save.
                                workItemsToSave.AddIfUnique(targetWorkItem);
                            }
                        }
                        else
                        {
                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}[Entry {2}] Aggregation '{3}' does not apply to {1} work items", "    ", workItemTypeName, currentAggregationId, currentAggregationName));
                        }

                        currentAggregationId++;
                    }

                    // Save any changes to the target work items.
                    workItemsToSave.ForEach(x =>
                    {
                        bool isValid = x.IsValid();

                        if (TFSAggregatorSettings.LoggingIsEnabled)
                        {
                            MiscHelpers.LogMessage(String.Format("{0}{0}{0}{1} [{2}] {3} valid to save. {4}",
                                                                "    ",
                                                                x.Type.Name,
                                                                x.Id,
                                                                isValid ? "IS" : "IS NOT",
                                                                String.Format("\n{0}{0}{0}{0}Invalid fields: {1}", "    ", MiscHelpers.GetInvalidWorkItemFieldsList(x).ToString())));
                        }

                        if (isValid)
                        {
                            x.PartialOpen();
                            x.Save();
                        }
                    });

                    MiscHelpers.AddRunSeparatorToLog();
                }

            }
            catch (Exception e)
            {
                string message = String.Format("Exception encountered processing Work Item [{2}]: {0} \nStack Trace:{1}", e.Message, e.StackTrace, workItemId);
                if (e.InnerException != null)
                {
                    message += String.Format("\n    Inner Exception: {0} \nStack Trace:{1}", e.InnerException.Message, e.InnerException.StackTrace);
                }
                MiscHelpers.LogMessage(message, true);
            }

            return EventNotificationStatus.ActionPermitted;
        }

        public string Name
        {
            get { return "TFSAggregator"; }
        }



        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.Normal; }
        }


    }
}
