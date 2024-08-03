using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using InsightLogParser.Common.World;
using InsightLogParser.UI.Websockets;

namespace InsightLogParser.UI {
    public partial class Main : Form {
        private readonly Client _webSocketClient = new();

        private readonly string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InsightLogParser", "cache");

        private Coordinate _location;
        private Coordinate _target;
        private PuzzleType _puzzleType;
        private int _puzzleId;
        private int _routeNumber;
        private int _routeLength;
        
        public Main(int port) {
            InitializeComponent();
            _webSocketClient.MessageReceived += WebSocketClient_OnMessageReceived;
            _ = _webSocketClient.ConnectAsync(port);
            RemoveOldScreenshots();
        }

        private async void WebSocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e) {
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
                    await SetTarget(location, (PuzzleType)puzzleType, puzzleId, routeNumber, routeLength);
                    break;
                }
                case "shutdown":
                    Application.Exit();
                    break;
                case "screenshot": {
                    var puzzleId = data.RootElement.GetProperty("puzzleId").GetInt32();
                    var url = data.RootElement.GetProperty("url").GetString();
                    await DownloadScreenshot(puzzleId, url);
                    break;
                }
            }
        }

        public void MovePlayer(Coordinate location) {
            _location = location;
            UpdateTarget();
        }

        public async Task SetTarget(Coordinate target, PuzzleType type, int id, int routeNumber, int routeLength) {
            _target = target;
            _puzzleType = type;
            _routeNumber = routeNumber;
            _routeLength = routeLength;
            var oldPuzzleId = _puzzleId;
            _puzzleId = id;
            UpdateTarget();

            if (id != 0) {
                if (id == oldPuzzleId) {
                    return;
                }

                // Clear the current image
                if (picScreenshot.InvokeRequired) {
                    picScreenshot.Invoke(new Action(() => {
                        picScreenshot.Image?.Dispose();
                        picScreenshot.Image = null;
                        picScreenshot.Update();
                    }));
                } else {
                    picScreenshot.Image?.Dispose();
                    picScreenshot.Image = null;
                    picScreenshot.Update();
                }

                // Check if the screenshot has already been cached.
                var cachePath = Path.Combine(cacheDirectory, $"{id}.png");
                if (File.Exists(cachePath)) {
                    if (picScreenshot.InvokeRequired) {
                        picScreenshot.Invoke(new Action(() => {
                            picScreenshot.Image = Image.FromFile(cachePath);
                            picScreenshot.Update();
                        }));
                    } else {
                        picScreenshot.Image = Image.FromFile(cachePath);
                        picScreenshot.Update();
                    }
                    return;
                }

                // Otherwise, request the screenshot from the server.
                try {
                    await _webSocketClient.SendAsync(new {
                        type = "screenshot",
                        puzzleId = id
                    });
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void UpdateTarget() {
            if (_target.X == 0 && _target.Y == 0 && _target.Z == 0) {
                return;
            }

            if (_location.X == 0 && _location.Y == 0 && _location.Z == 0) {
                return;
            }

            var distance = _target.GetDistance2d(_location) / 100;

            if (distance == 0) {
                return;
            }

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

            picPuzzleType.Image?.Dispose();
            picPuzzleType.Image = image;
            picPuzzleType.Update();

            try {
                var delta = _target - _location;

                var angleInRadians = Math.Atan2(delta.Y, delta.X);
                var angleInDegrees = angleInRadians * (180 / Math.PI);
                var normalizedAngle = (angleInDegrees + 360) % 360;
                var compassAngle = (normalizedAngle + 90) % 360;

                using (var compass = new Bitmap(Properties.Resources.CompassArrow.Width, Properties.Resources.CompassArrow.Height))
                using (Graphics g = Graphics.FromImage(compass)) {
                    g.TranslateTransform(compass.Width / 2, compass.Height / 2);
                    g.RotateTransform((float)compassAngle);
                    g.TranslateTransform(-compass.Width / 2, -compass.Height / 2);
                    g.DrawImage(Properties.Resources.CompassArrow, new Point(0, 0));
                    picCompass.Image?.Dispose();
                    picCompass.Image = (Bitmap)compass.Clone();
                }

                picCompass.Update();

                lbl2DDistance.Text = $"{distance:F0}m";

                picArrow.Image?.Dispose();
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

                Application.DoEvents();

                var locationX = (_location.X + 102844) / 32.88 + 1450;
                var locationY = (_location.Y + 104171) / 32.96 + 572;
                var targetX = (_target.X + 102844) / 32.88 + 1450;
                var targetY = (_target.Y + 104171) / 32.96 + 572;
                var centerX = (locationX + targetX) / 2;
                var centerY = (locationY + targetY) / 2;

                // Load the full map
                var fullMap = Properties.Resources.Map;

                // Calculate the distance between location and target
                var distanceX = Math.Abs(locationX - targetX);
                var distanceY = Math.Abs(locationY - targetY);

                // Add buffer to the distance
                var buffer = 100; // Buffer in pixels
                var maxDistance = Math.Max(distanceX, distanceY);
                var zoomSize = maxDistance + 2 * buffer;

                // Ensure the zoom dimensions are at least as large as the picture box dimensions
                var aspectRatio = (float)picMap.Width / picMap.Height;
                if (aspectRatio > 1) {
                    zoomSize = Math.Max(zoomSize, picMap.Width);
                } else {
                    zoomSize = Math.Max(zoomSize, picMap.Height);
                }

                // Ensure we don't zoom in too much
                zoomSize = Math.Min(zoomSize, Math.Min(fullMap.Width, fullMap.Height));

                // Adjust zoomRect to maintain aspect ratio
                var zoomWidth = zoomSize;
                var zoomHeight = zoomSize / aspectRatio;
                if (zoomHeight > fullMap.Height) {
                    zoomHeight = fullMap.Height;
                    zoomWidth = zoomHeight * aspectRatio;
                }

                var zoomRect = new Rectangle((int)(centerX - zoomWidth / 2), (int)(centerY - zoomHeight / 2), (int)Math.Round(zoomWidth), (int)Math.Round(zoomHeight));

                // Create a bitmap for the zoomed map
                using (var zoomedMap = new Bitmap(picMap.Width, picMap.Height))
                using (Graphics g = Graphics.FromImage(zoomedMap)) {
                    // Draw the zoomed area of the map
                    g.DrawImage(fullMap, new Rectangle(0, 0, picMap.Width, picMap.Height), zoomRect, GraphicsUnit.Pixel);

                    // Draw a green line from the location to the target
                    using (var pen = new Pen(Color.Aqua, 3)) {
                        var startX = (int)((locationX - zoomRect.X) * picMap.Width / zoomRect.Width);
                        var startY = (int)((locationY - zoomRect.Y) * picMap.Height / zoomRect.Height);
                        var endX = (int)((targetX - zoomRect.X) * picMap.Width / zoomRect.Width);
                        var endY = (int)((targetY - zoomRect.Y) * picMap.Height / zoomRect.Height);
                        g.DrawLine(pen, new Point(startX, startY), new Point(endX, endY));
                    }

                    // Draw the location as the compass
                    var mapCompass = new Bitmap(picCompass.Image, new Size(picCompass.Image.Width / 2, picCompass.Image.Height / 2));
                    var compassX = (int)((locationX - zoomRect.X) * picMap.Width / zoomRect.Width - mapCompass.Width / 2);
                    var compassY = (int)((locationY - zoomRect.Y) * picMap.Height / zoomRect.Height - mapCompass.Height / 2);
                    g.DrawImage(mapCompass, new Point(compassX, compassY));

                    // Draw the target as the current marker
                    var currentMarker = new Bitmap(Properties.Resources.CurrentMarker, new Size(Properties.Resources.CurrentMarker.Width / 4, Properties.Resources.CurrentMarker.Height / 4));
                    var markerX = (int)((targetX - zoomRect.X) * picMap.Width / zoomRect.Width - currentMarker.Width / 2);
                    var markerY = (int)((targetY - zoomRect.Y) * picMap.Height / zoomRect.Height - currentMarker.Height / 2);
                    g.DrawImage(currentMarker, new Point(markerX, markerY));

                    picMap.Image?.Dispose();
                    picMap.Image = (Bitmap)zoomedMap.Clone();
                }

                // Set the zoomed map to picMap
                picMap.Update();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

            GC.Collect();
        }

        private async Task DownloadScreenshot(int puzzleId, string url) {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            var image = Image.FromStream(stream);
            
            // Cache the image to disk.
            Directory.CreateDirectory(cacheDirectory);
            var cachePath = Path.Combine(cacheDirectory, $"{puzzleId}.png");
            image.Save(cachePath);

            if (InvokeRequired) {
                Invoke(new Action(() => {
                    picScreenshot.Image?.Dispose();
                    picScreenshot.Image = image;
                    picScreenshot.Update();
                }));
            } else {
                picScreenshot.Image?.Dispose();
                picScreenshot.Image = image;
                picScreenshot.Update();
            }
        }

        private void RemoveOldScreenshots() {
            // Remove any files older than 24 hours from the cache directory.
            if (Directory.Exists(cacheDirectory)) {
                var files = Directory.GetFiles(cacheDirectory);
                foreach (var file in files) {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)) {
                        fileInfo.Delete();
                    }
                }
            }
        }
    }
}
