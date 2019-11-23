using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Synapse_UI_WPF.Static;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Synapse_UI_WPF.Interfaces
{
    public static class WebSocketInterface
    {
        private static MainWindow Main;
        private static WebSocketServer Server;
        private static Data.WebSocketHolder Whitelist;
        private static Data.WebSocketTrustCache Cache;
        private static readonly List<string> TrustBlacklist = new List<string>();

        public class Execute : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                Main.Dispatcher.Invoke(() =>
                {
                    if (!Main.Ready())
                    {
                        Send("NOT_READY");
                        return;
                    }

                    Main.Execute(e.Data);
                    Send("OK");
                });
            }
        }

        public class Attach : WebSocketBehavior
        {
            public static bool Init;

            protected override void OnOpen()
            {
                if (Init) return;
                Main.InteractMessageRecieved += (sender, input) =>
                {
                    Sessions.Broadcast(input.Replace("SYN_", ""));
                };

                Init = true;
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                if (e.Data == "IS_READY")
                {
                    Send(Main.Ready() ? "TRUE" : "FALSE");
                    return;
                }

                if (e.Data != "ATTACH") return;

                Main.Dispatcher.Invoke(() =>
                {
                    if (Main.Ready())
                    {
                        Send("ALREADY_ATTACHED");
                        return;
                    }

                    Main.Attach();
                    Send("ATTEMPTING");
                });
            }
        }

        public class Editor : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                Main.Dispatcher.Invoke(() =>
                {
                    Main.SetEditor(e.Data);
                    Send("OK");
                });
            }
        }

        public class Custom : WebSocketBehavior
        {
            public string ChannelName;
            public bool SynapseChannel;

            protected override void OnOpen()
            {
                ChannelName = Context.QueryString["channel"];

                if (ChannelName == "INTERNAL_SYN_CHANNEL")
                {
                    SynapseChannel = true;
                }
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                if (SynapseChannel)
                {
                    var Split = e.Data.Split('|');
                    var RealChannel = Encoding.UTF8.GetString(Convert.FromBase64String(Split[0]));
                    var RealMessage = Encoding.UTF8.GetString(Convert.FromBase64String(Split[1]));

                    foreach (var Session in Sessions.Sessions)
                    {
                        if (!(Session is Custom cSession)) continue;
                        if (cSession.ID != ID && cSession.ChannelName == RealChannel)
                        {
                            cSession.Context.WebSocket.Send(RealMessage);
                        }
                    }
                }
                else
                {
                    foreach (var Session in Sessions.Sessions)
                    {
                        if (!(Session is Custom cSession)) continue;
                        if (cSession.ID == ID) continue;
                        if (cSession.ChannelName == ChannelName)
                        {
                            cSession.Context.WebSocket.Send(e.Data);
                        }
                        else if (cSession.SynapseChannel)
                        {
                            cSession.Context.WebSocket.Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(ChannelName)) + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(e.Data)));
                        }
                    }
                }
            }
        }

        public static void Start(int Port, MainWindow _Main)
        {
            Whitelist = WebInterface.GetWhitelistedDomains();

            if (DataInterface.Exists("trustcache"))
            {
                Cache = DataInterface.Read<Data.WebSocketTrustCache>("trustcache");
            }
            else
            {
                Cache = new Data.WebSocketTrustCache
                {
                    Entries = new List<string>()
                };
                DataInterface.Save("trustcache", Cache);
            }

            var Debug = Globals.Theme.Main.WebSocket.DebugMode;

            Main = _Main;
            Server = new WebSocketServer(Port);
            Server.AddWebSocketService("/execute", () =>
            {
                return new Execute
                {
                    OriginValidator = origin =>
                    {
                        if (origin == null || string.IsNullOrEmpty(origin)) return true;

                        if (Debug)
                        {
                            var Result = MessageBox.Show(
                                "Attempt to connect to the 'Execute' WebSocket.\n\nOrigin: \"" + origin +
                                "\"\n\nIf you wish to get this origin whitelisted, please press Control + C on this MessageBox and give the contents to 3dsboy08.\nPress 'Yes'/'No' to continue the connection.",
                                "Synapse X WebSocket - Debugger", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                            if (Result == MessageBoxResult.Yes)
                            {
                                return true;
                            }
                        }

                        foreach (var Entry in Whitelist.EntriesNoPrompt)
                        {
                            if (Entry == origin) return true;
                        }

                        foreach (var Entry in Whitelist.EntriesPrompt)
                        {
                            if (TrustBlacklist.Contains(Entry.Origin)) return false;
                            if (Cache.Entries.Contains(Entry.Origin)) return true;

                            if (Entry.Origin != origin) continue;
                            var Result = MessageBox.Show(
                                "Application '" + Entry.AppName + "' by developer '" + Entry.DevName +
                                "' wishes to connect to the Synapse WebSocket. Do you wish to allow the connection?\n\nBy pressing 'Yes', you give this application permission to execute scripts, attach Synapse, and set the text of the Synapse X editor of on your behalf.\n\nYou should NEVER allow connections with developers you do not trust. Press 'No' if you do not know what this means.",
                                "Synapse X Trust Manager", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No);
                            if (Result != MessageBoxResult.Yes)
                            {
                                TrustBlacklist.Add(origin);
                                return false;
                            }

                            Cache.Entries.Add(origin);
                            DataInterface.Save("trustcache", Cache);
                            return true;
                        }

                        return false;
                    }
                };
            });
            Server.AddWebSocketService("/attach", () =>
            {
                return new Attach
                {
                    OriginValidator = origin =>
                    {
                        if (origin == null || string.IsNullOrEmpty(origin)) return true;

                        if (Debug)
                        {
                            var Result = MessageBox.Show(
                                "Attempt to connect to the 'Attach' WebSocket.\n\nOrigin: \"" + origin +
                                "\"\n\nIf you wish to get this origin whitelisted, please press Control + C on this MessageBox and give the contents to 3dsboy08.\nPress 'Yes'/'No' to continue the connection.",
                                "Synapse X WebSocket - Debugger", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                            if (Result == MessageBoxResult.Yes)
                            {
                                return true;
                            }
                        }

                        foreach (var Entry in Whitelist.EntriesNoPrompt)
                        {
                            if (Entry == origin) return true;
                        }

                        foreach (var Entry in Whitelist.EntriesPrompt)
                        {
                            if (TrustBlacklist.Contains(Entry.Origin)) return false;
                            if (Cache.Entries.Contains(Entry.Origin)) return true;

                            if (Entry.Origin != origin) continue;
                            var Result = MessageBox.Show(
                                "Application '" + Entry.AppName + "' by developer '" + Entry.DevName +
                                "' wishes to connect to the Synapse WebSocket. Do you wish to allow the connection?\n\nBy pressing 'Yes', you give this application permission to execute scripts, attach Synapse, and set the text of the Synapse X editor of on your behalf.\n\nYou should NEVER allow connections with developers you do not trust. Press 'No' if you do not know what this means.",
                                "Synapse X Trust Manager", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No);
                            if (Result != MessageBoxResult.Yes)
                            {
                                TrustBlacklist.Add(origin);
                                return false;
                            }

                            Cache.Entries.Add(origin);
                            DataInterface.Save("trustcache", Cache);
                            return true;
                        }

                        return false;
                    }
                };
            });
            Server.AddWebSocketService("/editor", () =>
            {
                return new Editor
                {
                    OriginValidator = origin =>
                    {
                        if (origin == null || string.IsNullOrEmpty(origin)) return true;

                        if (Debug)
                        {
                            var Result = MessageBox.Show(
                                "Attempt to connect to the 'Editor' WebSocket.\n\nOrigin: \"" + origin +
                                "\"\n\nIf you wish to get this origin whitelisted, please press Control + C on this MessageBox and give the contents to 3dsboy08.\nPress 'Yes'/'No' to continue the connection.",
                                "Synapse X WebSocket - Debugger", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                            if (Result == MessageBoxResult.Yes)
                            {
                                return true;
                            }
                        }

                        foreach (var Entry in Whitelist.EntriesNoPrompt)
                        {
                            if (Entry == origin) return true;
                        }

                        foreach (var Entry in Whitelist.EntriesPrompt)
                        {
                            if (TrustBlacklist.Contains(Entry.Origin)) return false;
                            if (Cache.Entries.Contains(Entry.Origin)) return true;

                            if (Entry.Origin != origin) continue;
                            var Result = MessageBox.Show(
                                "Application '" + Entry.AppName + "' by developer '" + Entry.DevName +
                                "' wishes to connect to the Synapse WebSocket. Do you wish to allow the connection?\n\nBy pressing 'Yes', you give this application permission to execute scripts, attach Synapse, and set the text of the Synapse X editor of on your behalf.\n\nYou should NEVER allow connections with developers you do not trust. Press 'No' if you do not know what this means.",
                                "Synapse X Trust Manager", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No);
                            if (Result != MessageBoxResult.Yes)
                            {
                                TrustBlacklist.Add(origin);
                                return false;
                            }

                            Cache.Entries.Add(origin);
                            DataInterface.Save("trustcache", Cache);
                            return true;
                        }

                        return false;
                    }
                };
            });
            Server.AddWebSocketService("/custom", () =>
            {
                return new Custom
                {
                    OriginValidator = origin =>
                    {
                        if (origin == null || string.IsNullOrEmpty(origin)) return true;

                        if (Debug)
                        {
                            var Result = MessageBox.Show(
                                "Attempt to connect to the 'Custom' WebSocket.\n\nOrigin: \"" + origin +
                                "\"\n\nIf you wish to get this origin whitelisted, please press Control + C on this MessageBox and give the contents to 3dsboy08.\nPress 'Yes'/'No' to continue the connection.",
                                "Synapse X WebSocket - Debugger", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                            if (Result == MessageBoxResult.Yes)
                            {
                                return true;
                            }
                        }

                        foreach (var Entry in Whitelist.EntriesNoPrompt)
                        {
                            if (Entry == origin) return true;
                        }

                        foreach (var Entry in Whitelist.EntriesPrompt)
                        {
                            if (TrustBlacklist.Contains(Entry.Origin)) return false;
                            if (Cache.Entries.Contains(Entry.Origin)) return true;

                            if (Entry.Origin != origin) continue;
                            var Result = MessageBox.Show(
                                "Application '" + Entry.AppName + "' by developer '" + Entry.DevName +
                                "' wishes to connect to the Synapse WebSocket. Do you wish to allow the connection?\n\nBy pressing 'Yes', you give this application permission to execute scripts, attach Synapse, and set the text of the Synapse X editor of on your behalf.\n\nYou should NEVER allow connections with developers you do not trust. Press 'No' if you do not know what this means.",
                                "Synapse X Trust Manager", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No);
                            if (Result != MessageBoxResult.Yes)
                            {
                                TrustBlacklist.Add(origin);
                                return false;
                            }

                            Cache.Entries.Add(origin);
                            DataInterface.Save("trustcache", Cache);
                            return true;
                        }

                        return false;
                    }
                };
            });

            Server.Start();
        }

        public static void Stop()
        {
            Server.Stop();
        }
    }
}
