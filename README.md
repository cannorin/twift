Twift - TWItter File Transfer
=============================

# Usage

```
twift [OPTIONS] FILE
```

Share FILE by Tweet. Filename will be omitted to a.[extension] when -d is not specified.

## Options:

* -d, --dm=VALUE             Send or receive file by DM to/from specified user.
* -r, --receive=VALUE        Receive file specified by two ids and write to
FILE. (two comma-separated ids, no spaces)
* -v, --verbose              Show detailed log.
* -s, --share                Tweet or DM two ids to specify sent file.
* -t, --setup                Setup Twitter account

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

# LICENSE

is GPL v3 or above.
