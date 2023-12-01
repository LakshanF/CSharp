```mermaid
graph TD;
    ApplyResources_ListBox_PropertyName-->FillResources;
    FillResources-->GetResourceSet;
    ApplyResources_ListBox_PropertyName-->Find_PropertyNameValueFromResourceSet;
    Find_PropertyNameValueFromResourceSet-->|Runtime or Designtime?| Runtime;
    Runtime-->|Yes| SetPropertyOnListBoxViaReflection;
```
