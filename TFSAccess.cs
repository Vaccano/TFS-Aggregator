using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator
{
    /// <summary>
    /// Singleton Used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// </summary>
    public class Store
    {
        private readonly string _tfsServerUrl;
        private readonly string _tfsUserDomain;
        private readonly string _tfsUserName;
        private readonly string _tfsUserPassword;

        public Store(string tfsServerUrl)
        {
            _tfsServerUrl = tfsServerUrl;
        }
        public Store(string tfsServerUrl, string tfsUserName, string tfsUserPassword, string tfsUserDomain)
        {
            _tfsServerUrl = tfsServerUrl;
            _tfsUserName = tfsUserName;
            _tfsUserPassword = tfsUserPassword;
            _tfsUserDomain = tfsUserDomain;
        }

        private TFSAccess _access;
        public TFSAccess Access
        {
            get
            {
                if (_access == null && String.IsNullOrEmpty(_tfsUserName))
                    _access = new TFSAccess(_tfsServerUrl);
                else if(_access == null)
                    _access = new TFSAccess(_tfsServerUrl, _tfsUserName, _tfsUserPassword, _tfsUserDomain);
                return _access;
            }
        }
    }

    /// <summary>
    /// Don't use this class directly.  Use the StoreSingleton.
    /// </summary>
    public class TFSAccess
    {
        private readonly WorkItemStore _store;

        public TFSAccess(string tfsUri)
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsUri));
            _store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
        }
        public TFSAccess(string tfsUri, string sUserName, string sPassword, string sDomain)
        {
            System.Net.NetworkCredential cre = new System.Net.NetworkCredential(sUserName, sPassword, sDomain);
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsUri), cre);
            _store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
        }

        public WorkItem GetWorkItem(int workItemId)
        {
            return _store.GetWorkItem(workItemId);
        }
    }
}