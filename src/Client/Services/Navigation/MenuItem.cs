// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;

namespace HeadStart.Client.Services.Navigation;

public class MenuItem
{
    public string Label { get; set; }=string.Empty;
    public string? Description { get; set; }
    public string? Href { get; set; }
    public string? StartIcon { get; set; }
    public string? EndIcon { get; set; }
    public List<MenuItem> SubItems { get; set; } = [];
    public PageStatus Status { get; set; } = PageStatus.Completed; // Default to Completed
    public bool SpecialMenu => SubItems.Any() && SubItems.First().IsParent;
    public bool IsParent => SubItems.Any();
}


public enum PageStatus
{
    [Description("Coming Soon")] ComingSoon,
    [Description("New")] New,
    [Description("Completed")] Completed
}
