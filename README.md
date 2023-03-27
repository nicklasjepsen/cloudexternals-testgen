https://user-images.githubusercontent.com/2478876/227837527-6ffa3962-5010-439b-8559-9f3f70bf9985.mp4

# CloudExternals TestGen
## Introduction
This Visual Studio 2022 extension allow for rapid unit test generation of existing C# classes by using OpenAI.
The extension will take a C# class, and generate unit tests for each method in that class. It will then create or add the test to a unit test project inside the solution. The goal is that each generated unit test should be directly runnable after generation, but since this is AI, it's not always guarenteed that the tests compiles without small modifications.

The idea to this project arrose when I was writing unit test for an existing project and I found that sometimes it would be a lot easier to and efficient to build the test suite if I had a foundation to start from. So while this extension does not generate a full unit test suite for all your code, and can help you do so, one class at the time.

## Requirements
This project has early access, beta release written all over it, so treat it as such 😏
The goal is to support all currently supported .NET/.NET Framework versions, with different unit test frameworks, but to begin with, here's what we are aiming for supporting:

- .NET 6 
- xUnit
- Moq

> Please note that test generation will work best on files with a single class in it. 

## How it works
It's pretty simple, you trigger the extension by right clicking a C# class in the Solution Explorer in VS, then hit Generate Tests. The way the extension works is that it parses the file, removes all methods from the class, add one method at the time to the class and then asks OpenAI to generate unit tests for that stripped down class. Then it iterates each method repeating the process.

The reason behind this is the current token limit of OpenAI and the GPT models, which is rougly around 4000 tokens (small words), more about that here: https://platform.openai.com/docs/introduction/tokens - the token limit includes both the tokens in the request and response. 

## Getting started
At the time being there's no released VS Extension, so you would have to clone the repo, run from VS and then the Extension will appear in VS. 

> You need to provide your own OpenAI API key

Please go to: Tools > Options > CloudExternals > Enter your OpenAI API key here.
After doing so you can open up a solution, find a C# class, right click, Generate Tests.
