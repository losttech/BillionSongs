# THIS BRANCH IS FOR THE FROZEN VERSION OF BILLION SONGS, THAT DOES NOT GENERATE NEW ONES

It is used to replace a previously functional Billion Songs instance
to keep serving already generated songs. It needs a `songs.db` already
filled with pregenerated songs to work, and does **NOT** use GPT-2.

It is used to host [the original instance of Billion Songs](http://billion.dev.losttech.software:2095/),
which used an older TensorFlow version, and hence generated a bit different lyrics,
which I wanted to preserve.

It also consumes much less resources to run.

# Billion Songs, AI-powered song lyrics generator

[Get a generated song](http://billion.dev.losttech.software:2095/)

See the blog post
[Writing billion songs with C# and Deep Learning](https://habr.com/post/453232/)
for a detailed explanation how it works.

This project mainly serves as a demonstration of
[Gradient](https://losttech.software/gradient.html),
our TensorFlow binding for C# and other .NET languages.

[Other Gradient samples](https://github.com/losttech/Gradient-Samples).

# Prerequisites

1. Download and install Python and TensorFlow 1.10.x via pip
2. Install Python package, called `regex` (`python -m pip install regex --user`)
3. Install the latest .NET Core 3.1 SDK
