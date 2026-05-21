using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for LinkRequestItem
/// </summary>

public class LinkRequestItem
{
    public string destination { get; set; }
    public Domain domain { get; set; }
}
public class Domain
{
    public string fullName { get; set; }
}
