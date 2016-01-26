Twift - TWItter File Transfer
=============================

# Usage

```
twift [OPTIONS] FILE
```

Share FILE by Tweet. Filename will be omitted to a.[extension] when -d is not specified.

__WARNING: Share by tweet is currently disabled, until 10000 charactors tweets become available.__

## Options:

* -d, --dm=VALUE             Send or receive file by DM to/from specified user.
* -r, --receive=VALUE        Receive file specified by two ids and write to
FILE. (two comma-separated ids, no spaces)
* -q, --quiet                DOES NOT Show detailed log.
* -n, --noshare              DOES NOT Tweet or DM two ids to specify sent file.
* -t, --setup                Setup Twitter account.
 
# Requires

## *nix

* mono 3.0 or above
* fsharp
* curl

## Windows

* .NET 4.5 or above
* Visual Studio 2012 or above
* fsharp

# Examples

```
twift a.png
```

Share a.png by tweet.

```
twift -d username a.png
```

Send a.png to @username by direct messages.

```
twift a.png -s
```

Something like "Just shared a.png by @tw1ft! $ twift -r 687214456179298304,687214460004478976 a.png to receive." will be tweeted.

```
twift -r 687214456179298304,687214460004478976 a.png
```

Receive a file shared by twift, and then write to a.png.

# LICENSE

is GPL v3 or above.
