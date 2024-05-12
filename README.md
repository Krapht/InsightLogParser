Bugs.... expect bugs

### Requirements
You need the .NET 8 Runtime, available from Microsoft at https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Either the .NET Runtime or the .NET Desktop Runtime should do the trick

### So what is this?
This is a log parser for the game Islands of Insight that helps you track which world puzzles you have solved as the game doesn't really help you much in that regard. It is a console style application built in .NET 8 using C#

### So what does it do for me?
At the start, not much. There is no (easy) way to extract the puzzles you've already solved from the game which means the parser will start from scratch. As you solve puzzles it will track them and let you know if you've solved them before or not. For some puzzles like the puzzle boxes and puzzles on walls, it will be able to tell you if you've previously solved it just by opening the puzzles. For the others, you have to solve it first to know if it was a new solve or not. The solved puzzles will be saved locally in the same folder as the program in a file called db.json

Other features include
* Everytime you solve a puzzle it will give you some statistics. Again, note that the parser are not aware of any puzzles you've solved without it running
  * The number of times you parsed this specific puzzle
  * The number of puzzles you've parsed of this type in this zone during the current cycle
  * The total number of puzzles you've parsed of this type in this zone
  * The total number of puzzles of this type in this zone
  * When the current cycle ends for this type of puzzle in this zone
* You can also use the command line menu to display statistics for zone, to get an overview
* Everytime a puzzle cycles it will notify you of which puzzle type and where. It will also display the next upcoming cycle

### So how do I use it?
* Download the zip from the release section (or download the source code and compile it yourself)
* Extract it anywhere you like
* Feel free to scan it with any anti-virus of your choice. (Just a note: I don't compile the binaries, there is a github action that builds, zips and publishes the binaries. They have never been on my computer)
* Run the .exe
* The parser will start up, wait for the game to start logging and then it will start parsing
  
### So what exactly are you doing on my computer?
The log file is located in your user's appdata folder so the parser will access that. It will read a couple of registry keys to find where steam is installed, and then read the steam config file to extract your steam library locations. It will then use these locations to check where the game is installed to read a puzzle.json file. Other than that, it will check if the game is running by listing all proceses called "IslandsofInsight". That's about it. Most, if not all, code accessing your computer should be in the UserComputer.cs file

The parser does not interact with steam or the game in any way. This limits what it can do to scraps of information gifted by the devs in the log file, but I won't be reading game memory or injecting code into the game. The log file (and puzzle.json file) should hopefully do.

### Configuration
After the first run, it will create a configuration file in the same folder as the program. This is a json file and can be modified with a text editor. Here you can override the various file paths as well as change the "beep"

### Online mode (completly optional)
Now this is where it gets interesting.

By entering an apikey and url in the config file, you'll enter "online mode", basically a crude form of crowdsourcing
* Solving a puzzle will notify the server which puzzle was solved and when
* Opening a puzzlebox or wall puzzle will also notify the server
* When parsing a puzzle you'll get additional statistics from the server
  * Puzzles other players running the parser have reported, but you haven't parsed yet
  * The total number of puzzles other players have reported
* The zone statistics will be augmented with the above information

Sometimes when a puzzle cycles the puzzles from the old cycle remain in the game world and can be solved. Once reconnected they will not be there anymore. I use the term "stale" to describe such reports. That means a puzzle was reported when the puzzle had cycled after the player connected. They should be treated as "maybe"s

### So exactly what "data" are you uploading from my computer?
Only stuff related to the puzzle/game like the puzzle id, the type of puzzle, the zone it was in, the server you're playing on. I'm not really interested in anything else you might have on your computer :-)

### Why do I need an API key for online mode?
For reported content (puzzle reports, but in the future also screenshots) I want an easy way to purge someone's contributions from the data if they are not playing nice. So I need some form of crude authorization to tag contributions.

As for getting access to the data... Well since I have to make the authorization anyways I might as well require you to contribute if you want the data. "Earn your keep" so to say

### So how do I get an API key for this so called "online mode"?
Currently it is handled manually as I was some initial control of who can access it. But basically, you give me your "cloud id" from the accounts page in-game and I'll give you an api-key once I've enabled access.

From what I can tell, this "cloud id" is the same as the steam id for your steam account and afaik, not considered private information.

The apikey should be treated as a password, but it is NOT a password. It may look like a password, but it will be stored in plain text both in your configuration file and in my database. You won't be able to change it. Don't use it as a password anywhere else.

### In the pipeline 
I do plan to add a screenshot upload feature to the online part. I want to make it as smooth as possible. The plan is to monitor the steam screenshot folder to detect the latest screenshot and then have a command you can use to "tag" that screenshot with the puzzle id of the puzzle you just solved as well as a type of screenshot and upload it straight from the parser. 

Basically, if you find a particullary sneaky hidden ring and want to upload a screenshot of it, it should be as simple as pressing F12 to take a steam screenshot of ring, alt-tab to the parser, hit a key to say "upload please" and then perhaps another key to select a "location" type of screenshot. Then alt-tab back to the game

Somthing like this



Then Coordinates.... maybe
