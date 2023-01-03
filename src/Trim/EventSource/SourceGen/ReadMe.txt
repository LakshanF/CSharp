This experiment attempts to discover EventSource issues related to AOT diagnostics. Specifically, the experiment is meant to cover the below areas
 - An app sending EventSource events using primitive data types
 - An app sending EventSource events using complex data types
 - Trimmed app sending both primitive and complex data types
 - AOT application sending both primitive and complex data types

 The Application uses 2 projects, one that contains an EventSource and the other containing complex data types