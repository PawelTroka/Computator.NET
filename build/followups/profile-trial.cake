// Uses synthetic/private data only; never uploads a user's real corpus to CI.
Task("Profile-Validate").IsDependentOn("CoreTests").IsDependentOn("DesktopTests");
