# ConsistentSharp
Port https://github.com/stathat/consistent into C#

## Install

[![NuGet version](https://badge.fury.io/nu/ConsistentSharp.svg)](https://badge.fury.io/nu/ConsistentSharp)
![](https://github.com/tg123/ConsistentSharp/workflows/.NET%20Core/badge.svg)

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
