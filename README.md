# Billion Songs, AI-powered song lyrics generator

[Generate a song](http://billion.dev.losttech.software:2095/)

> NOTE: this repository has git submodules. So clone with --recurse-submodules.
> Learn about them [here](https://git-scm.com/book/en/v2/Git-Tools-Submodules).

See the blog post
[Writing billion songs with C# and Deep Learning](https://habr.com/post/453232/)
for a detailed explanation how it works.

This project mainly serves as a demonstration of
[Gradient](https://losttech.software/gradient.html),
our TensorFlow binding for C# and other .NET languages.

[Other Gradient samples](https://github.com/losttech/Gradient-Samples).

# What is it, and how does it work?

This is a deep learning-powered song lyrics generator, based on
[GPT-2](https://github.com/openai/gpt-2), wrapped as a ASP.NET Core website.

It generates songs word by word (or rather token by token), using
the statistical relationships learned by a deep learning model, called
[GPT-2](https://github.com/openai/gpt-2).
The actual generator code is in
[GradientTextGenerator class](Web/GradientTextGenerator.cs).

Text generation is pretty slow even with a powerful GPU,
so we have a bunch of caches in /Web to provide a better user experience.
There is also [PregeneratedSongProvider](Web/PregeneratedSongProvider.cs),
which continuously creates new texts in the background to ensure clicking 
"Make Random" button gives an instant result.

Detailed explanation in a [blog post](https://habr.com/post/453232/)

# Prerequisites

1. Download and install Python and TensorFlow as described in
[Gradient documentation](https://github.com/losttech/Gradient/#install-python-tensorflow)
2. Install Python package, called `regex` (`python -m pip install regex --user`)
3. Install the latest .NET Core SDK

# Run instructions

1. After cloning the repository, enter the `Web` folder and run `dotnet ef database update`.
That should create `songs.db` file in the same directory.
2. Edit `appsettings.json` (see `appsettings.Development.json` for an example):
    - add `"DB": "sqlite"`
    - modify `DefaultConnection` to `"DefaultConnection": "Data Source=songs.db"`
3. Run `dotnet run web`. This should print some logs.
Wait for `Now listening on: http://`, then open that URL in the browser.
It will take up to 4 minutes to generate the first song.


# Train instructions

> NOTE: training requires a lot of RAM (>16GB), and will be slow on non-GPU

1. Download the original 117M GPT-2 model by running
one of download_model.* scripts in External/Gradient-Samples/GPT-2
from the same directory.
2. Download and extract any lyrics dataset (I used 
[Every song you have heard (almost)!](https://www.kaggle.com/artimous/every-song-you-have-heard-almost)),
and unpack it if needed.
3. From the command line in the same directory (GPT-2), run
`dotnet run train --include *.csv --column Lyrics path/to/lyrics/folder --run Lyrics`
(change the `column` parameter to the name of the lyrics column in you dataset)
> NOTE: dev instance was trained with `train -i "*.csv" --column=Lyrics Downloads\every-song-you-have-heard-almost -r Lyrics --checkpoint=fresh --save-every=100 -n 3`.
> If training from IDE, set working directory to GPT-2
> (which should contain `models` subfolder downloaded previously).
4. Interrupt training process, when samples start looking good.
5. Try the trained model by running `dotnet run --run Lyrics`
