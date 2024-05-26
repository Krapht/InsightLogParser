Bugs.... expect bugs

### Requirements
You need the .NET 8 Runtime, available from Microsoft at https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Either the .NET Runtime or the .NET Desktop Runtime should do the trick

### So what is this?
This is a log parser for the game Islands of Insight that helps you track which world puzzles you have solved as the game doesn't really help you much in that regard. It is a console style application built in .NET 8 using C#

### So what does it do for me?
At the start, not much. There is no (non-intrusive) way to extract the puzzles you've already solved from the game which means the parser will start from scratch. As you solve puzzles it will track them and let you know if you've solved them before or not. For some puzzles like the puzzle boxes and puzzles on walls, it will be able to tell you if you've previously solved it by just opening the puzzles. For the others, you have to solve it first to know if it was a new solve or not. The solved puzzles will be saved locally in the same folder as the program in a file called db.json

Other features include
* Everytime you solve a puzzle it will give you some statistics. Again, note that the parser are not aware of any puzzles you've solved without it running
  * The number of times you solved this specific puzzle
  * The number of puzzles you've solved of this type in this zone during the current cycle
  * The total number of puzzles you've solved of this type in this zone
  * The total number of puzzles of this type in this zone
  * When the current cycle ends for this type of puzzle in this zone
* You can also use the command line menu to display statistics for a zone, to get an overview
* Everytime a puzzle cycles it will notify you of which puzzle type and where. It will also display the next upcoming cycle

### So how do I use it?
* Download the zip from the release section (or download the source code and compile it yourself)
* Extract it anywhere you like, no installation required
* Feel free to scan it with any anti-virus of your choice. (Just a note: I don't compile the binaries, there is a github action that builds, zips and publishes the binaries. They have never been on my computer)
* Run the .exe
* The parser will start up, wait for the game to start logging and then it will start parsing
  
### So what exactly are you doing on my computer?
The log file is located in your user's appdata folder so the parser will access that. It will read a couple of registry keys to find where steam is installed, and then read the steam config file to extract your steam library locations. It will then use these locations to check where the game is installed to read a puzzle.json file. Other than that, it will check if the game is running by listing all proceses called "IslandsofInsight". For online mode, it will also access your steam screenshot folder for Island of Insight. That's about it. Most, if not all, code accessing your computer should be in the UserComputer.cs file

The parser does not interact with steam or the game in any way. This limits what it can do to scraps of information gifted by the devs in the log file, but I won't be reading game memory or injecting code into the game. The log file (and puzzle.json file) should hopefully do.

### Configuration
After the first run, it will create a configuration file in the same folder as the program. This is a json file and can be modified with a text editor. Here you can override the various file paths as well as change the "beep"

### Online mode (completly optional)
Now this is where it gets interesting.

By entering a playerId, apikey and url in the config file, you'll enter "online mode", basically a crude form of crowdsourcing
* Solving a puzzle will notify the server which puzzle was solved and when
* Opening a puzzlebox or wall puzzle will also notify the server
* When parsing a puzzle you'll get additional statistics from the server
  * Puzzles other players running the parser have reported, but you haven't solved yet
  * The total number of puzzles other players have reported
* The zone statistics will be augmented with the above information
* Ability to upload screenshots linked to a puzzle you've solved
* Access to a super-basic web site (remember, I'm a backend dev so pixels isn't really my thing) so view some statistics and screenshots

Sometimes when a puzzle cycles the puzzles from the old cycle remain in the game world and can be solved. Once reconnected they will not be there anymore. I use the term "stale" to describe such reports. That means a puzzle was reported when the puzzle had cycled after the player connected. They should be treated as "maybe"s

### So exactly what "data" are you uploading from my computer?
Only stuff related to the puzzle/game like the puzzle id, the type of puzzle, the zone it was in, the server you're playing on. If you choose to upload screenshots, those will be uploaded together with the "thumbnail" file created by Steam.

I'm not really interested in anything else you might have on your computer :-)

### Why do I need an API key for online mode?
For reported content (puzzle reports, screenshots, ...) I want an easy way to purge someone's contributions from the data if they are not playing nice. So I need some form of crude authorization to tag contributions.

As for getting access to the data... Well since I have to make the authorization anyways I might as well require you to contribute if you want the data. "Earn your keep" so to say

### So how do I get an API key for this so called "online mode"?
Currently it is handled manually as I was some initial control of who can access it. But basically, you'll let me know you want access and I'll give you a PlayerId and an api-key once I've enabled access.

The apikey should be treated as a password, but it is NOT a password. It may look like a password, but it will be stored in plain text both in your configuration file and in my database. You won't be able to change it. Don't use it as a password anywhere else.

### Why screenshots?"
For world puzzles, taking screenshots of the locations will help people who have not yet found that puzzle find it. For puzzle boxes, you could look up the solition to puzzles if screenshots have been submitted for them

My idea here is that if you've done your look and found many, for example, matchboxes, but not all of them. You could check whihch matchboxes others have found today and by looking at the screenshots see where they are. This is especially useful as (currently) some puzzles do not appear on all servers. So if you can't find a puzzle on your server but someone else found it on a different server, you can see the server address for that

### How does this screenshot thing work?
The default configuration will keep an eye out for new Steam screenshots and the parser will display a menu asking you what to do with the screenshot. Here you can ignore, delete, upload or upload+delete the screenshot. There is also a quick-upload option.
The confirmation on screenshot deletion can be disabled in the configuration. It is also possible to automatically delete screenshots uploaded using the quick-uplolad

Screenshots can be taken before or after solving a puzzle. Generally you take the screenshot after, but for some puzzles, like Wandering Echoes, it makes more sense to take the screenshot before as the end location does nothing to help find the start

When uploading a screenshot you will be asked to categorize it with one of the following categories
* Initial - Basically the initial state of a puzzle, how it looks before you start solving it. This is applicable to Logic Grids, Pattern Grids, Match Three, Rolling blocks, Phasic Dials, Morphic Fractals and Shifting Mosaics
* Solved - This is the solved state of a puzzle, how it looks once it's fully solved. This is applicable to Logic Grids, Pattern Grids, Memory Grids, Music Grids, Sightseers, Light motifs, and Shifting Mosaics
* Location - This category is used to describe a location of a puzzle. Applicable to Hidden Archways, Hidden Pentads, Hidden Rings, Flow Orbs, Hidden Cubes, Shy Auraa, GlideR ings, Matchboxes and Light motifs
* Other - For any screenshot that doesn't fit another category

The quick-upload will determine the category based on which puzzle was solved.

When opening / solving a puzzle you will get information from the server if a screenshot of a specific category exists for that puzzle. If it is red, it means the screenshot a screenshot would be appreciated. If it is green it means a screenshot has been provided already. You can configure the parser to beep at you if you solve a puzzle that could use screenshot, but this is off by default.

### A note on deleting screenshots
Steam does this weird thing where it keeps an index file of all screenshots across all your games and deleting the screenshot file will not affect this index file. I have included a feature that will remove Islands of Insight screenshots that no longer exists on disk, but it should be considered experimental and could impact screenshots of other games. You can access this feature in the advanced manu in the parser. Please carefully read the instructions provided before proceeding

### In the pipeline 
Next up I'll expand the statistics and work on adding more puzzle screenshots to the library

Then Coordinates.... maybe
