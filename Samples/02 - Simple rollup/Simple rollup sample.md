# TFS Configuration
TFS must use a local or domain account: default *Local Service* won't work. The service account must be, at least, Project Admin.

# Test Project
No OOB Process templates support the sample.
Create a test project using CMMI template.
The install script will customize *Requirement* and *Task* work item types.

# Test Data
Requirement *Finish Date* must be set to a future date.
Set *Original Estimate* and *Remaining work* in a child task.

# Expected log
```
[12816] TFSAggregator: Change detected to Task [3] 
[12816] TFSAggregator:   Processing 2 AggregationItems 
[12816] TFSAggregator:     [Entry 0] Aggregation 'Remaining Work roll-up' applies to Task work items 
[12816] TFSAggregator:       Found Requirement [2] 1 level up from Task [3].  Aggregation continues. 
[12816] TFSAggregator:         All conditions for parent aggregation are met 
[12816] TFSAggregator:     [Entry 1] Aggregation 'Original Estimate freeze' applies to Task work items 
[12816] TFSAggregator:       Found Requirement [2] 1 level up from Task [3].  Aggregation continues. 
[12816] TFSAggregator:         All conditions for parent aggregation are met 
[12816] TFSAggregator:             Requirement [2] IS valid to save.  
[12816] TFSAggregator:                 Invalid fields: None  
[12816] TFSAggregator:  
```