using InsightLogParser.Common.World;
using System.Linq;

namespace InsightLogParser.Client.Menu
{
    internal class MenuHandler
    {
        private readonly MessageWriter _messageWriter;
        private readonly Spider _spider;
        private readonly TaskCompletionSource _tcs;
        private readonly CancellationTokenSource _cts;
        private readonly Stack<IMenu> _menuStack = new();

        private Task _menuTask = Task.CompletedTask;

        public MenuHandler(CancellationToken forcedExitToken, MessageWriter messageWriter, Spider spider)
        {
            _messageWriter = messageWriter;
            _spider = spider;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(forcedExitToken);
            _tcs = new TaskCompletionSource();

            _menuStack.Push(new RootMenu(this, spider));
        }

        private async Task RunMenuAsync()
        {
            _messageWriter.WriteMenu(_menuStack.Peek().MenuOptions);
            while (true)
            {
                if (_cts.IsCancellationRequested)
                {
                    SignalMenuClosed();
                    return;
                }

                if (!Console.KeyAvailable)
                {
                    await Task.Delay(100, _cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                    continue;
                }

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    _menuStack.Pop();
                    if (_menuStack.Count == 0)
                    {
                        SignalMenuClosed();
                        return;
                    }
                    _messageWriter.WriteMenu(_menuStack.Peek().MenuOptions);
                    continue;
                }
                if (key.Key == ConsoleKey.H)
                {
                    _messageWriter.WriteMenu(_menuStack.Peek().MenuOptions);
                    continue;
                }
                if (! await _menuStack.Peek().HandleOptionAsync(key.KeyChar).ConfigureAwait(ConfigureAwaitOptions.None))
                {
                    _messageWriter.WriteLine($"Unknown command '{key.KeyChar}', press [h] to see available commands");
                }
            }
        }

        public Task StartAsync()
        {
            _menuTask = RunMenuAsync();
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _messageWriter.WriteDebug("Stopping menu");
            await _cts.CancelAsync().ConfigureAwait(ConfigureAwaitOptions.None);
            await _menuTask.ConfigureAwait(ConfigureAwaitOptions.None);
        }

        public async Task WaitForMenuClosed()
        {
            _messageWriter.WriteDebug("Waiting for menu to close");
            await _tcs.Task.ConfigureAwait(ConfigureAwaitOptions.None);
            _messageWriter.WriteDebug("Menu closed");
        }

        public void EnterMenu(IMenu newMenu)
        {
            _menuStack.Push(newMenu);
            _messageWriter.WriteMenu(newMenu.MenuOptions);
        }

        private void SignalMenuClosed()
        {
            _tcs.TrySetResult();
        }

        private IEnumerable<(char? key, string text)> ZoneOptions
        {
            get
            {
                yield return (null, "Please pick a zone or any other key to go back");
                yield return ('1', "Verdant Glen");
                yield return ('2', "Lucent Waters");
                yield return ('3', "Autumn Falls");
                yield return ('4', "Shady Wildwood");
                yield return ('5', "Serene Deluge");
            }
        }

        private IEnumerable<(char? key, string text)> WorldPuzzleOptions
        {
            get
            {
                yield return (null, "Please pick a world puzzle type or any other key to go back");
                yield return ('1', "Matchboxes");
                yield return ('2', "Light Motifs");
                yield return ('3', "Sightseers");
                yield return ('4', "Hidden Rings");
                yield return ('5', "Hidden Cubes");
                yield return ('6', "Hidden Archways");
                yield return ('7', "Hidden Pentads");
                yield return ('8', "Wandering Echoes");
                yield return ('8', "Glide Rings");
                yield return ('a', "Flow Orbs");
                yield return ('a', "Shy Auras");
            }
        }

        public PuzzleZone PickZone()
        {
            _messageWriter.WriteMenu(ZoneOptions);
            var key = Console.ReadKey(true);
            switch (key.KeyChar)
            {
                case '1': return PuzzleZone.VerdantGlen;
                case '2': return PuzzleZone.LucentWaters;
                case '3': return PuzzleZone.AutumnFalls;
                case '4': return PuzzleZone.ShadyWildwood;
                case '5': return PuzzleZone.SereneDeluge;
                default: return PuzzleZone.Unknown;
            }
        }

        public PuzzleType PickWorldPuzzleType()
        {
            _messageWriter.WriteMenu(WorldPuzzleOptions);
            var key = Console.ReadKey(true);
            switch (key.KeyChar)
            {
                case '1': return PuzzleType.MatchBox;
                case '2': return PuzzleType.LightMotif;
                case '3': return PuzzleType.SightSeer;
                case '4': return PuzzleType.HiddenRing;
                case '5': return PuzzleType.HiddenCube;
                case '6': return PuzzleType.HiddenArchway;
                case '7': return PuzzleType.HiddenPentad;
                case '8': return PuzzleType.WanderingEcho;
                case '9': return PuzzleType.GlideRings;
                case 'a': return PuzzleType.FlowOrbs;
                case 'b': return PuzzleType.ShyAura;
                default: return PuzzleType.Unknown;
            }
        }

    }
}
