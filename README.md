# ConsistentSharp
Port https://github.com/stathat/consistent in to C#

## Install

[![NuGet version](https://badge.fury.io/nu/ConsistentSharp.svg)](https://badge.fury.io/nu/ConsistentSharp)
[![Build status](https://farmer1992.visualstudio.com/opensources/_apis/build/status/ConsistentSharp)](https://farmer1992.visualstudio.com/opensources/_build/latest?definitionId=6)

```
Install-Package ConsistentSharp 
```

## Usage

```
var c = new ConsistentHash();

c.Add("cacheA");
c.Add("cacheB");
c.Add("cacheC");

var users = new[] {"user_mcnulty", "user_bunk", "user_omar", "user_bunny", "user_stringer"};

Console.WriteLine(c.Get("user_mcnulty"));  // cacheA

c.Add("cacheD");
c.Add("cacheE");
Console.WriteLine(c.Get("user_mcnulty"));  // cacheE


c.Remove("cacheD");
c.Remove("cacheE");
Console.WriteLine(c.Get("user_mcnulty"));  // cacheA

c.Remove("cacheC");
Console.WriteLine(c.Get("user_mcnulty"));  // cacheA
```
