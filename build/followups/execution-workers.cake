Task("Worker-Validate")
    .Description("Windows: validate API worker and real custom-function browser workflows.")
    .IsDependentOn("ApiTests").IsDependentOn("Web-CustomFunctions");
