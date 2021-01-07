var buildVersion = "0.0.1-preview";

IEnumerable<string> Git(string arguments) {
    using(var process = StartAndReturnProcess("git", new ProcessSettings{ Arguments = arguments, RedirectStandardError = true, RedirectStandardOutput = true, Silent = true }))
    {
        process.WaitForExit();
        return process.GetStandardOutput().ToList();
    }
}

string GetCurrentBranchName() {
    var result = Git("rev-parse --abbrev-ref HEAD");
    return result.FirstOrDefault();
}

string GetCurrentTagName() {
    var result = Git("tag -l --points-at HEAD");
    return result.FirstOrDefault();
}

string GetLastReleaseVersion() {
    var heads = Git("show-ref --head");
    var versions = new List<Version>();
    foreach (var head in heads) {
        if (head.Contains("/release/")) {
            var version = head.Split('/').Last();
            versions.Add(new Version(version));
        }
    }
    
    versions.Sort();
    
    var lastReleaseVersion = versions.LastOrDefault();
    if (lastReleaseVersion == null) {
        return "0.0";
    }
    else {
        return lastReleaseVersion.ToString();
    }
}

string GetNextMinorVersion(string lastReleaseVersion) {
    var versionNodes = lastReleaseVersion.Split('.');
    var major = versionNodes[0];
    var minor = int.Parse(versionNodes[1]) + 1;

    var releaseVersion = $"{major}.{minor}.0";
    return releaseVersion;
}

string GetBuildVersion() {
    string buildNumber = Argument<string>( "build", "1" );
    
    var currentBranch = GetCurrentBranchName();
    var tag = GetCurrentTagName();
    var lastReleaseVersion = GetLastReleaseVersion();

    if (lastReleaseVersion == "") {
        lastReleaseVersion = "0.0";
    }
    
    Information("Current branch   : {0}", currentBranch);
    Information("Current tag      : {0}", tag);
    Information("Last release ver : {0}", lastReleaseVersion);
    
    var major = "0";
    var minor = "1";
    var patch = "0";
    var releaseVersion = "";
    
    if (currentBranch.Contains("release/")) {
        var matches = System.Text.RegularExpressions.Regex.Matches(currentBranch, "release/[0-9]+\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (matches.Count == 0) {
            throw new Exception("Unable to extract version number from release branch");
        }

        releaseVersion = currentBranch.Replace("release/", "");

        var versionNodes = releaseVersion.Split('.');
        major = versionNodes[0];
        minor = versionNodes[1];
        patch = buildNumber;
    }
    else if (currentBranch.Contains("hotfix/") || currentBranch.Contains("bugfix/")) {
        var patchPrefix = "0";
        var matches = System.Text.RegularExpressions.Regex.Matches(currentBranch, "[0-9]+\\.[0-9]+\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (matches.Count >= 1) {
            releaseVersion = matches[0].Value;
            patchPrefix = matches[0].Groups[1].ToString();
        }
        else {
            matches = System.Text.RegularExpressions.Regex.Matches(currentBranch, "[0-9]+\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matches.Count >= 1) {
                releaseVersion = matches[0].Value;
            }
            else {
                releaseVersion = GetNextMinorVersion(lastReleaseVersion);
            }
        }

        var versionNodes = releaseVersion.Split('.');
        major = versionNodes[0];
        minor = versionNodes[1];
        patch = $"{patchPrefix}-hf-{buildNumber}";
    }
    else if (currentBranch.Contains("feature/")) {
        var nameNodes = currentBranch.Split('/');
        var featureName = nameNodes[nameNodes.Length-1];
        var matches = System.Text.RegularExpressions.Regex.Matches(currentBranch, "[0-9]+\\.[0-9]+\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (matches.Count >= 1) {
            releaseVersion = matches[0].Value;
        }
        else {
            matches = System.Text.RegularExpressions.Regex.Matches(currentBranch, "[0-9]+\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matches.Count >= 1) {
                releaseVersion = matches[0].Value;
            }
            else {
                releaseVersion = GetNextMinorVersion(lastReleaseVersion);
            }
        }

        var versionNodes = releaseVersion.Split('.');
        major = versionNodes[0];
        minor = versionNodes[1];
        patch = $"0-{featureName}-{buildNumber}";

        while (patch.Length > 20) {
            featureName = featureName.Remove(featureName.Length-1, 1);
            patch = $"0-{featureName}-{buildNumber}";
        }
    }
    else {
        releaseVersion = GetNextMinorVersion(lastReleaseVersion);

        var versionNodes = releaseVersion.Split('.');
        major = versionNodes[0];
        minor = versionNodes[1];
        patch = $"0-master-{buildNumber}";
    }
    
    var teamcityVersion = $"{major}.{minor}.{patch}";
    var netfullAssemblyVersion = $"{major}.{minor}.{patch}";
    var netcoreAssemblyVersion = teamcityVersion;

    Information("##teamcity[setParameter name='buildNumber.assembly.full' value='{0}']", netfullAssemblyVersion);
    Information("##teamcity[setParameter name='buildNumber.assembly.core' value='{0}']", netcoreAssemblyVersion);
    Information("##teamcity[buildNumber '{0}']", teamcityVersion);

    return teamcityVersion;
}

void UserPowershellVersion() {
    var result = GetBuildVersion();
    Information("Build version: {0}", result);
}

Task("create-version")
    .Does(() =>
{
    buildVersion = GetBuildVersion();
    Information("Build version: {0}", buildVersion);
});

var target = Argument("target", "Default");

Task("Default")
  .IsDependentOn("create-version")
  .Does(() =>
{
    Information("Build version: {0}", buildVersion);
});

RunTarget(target);