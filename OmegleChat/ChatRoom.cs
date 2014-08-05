using OmegleChatRoom.Event;
using OmegleChatRoom.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegleChatRoom
{
    /// <summary>
    /// Encapsulates an Omegle chat room.
    /// </summary>
    public class ChatRoom
    {
        public string ConnectMessage = "Welcome to the Omegle multi-chat room! Your name is {0}. Type \"/help\" for more information.";
        public List<IUser> Users;
        public LocalUser Admin;
        public bool AutoReconnect;
        public bool CommandEngineEnabled;
        public bool PrependUsernames;

        public event StatusChanged StatusChanged;
        public event MessageReceivedEvent NewMessage;

        private Dictionary<string, Command> StandardCommands;
        private Dictionary<string, Command> ModCommands;

        delegate string Command(IUser user, string[] args);

        public ChatRoom(LocalUser admin)
        {
            Admin = admin;

            Users = new List<IUser>();
            Users.Add(Admin);

            StandardCommands = new Dictionary<string, Command>();
            ModCommands = new Dictionary<string, Command>();

            StandardCommands.Add("help", Help);
            StandardCommands.Add("name", Nick);
            StandardCommands.Add("nick", Nick);
            StandardCommands.Add("who", Who);
            StandardCommands.Add("w", Whisper);
            StandardCommands.Add("pm", Whisper);
            StandardCommands.Add("me", Me);

            ModCommands.Add("kick", Kick);
            ModCommands.Add("ban", Kick);
            ModCommands.Add("mute", Mute);
            ModCommands.Add("rename", Rename);
            ModCommands.Add("mod", Mod);
            ModCommands.Add("op", Mod);
            ModCommands.Add("demod", Demod);
            ModCommands.Add("deop", Demod);
            ModCommands.Add("connect", Connect);
        }

        private void NewUser()
        {
            RemoteUser user = new RemoteUser();

            user.WaitingForPartner += OnWaitingForPartner;
            user.Connected += OnConnected;
            user.MessageReceived += OnMessageReceived;
            user.StrangerDisconnected += OnStrangerDisconnected;
            user.CaptchaRequired += OnCaptchaRequired;
            user.CaptchaRefused += OnCaptchaRefused;
            user.UnhandledResponse += Omegle_UnhandledResponse;
            user.StrangerTyping += OnStrangerTyping;
            user.StrangerStoppedTyping += OnStrangerTyping;

            user.Connect();
        }

        private void RemoveUser(RemoteUser user)
        {
            if (user.Omegle.IsConnected)
            {
                user.Disconnect();
            }

            Users.Remove(user);
        }

        /// <summary>
        /// Sends a message from the Admin to all other users. Runs a command by the Admin if the message is prefixed with a '/' character.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage(string message)
        {
            if (CommandEngineEnabled && message.Length > 0 && message[0] == '/')
            {
                message = message.Remove(0, 1);
                string command = message.Split(' ')[0];

                message = message.Remove(0, command.Length).Trim();

                string[] commandArgs = message.Split(' ');
                UserCommand(Admin, command, commandArgs);
            }
            else
            {
                DisplayMessage(Admin, message);
                Broadcast(String.Format("{0}: {1}", Admin.Name, message), Admin);
            }
        }

        /// <summary>
        /// Broadcasts a message to all users except a specified sender.
        /// </summary>
        /// <param name="message">The message to be broadcasted.</param>
        /// <param name="sender">The sender who will not receive this message. Pass null to broadcast to all users.</param>
        public void Broadcast(string message, IUser sender)
        {
            foreach (IUser u in Users)
            {
                if (u != sender || (u is LocalUser && sender != null))
                {
                    u.SendMessage(message);
                }
            }
        }

        private void DisplayMessage(string message)
        {
            if (NewMessage != null)
                NewMessage(this, new MessageEventArgs(message));
        }

        private void DisplayMessage(IUser user, string message)
        {
            if (NewMessage != null)
                NewMessage(user, new MessageEventArgs(message));
        }

        /// <summary>
        /// Returns the first occurence of a user with a specified name.
        /// </summary>
        /// <param name="name">The name to find.</param>
        /// <returns>A user with that name.</returns>
        public IUser GetUserByName(string name)
        {
            foreach (IUser user in Users)
            {
                if (user.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return user;
            }

            return null;
        }

        #region ChatEvents

        private void OnStrangerTyping(object sender, EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(sender, new StatusChangedEventArgs());
        }

        private void OnWaitingForPartner(object sender, EventArgs e)
        {
            DisplayMessage("Searching for a stranger...");
        }

        private void OnConnected(object sender, EventArgs e)
        {
            RemoteUser user = sender as RemoteUser;

            Users.Add(user);

            DisplayMessage(String.Format("{0} connected!", user.Name));

            user.SendMessage(String.Format(ConnectMessage, user.Name));

            Broadcast(String.Format("{0} connected!", user.Name), user);

            if (StatusChanged != null)
                StatusChanged(sender, new StatusChangedEventArgs());
        }

        private void OnStrangerDisconnected(object sender, EventArgs e)
        {
            RemoteUser user = sender as RemoteUser;

            DisplayMessage(String.Format("{0} disconnected!", user.Name));

            RemoveUser(user);

            if (AutoReconnect)
                NewUser();

            Broadcast(String.Format("{0} disconnected!", user.Name), null);

            if (StatusChanged != null)
                StatusChanged(sender, new StatusChangedEventArgs());
        }

        private void OnMessageReceived(object sender, MessageEventArgs args)
        {
            RemoteUser user = sender as RemoteUser;

            DisplayMessage(user, args.Message);

            if (args.Message.Length > 0 && args.Message[0] == '/')
            {
                string message = args.Message.Remove(0, 1);
                string command = message.Split(' ')[0];

                message = message.Remove(0, command.Length).Trim();

                string[] commandArgs = message.Split(' ');
                UserCommand(user, command.ToLower(), commandArgs);
            }
            else if (!user.IsMuted)
            {
                Broadcast(String.Format("{0}: {1}", user.Name, args.Message), user);
            }

            if (StatusChanged != null)
                StatusChanged(sender, new StatusChangedEventArgs());
        }

        private void OnCaptchaRequired(object sender, CaptchaRequiredArgs e)
        {
            DisplayMessage(String.Format("Captcha! {0}", e.Url));

            // TODO: Handle captcha
        }

        private void OnCaptchaRefused(object sender, EventArgs e)
        {
            DisplayMessage("Captcha refused!");
        }

        private void Omegle_UnhandledResponse(object sender, UnhandledResponseEventArgs e)
        {
            DisplayMessage(String.Format("Unhandled response: {0}!", e.Response));

            RemoteUser user = sender as RemoteUser;

            if (!user.Omegle.IsConnected || e.Response.Contains("error"))
            {
                RemoveUser(user);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Performs a command on behalf of a user.
        /// </summary>
        /// <param name="user">The user performing the command.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">Any command arguments.</param>
        public void UserCommand(IUser user, string command, string[] args)
        {
            if (user.PriviledgeLevel == PriviledgeLevel.Mod)
            {
                foreach (string key in ModCommands.Keys)
                {
                    if (command.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string response = ModCommands[key](user, args);

                        if (response != String.Empty)
                        {
                            if (user is LocalUser)
                            {
                                DisplayMessage(response);
                            }
                            else
                            {
                                user.SendMessage(response);
                            }
                        }

                        if (StatusChanged != null)
                            StatusChanged(this, new StatusChangedEventArgs());

                        return;
                    }
                }
            }

            if (user.PriviledgeLevel == PriviledgeLevel.Standard || user.PriviledgeLevel == PriviledgeLevel.Mod)
            {
                foreach (string key in StandardCommands.Keys)
                {
                    if (command.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string response = StandardCommands[key](user, args);

                        if (response != String.Empty)
                        {
                            if (user is LocalUser)
                            {
                                DisplayMessage(response);
                            }
                            else
                            {
                                user.SendMessage(response);
                            }
                        }

                        if (StatusChanged != null)
                            StatusChanged(this, new StatusChangedEventArgs());

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// A user command. Displays a help message.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">Not used in this command (can pass null).</param>
        /// <returns>The command response to the user.</returns>
        public string Help(IUser user, string[] args)
        {
            string response = "Hey! Welcome to the multi-chat. This isn't your average 1 on 1 conversation!\r\n"
                + "Available commands:\r\n"
                + "/help --displays this message\r\n"
                + "/name newName --changes your name to newName\r\n"
                + "/who --lists all other users\r\n"
                + "/w userName message --whispers a private message to userName\r\n"
                + "/me";

            if (user.PriviledgeLevel == PriviledgeLevel.Mod)
            {
                response += "\r\n/rename userName newName --changes userName's name to newName\r\n"
                    + "/mute userName --mutes userName\r\n"
                    + "/kick userName --kicks userName\r\n"
                    + "/connect --connects a new user to the chat";
            }

            return response;
        }

        /// <summary>
        /// A user command. Changes the calling user's name.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">One argument expected. args[0]=NewName.</param>
        /// <returns>The command response to the user.</returns>
        public string Nick(IUser user, string[] args)
        {
            string response = String.Empty;

            if (args.Length >= 1 && args[0].Length > 0 && !(args[0].Equals(Admin.Name, StringComparison.InvariantCultureIgnoreCase) && user is RemoteUser))
            {
                string oldName = user.Name;
                user.Name = args[0];
                //response = String.Format("Name changed to {0}!", user.Name);
                DisplayMessage(String.Format("{0}'s name is now {1}!", oldName, user.Name));
                Broadcast(String.Format("{0}'s name is now {1}!", oldName, user.Name), null);
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Displays a list of connected users.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">Not used in this command (can pass null).</param>
        /// <returns>The command response to the user.</returns>
        public string Who(IUser user, string[] args)
        {
            string response = String.Format("{0} users: ", Users.Count);

            foreach (IUser u in Users)
            {
                response += u.Name + ", ";
            }

            return response.TrimEnd(new char[] { ',', ' ' });
        }

        /// <summary>
        /// A user command. Sends a private message to another user.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">At least two arguments expected. args[0]=RecipientName, args[1..n]=Message.</param>
        /// <returns>The command response to the user.</returns>
        public string Whisper(IUser user, string[] args)
        {
            string response;

            if (args.Length >= 2)
            {
                IUser recipient = GetUserByName(args[0]);

                if (recipient != null)
                {
                    args[0] = "";
                    string message = String.Join(" ", args).Trim();

                    recipient.SendMessage(String.Format("{0} whispered: {1}", user.Name, message));

                    response = "Message sent!";
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Displays an "action message" or "emote".
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">At least one argument expected. All args are the message.</param>
        /// <returns>The command response to the user.</returns>
        public string Me(IUser user, string[] args)
        {
            string message = String.Join(" ", args);

            if (message.Length > 0)
            {
                DisplayMessage(String.Format("{0} {1}", user.Name, message));
                Broadcast(String.Format("{0} {1}", user.Name, message), null);
            }
            else
            {
                return "Expected argument(s).";
            }

            return String.Empty;
        }

        /// <summary>
        /// A user command. Kicks another user.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">1 argument expected. args[0]=UserToKick.</param>
        /// <returns>The command response to the user.</returns>
        public string Kick(IUser user, string[] args)
        {
            string response = String.Empty;

            if (args.Length >= 1)
            {
                RemoteUser kickee = GetUserByName(args[0]) as RemoteUser;

                if (kickee != null)
                {
                    if (kickee.PriviledgeLevel != PriviledgeLevel.Mod)
                    {
                        RemoveUser(kickee);

                        Broadcast(String.Format("{0} was kicked!", kickee.Name), null);
                        DisplayMessage(String.Format("{0} was kicked!", kickee.Name));
                    }
                    else
                    {
                        response = "You can't kick a mod!";
                    }
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Mutes another user.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">1 argument expected. args[0]=UserToMute.</param>
        /// <returns>The command response to the user.</returns>
        public string Mute(IUser user, string[] args)
        {
            string response = "";

            if (args.Length >= 1)
            {
                IUser mutee = GetUserByName(args[0]);

                if (mutee != null)
                {
                    if (mutee.PriviledgeLevel != PriviledgeLevel.Mod)
                    {
                        mutee.IsMuted = !mutee.IsMuted;

                        Broadcast(String.Format("{0} was {1}muted!", mutee.Name, mutee.IsMuted ? "" : "un"), null);
                        DisplayMessage(String.Format("{0} was {1}muted!", mutee.Name, mutee.IsMuted ? "" : "un"));
                    }
                    else
                    {
                        response = "You can't mute a mod!";
                    }
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Renames another user.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">2 arguments expected. args[0]=UserToRename, args[1]=NewName</param>
        /// <returns>The command response to the user.</returns>
        public string Rename(IUser user, string[] args)
        {
            string response = String.Empty;

            if (args.Length >= 2)
            {
                IUser renamed = GetUserByName(args[0]);

                if (renamed != null)
                {
                    if (renamed.PriviledgeLevel != PriviledgeLevel.Mod)
                    {
                        Nick(renamed, new string[] { args[1] });

                        renamed.SendMessage((user != null ? user.Name : Admin.Name) + " changed your name to " + renamed.Name + "!");
                    }
                    else
                    {
                        response = "You can't rename a mod!";
                    }
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Promotes a user to moderator.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">1 argument expected. args[0]=UserToPromote</param>
        /// <returns>The command response to the user.</returns>
        public string Mod(IUser user, string[] args)
        {
            string response = String.Empty;

            if (args.Length >= 1)
            {
                IUser mod = GetUserByName(args[0]);

                if (mod != null)
                {
                    if (mod.PriviledgeLevel != PriviledgeLevel.Mod)
                    {
                        mod.PriviledgeLevel = PriviledgeLevel.Mod;

                        Broadcast(String.Format("{0} is now a mod!", mod.Name), null);
                        DisplayMessage(String.Format("{0} is now a mod!", mod.Name));
                    }
                    else
                    {
                        response = String.Format("{0} is already a mod!", mod.Name);
                    }
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Demotes a user from moderator.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">1 argument expected. args[0]=UserToDemote.</param>
        /// <returns>The command response to the user.</returns>
        public string Demod(IUser user, string[] args)
        {
            string response = String.Empty;

            if (args.Length >= 1)
            {
                IUser mod = GetUserByName(args[0]);

                if (mod != null)
                {
                    if (mod.PriviledgeLevel == PriviledgeLevel.Mod)
                    {
                        if (mod is LocalUser)
                        {
                            response = "You can't demod an admin!";
                        }
                        else
                        {
                            mod.PriviledgeLevel = PriviledgeLevel.Standard;

                            Broadcast(String.Format("{0} is no longer a mod!", mod.Name), null);
                            DisplayMessage(String.Format("{0} is no longer a mod!", mod.Name));
                        }
                    }
                    else
                    {
                        response = String.Format("{0} isn't a mod!", mod.Name);
                    }
                }
                else
                {
                    response = String.Format("{0} is not a valid user.", args[0]);
                }
            }
            else
            {
                response = "Invalid argument(s).";
            }

            return response;
        }

        /// <summary>
        /// A user command. Connects a new user to the chat room.
        /// </summary>
        /// <param name="user">The calling user.</param>
        /// <param name="args">Not used in this command (can pass null).</param>
        /// <returns>The command response to the user.</returns>
        public string Connect(IUser user, string[] args)
        {
            NewUser();

            return String.Empty;
        }

        #endregion
    }
}
