This project mainly serves as a demonstration of
[Gradient](https://losttech.software/gradient.html),
our TensorFlow binding for C# and other .NET languages.

Development instance can be accessed here:
[Billion Songs Dev](http://billion.dev.losttech.software:2095/).

> NOTE: this repository has git submodules.
> Learn about them [here](https://git-scm.com/book/en/v2/Git-Tools-Submodules).

# Run instructions

TBA

# Train instructions

TBA, but should be as easy as running GPT-2 project train command on .csv dataset.

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
