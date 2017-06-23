# Introduction
I have been looking for a software that could gather news from various websites, display them in a very sober interface,
help me sort out old news from hot stuff, and wouldn't require lots of clicking everywhere. I ended up looking for feeds aggregators,
but was not satisfied because some of my favorite websites didn't offer RSS any feed.
So I started writing GetFacts.

# How it works
GetFacts downloads HTML (and hopefully other formats in a future release) and parses it with the help of a template. A template
is a small set of instructions (XPath's and Regex's, for instance) that cuts down the HTML document into multiple "sections" and
"articles". 
When its done, GetFacts displays the articles in its lightweight interface. Only a very few items are displayed at a time, but 
the display is updated every few secondes. Additional resources can also downloaded, such as accompanying pictures (movies and 
sounds are planned to be supported in a future release).
So GetFacts requires some efforts to be used, at least in its current state. Hopefully, one day a large community
will contribute and write templates for plenty of websites... ^___^ 


# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
