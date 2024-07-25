using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using InsightLogParser.Common.World;
using InsightLogParser.UI.Websockets;

namespace InsightLogParser.UI {
    public partial class Main : Form {
        private readonly Client _webSocketClient = new();

        private Coordinate _location;
        private Coordinate _target;
        private PuzzleType _puzzleType;
        private int _puzzleId;
        private int _routeNumber;
        private int _routeLength;
        
        public Main() {
            InitializeComponent();
            _webSocketClient.MessageReceived += WebSocketClient_OnMessageReceived;
            _ = _webSocketClient.ConnectAsync();
        }

        private void WebSocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            var message = e.Message;
            var data = JsonDocument.Parse(message);

            var type = data.RootElement.GetProperty("type").GetString();

            switch (type) {
                case "movePlayer": {
                    var destination = data.RootElement.GetProperty("destination");
                    var x = destination.GetProperty("X").GetSingle();
                    var y = destination.GetProperty("Y").GetSingle();
                    var z = destination.GetProperty("Z").GetSingle();
                    var location = new Coordinate(x, y, z);
                    MovePlayer(location);
                    break;
                }
                case "setTarget": {
                    var target = data.RootElement.GetProperty("target");
                    var x = target.GetProperty("X").GetSingle();
                    var y = target.GetProperty("Y").GetSingle();
                    var z = target.GetProperty("Z").GetSingle();
                    var puzzleType = data.RootElement.GetProperty("puzzleType").GetInt32();
                    var puzzleId = data.RootElement.GetProperty("puzzleId").GetInt32();
                    var routeNumber = data.RootElement.GetProperty("routeNumber").GetInt32();
                    var routeLength = data.RootElement.GetProperty("routeLength").GetInt32();
                    var location = new Coordinate(x, y, z);
                    SetTarget(location, (PuzzleType)puzzleType, puzzleId, routeNumber, routeLength);
                    break;
                }
            }
        }

        public void MovePlayer(Coordinate location) {
            _location = location;
            UpdateTarget();
        }

        public void SetTarget(Coordinate target, PuzzleType type, int id, int routeNumber, int routeLength) {
            _target = target;
            _puzzleType = type;
            _puzzleId = id;
            _routeNumber = routeNumber;
            _routeLength = routeLength;
            UpdateTarget();
        }

        private void UpdateTarget() {
            var assembly = Assembly.GetExecutingAssembly();

            var image = _puzzleType switch {
                PuzzleType.ArmillaryRings => Properties.Resources.ArmillaryRings,
                PuzzleType.CrystalLabyrinth => Properties.Resources.CrystalLabyrinth,
                PuzzleType.FlowOrbs => Properties.Resources.FlowOrbs,
                PuzzleType.GlideRings => Properties.Resources.GlideRings,
                PuzzleType.HiddenArchway => Properties.Resources.HiddenArchway,
                PuzzleType.HiddenCube => Properties.Resources.HiddenCube,
                PuzzleType.HiddenPentad => Properties.Resources.HiddenPentad,
                PuzzleType.HiddenRing => Properties.Resources.HiddenRing,
                PuzzleType.LightMotif => Properties.Resources.LightMotif,
                PuzzleType.LogicGrid => Properties.Resources.LogicGrid,
                PuzzleType.MatchBox => Properties.Resources.Matchbox,
                PuzzleType.MatchThree => Properties.Resources.MatchThree,
                PuzzleType.MemoryGrid => Properties.Resources.MemoryGrid,
                PuzzleType.MorphicFractal => Properties.Resources.MorphicFractal,
                PuzzleType.MusicGrid => Properties.Resources.MusicGrid,
                PuzzleType.PatternGrid => Properties.Resources.PatternGrid,
                PuzzleType.PhasicDial => Properties.Resources.PhasicDial,
                PuzzleType.RollingBlock => Properties.Resources.RollingBlock,
                PuzzleType.SentinelStones => Properties.Resources.SentinelStones,
                PuzzleType.ShiftingMosaic => Properties.Resources.ShiftingMosaic,
                PuzzleType.ShyAura => Properties.Resources.ShyAura,
                PuzzleType.SightSeer => Properties.Resources.Sightseer,
                PuzzleType.Skydrop => Properties.Resources.Skydrop,
                PuzzleType.WanderingEcho => Properties.Resources.WanderingEcho,
                _ => null,
            };

            picPuzzleType.Image = image;
            picPuzzleType.Update();

            try {
                var delta = _target - _location;
                var distance = _target.GetDistance2d(_location) / 100;

                var angleInRadians = Math.Atan2(delta.Y, delta.X);
                var angleInDegrees = angleInRadians * (180 / Math.PI);
                var normalizedAngle = (angleInDegrees + 360) % 360;
                var compassAngle = (normalizedAngle + 90) % 360;

                var compass = new Bitmap(Properties.Resources.CompassArrow.Width, Properties.Resources.CompassArrow.Height);

                using (Graphics g = Graphics.FromImage(compass)) {
                    g.TranslateTransform(compass.Width / 2, compass.Height / 2);
                    g.RotateTransform((float)compassAngle);
                    g.TranslateTransform(-compass.Width / 2, -compass.Height / 2);
                    g.DrawImage(Properties.Resources.CompassArrow, new Point(0, 0));
                }

                picCompass.Image = compass;
                picCompass.Update();

                lbl2DDistance.Text = $"{distance:F0}m";

                if (delta.Z < 0) {
                    picArrow.Image = Properties.Resources.Down;
                } else if (delta.Z > 0) {
                    picArrow.Image = Properties.Resources.Up;
                } else {
                    picArrow.Image = null;
                }
                picArrow.Update();

                lblVerticalDistance.Text = $"{Math.Abs(delta.Z / 100):F0}m";

                lblPuzzleType.Text = $"{Regex.Replace(_puzzleType.ToString(), "(\\B[A-Z])", " $1")}{(_routeLength > 0 ? $" ({_routeNumber}/{_routeLength})" : "")}";

                lblID.Text = _puzzleId.ToString();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
