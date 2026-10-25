// Uses synthetic/private data only; never uploads a user's real corpus to CI.
Task("Profile-Validate")
    .Description("Windows: exercise profile/settings contracts in core and desktop suites.")
    .IsDependentOn("CoreTests").IsDependentOn("DesktopTests");
