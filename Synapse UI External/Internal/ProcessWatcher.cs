/******************************** Module Header ****************************\
Module Name:  ProcessWatcher.cs
Project:      CSProcessWatcher
Copyright (c) Microsoft Corporation.

using WMI Query Language (WQL) to query process events.

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/en-us/openness/licenses.aspx#MPL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System.Management;

namespace sxlib.Internal
{
    public delegate void ProcessEventHandler(Process proc);

    class ProcessWatcher : ManagementEventWatcher
    {
        // Process Events
        public event ProcessEventHandler ProcessCreated;
        public event ProcessEventHandler ProcessDeleted;
        public event ProcessEventHandler ProcessModified;

        // WMI WQL process query string
        static readonly string processEventQuery = @"SELECT * FROM 
             __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' 
             and TargetInstance.Name = '{0}'";

        public ProcessWatcher(string processName)
        {
            Init(processName);
        }

        private void Init(string processName)
        {
            this.Query.QueryLanguage = "WQL";
            this.Query.QueryString = string.Format(processEventQuery, processName);
            this.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            Process proc = new Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);

            switch (eventType)
            {
                case "__InstanceCreationEvent":
                    if (ProcessCreated != null)
                    {
                        ProcessCreated(proc);
                    }
                    break;

                case "__InstanceDeletionEvent":
                    if (ProcessDeleted != null)
                    {
                        ProcessDeleted(proc);
                    }
                    break;

                case "__InstanceModificationEvent":
                    if (ProcessModified != null)
                    {
                        ProcessModified(proc);
                    }
                    break;
            }
        }
    }
}
