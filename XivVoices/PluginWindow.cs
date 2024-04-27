﻿using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using System.Threading.Tasks;
using XivVoices.Engine;
using System.IO;
using System.Linq;

namespace XivVoices {
    public class PluginWindow : Window {
        private Configuration configuration;
        private IClientState clientState;

        private string managerNullMessage = string.Empty;
        private bool SizeYChanged = false;
        private bool managerNull;
        private Vector2? initialSize;
        private Vector2? changedSize;

        private DateTime lastSaveTime;
        private const int debounceIntervalMs = 500;
        private bool needSave = false;
        private string selectedDrive = string.Empty;
        private string reportInput = new string('\0', 250);
        private bool isFrameworkWindowOpen = false;
        private string currentTab = "General";


        public PluginWindow() : base("      XIVV                                       ~  Xiv Voices by Arcsidian  ~") {
            Size = new Vector2(440, 650);
            initialSize = Size;
            SizeCondition = ImGuiCond.Always;
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize;
        }

        public Configuration Configuration {
            get => configuration;
            set {
                configuration = value;
            }
        }

        public DalamudPluginInterface PluginInterface { get; internal set; }

        internal IClientState ClientState {
            get => clientState;
            set {
                clientState = value;
                clientState.Login += ClientState_Login;
                clientState.Logout += ClientState_Logout;
            }
        }

        public Plugin PluginReference { get; internal set; }
        public event EventHandler OnMoveFailed;

        private void ClientState_Logout() {
        }

        private void ClientState_Login() {
        }

        public void InitializeImages()
        {

        }

        public override void Draw() {
            if (clientState.IsLoggedIn) {

                if(!configuration.Initialized)
                {
                    InitializationWindow();
                }
                else
                {
                    if (Updater.Instance.State.Count > 0)
                    {
                        UpdateWindow();
                    }
                    else
                    {
                        // The sidebar with the tab buttons
                        ImGui.BeginChild("Sidebar", new Vector2(50, 550), false);
                        var backupColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Button];
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Button] = new Vector4(0, 0, 0, 0);
                        if(currentTab == "General")
                        {
                            if (ImGui.ImageButton(this.PluginReference.GeneralSettingsActive.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "General";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("General Settings");
                        }
                        else
                        {
                            if (ImGui.ImageButton(this.PluginReference.GeneralSettings.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "General";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("General Settings");
                        }

                        if (currentTab == "Dialogue Settings")
                        {
                            if (ImGui.ImageButton(this.PluginReference.DialogueSettingsActive.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Dialogue Settings";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Dialogue Settings");
                        }
                        else
                        {
                            if (ImGui.ImageButton(this.PluginReference.DialogueSettings.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Dialogue Settings";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Dialogue Settings");
                        }

                        if (currentTab == "Audio Settings")
                        {
                            if (ImGui.ImageButton(this.PluginReference.AudioSettingsActive.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Audio Settings";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Audio Settings");
                        }
                        else
                        {
                            if (ImGui.ImageButton(this.PluginReference.AudioSettings.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Audio Settings";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Audio Settings");
                        }

                        if (currentTab == "Audio Logs")
                        {
                            if (ImGui.ImageButton(this.PluginReference.ArchiveActive.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Audio Logs";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Audio Logs");
                        }
                        else
                        {
                            if (ImGui.ImageButton(this.PluginReference.Archive.ImGuiHandle, new Vector2(42, 42)))
                                currentTab = "Audio Logs";
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("Audio Logs");
                        }

                        if (ImGui.ImageButton(this.PluginReference.Discord.ImGuiHandle, new Vector2(42, 42)))
                        {
                            Process process = new Process();
                            try
                            {
                                // true is the default, but it is important not to set it to false
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.FileName = "https://arcsidian.com/discord";
                                process.Start();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip("Join Our Discord Community");

                        if (ImGui.ImageButton(this.PluginReference.KoFi.ImGuiHandle, new Vector2(42, 42)))
                        {
                            Process process = new Process();
                            try
                            {
                                // true is the default, but it is important not to set it to false
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.FileName = "https://ko-fi.com/arcsidian";
                                process.Start();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip("Support the Project on Ko-Fi");

                        if (this.configuration.FrameworkActive) {
                            if (ImGui.ImageButton(this.PluginReference.Icon.ImGuiHandle, new Vector2(42, 42)))
                            {
                                isFrameworkWindowOpen = true;
                            }
                            Framework();
                        }
                        

                        ImGui.GetStyle().Colors[(int)ImGuiCol.Button] = backupColor;
                        ImGui.EndChild();

                        // Draw a vertical line separator
                        ImGui.SameLine();
                        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                        Vector2 lineStart = ImGui.GetCursorScreenPos() - new Vector2(0,10);
                        Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + 630);
                        drawList.AddLine(lineStart, lineEnd, ImGui.GetColorU32(ImGuiCol.WindowBg), 3f);
                        ImGui.SameLine(80);

                        // The content area where the selected tab's contents will be displayed
                        ImGui.BeginGroup();

                        if (currentTab == "General")
                        {
                            DrawGeneral();
                        }
                        else if (currentTab == "Dialogue Settings")
                        {
                            DrawSettings();
                        }
                        else if (currentTab == "Audio Settings")
                        {
                            AudioSettings();
                        }
                        else if (currentTab == "Audio Logs")
                        {
                            LogsSettings();
                        }

                        ImGui.EndGroup();
                    }
                }
                
                DrawErrors();
                //Close();
            } else {
                ImGui.TextUnformatted("Please login to access and configure settings.");
            }
        }

        private static readonly List<string> ValidTextureExtensions = new List<string>(){
          ".png",
        };

        private void Framework() {
            if (isFrameworkWindowOpen)
            {
                ImGui.SetNextWindowSize(new Vector2(400, 650));
                ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
                if (ImGui.Begin("Framework", ref isFrameworkWindowOpen, windowFlags))
                {
                    if (ImGui.BeginTabBar("FrameworkTabs"))
                    {
                        if (ImGui.BeginTabItem("General Framework"))
                        {
                            Framework_General();
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem(" Unknown Dialogues "))
                        {
                            Framework_Unknown();
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("    Audio Monitoring    "))
                        {
                            Framework_Audio();
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.End();
                }
            }
        }

        private void RequestSave()
        {
            Task.Run(() => {
                this.configuration.Save();
            });
            lastSaveTime = DateTime.Now;
        }

        private void DrawErrors() {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10f);
            ImGui.BeginChild("ErrorRegion", new Vector2(
            ImGui.GetContentRegionAvail().X,
            ImGui.GetContentRegionAvail().Y - 40f), false);
            if (managerNull) {
                ErrorMessage(managerNullMessage);
            }
            ImGui.EndChild();
        }


        private Vector2? GetSizeChange(float requiredY, float availableY, int Lines, Vector2? initial) {
            // Height
            if (availableY - requiredY * Lines < 1) {
                Vector2? newHeight = new Vector2(initial.Value.X, initial.Value.Y + requiredY * Lines);
                return newHeight;
            }
            return initial;
        }

        private void ErrorMessage(string message) {
            var requiredY = ImGui.CalcTextSize(message).Y + 1f;
            var availableY = ImGui.GetContentRegionAvail().Y;
            var initialH = ImGui.GetCursorPos().Y;
            ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), message);
            ImGui.PopTextWrapPos();
            var changedH = ImGui.GetCursorPos().Y;
            float textHeight = changedH - initialH;
            int textLines = (int)(textHeight / ImGui.GetTextLineHeight());

            // Check height and increase if necessarry
            if (availableY - requiredY * textLines < 1 && !SizeYChanged) {
                SizeYChanged = true;
                changedSize = GetSizeChange(requiredY, availableY, textLines, initialSize);
                Size = changedSize;
            }
        }

        internal class BetterComboBox {
            string _label = "";
            int _width = 0;
            int index = -1;
            int _lastIndex = 0;
            bool _enabled = true;
            string[] _contents = new string[1] { "" };
            public event EventHandler OnSelectedIndexChanged;
            public string Text { get { return index > -1 ? _contents[index] : ""; } }
            public BetterComboBox(string _label, string[] contents, int index, int width = 100) {
                if (Label != null) {
                    this._label = _label;
                }
                this._width = width;
                this.index = index;
                if (contents != null) {
                    this._contents = contents;
                }
            }

            public string[] Contents { get => _contents; set => _contents = value; }
            public int SelectedIndex { get => index; set => index = value; }
            public int Width { get => (_enabled ? _width : 0); set => _width = value; }
            public string Label { get => _label; set => _label = value; }
            public bool Enabled { get => _enabled; set => _enabled = value; }

            public void Draw() {
                if (_enabled) {
                    ImGui.SetNextItemWidth(_width);
                    if (_label != null && _contents != null) {
                        if (_contents.Length > 0) {
                            ImGui.Combo("##" + _label, ref index, _contents, _contents.Length);
                        }
                    }
                }
                if (index != _lastIndex) {
                    if (OnSelectedIndexChanged != null) {
                        OnSelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                _lastIndex = index;
            }
        }


        private void InitializationWindow()
        {
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Indent(90);
            ImGui.TextWrapped("Xiv Voices Initialization");
            ImGui.Unindent(90);
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Choose a working directory that will hold all the voice files in your computer, afterwards press \"Start\" to begin downloading Xiv Voices into your computer.");

            ImGui.Dummy(new Vector2(0, 20));
            ImGui.Indent(65);

            if (this.PluginReference.Logo != null)
                ImGui.Image(this.PluginReference.Logo.ImGuiHandle, new Vector2(200, 200));
            else
                ImGui.Dummy(new Vector2(200, 200));

            ImGui.TextWrapped("Working Directory is " + this.configuration.WorkingDirectory);
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Unindent(30);

            // Get available drives
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<string> drives = allDrives.Where(drive => drive.IsReady).Select(drive => drive.Name.Trim('\\')).ToList();
            string[] driveNames = drives.ToArray();
            int driveIndex = drives.IndexOf(selectedDrive);

            ImGui.Text("Select Drive:");
            if (ImGui.Combo("##Drives", ref driveIndex, driveNames, driveNames.Length))
            {
                selectedDrive = drives[driveIndex];
                this.configuration.WorkingDirectory = $"{selectedDrive}/XIV_Voices";
            }

            ImGui.Dummy(new Vector2(0, 50));

            if (ImGui.Button("Start Downloading Xiv Voices", new Vector2(260, 50)))
            {
                if(selectedDrive != string.Empty)
                    Updater.Instance.Check();
            }

        }

        private void UpdateWindow()
        {
            if (Updater.Instance.State.Contains(1))
            {
                ImGui.Dummy(new Vector2(0, 10));
                ImGui.TextWrapped("Checking Server Manifest...");
            }

            if (Updater.Instance.State.Contains(2))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("Checking Local Manifest...");
            }

            if (Updater.Instance.State.Contains(3))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("Checking Xiv Voices Tools...");
            }

            if (Updater.Instance.State.Contains(-1))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("Error: Unable to load Manifests");
            }

            if (Updater.Instance.State.Contains(4))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("Xiv Voices Tools are Ready.");
            }

            if (Updater.Instance.State.Contains(5))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("Xiv Voices Tools Missing, Downloading..");
            }

            if (Updater.Instance.State.Contains(6))
            {
                ImGui.Dummy(new Vector2(0, 5));
                float progress = Updater.Instance.ToolsDownloadState / 100.0f;
                ImGui.ProgressBar(progress, new Vector2(-1, 0), $"{Updater.Instance.ToolsDownloadState}% Complete");
                ImGui.Dummy(new Vector2(0, 5));
            }

            if (Updater.Instance.State.Contains(7))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("All Voice Files are Up to Date");
            }

            if (Updater.Instance.State.Contains(8))
            {
                ImGui.Dummy(new Vector2(0, 5));
                ImGui.TextWrapped("There is a new update, downloading...");
                if (Updater.Instance.State.Contains(9))
                {
                    ImGui.SameLine();
                    ImGui.Text(" " +Updater.Instance.DataDownloadCount + " files left");
                }
                ImGui.Dummy(new Vector2(0, 5));
            }

            if (Updater.Instance.State.Contains(9))
            {
                foreach (var item in Updater.Instance.DownloadInfoState)
                {
                    ImGui.ProgressBar(item.percentage, new Vector2(-1, 0), $"{item.file} {item.status}");
                }
            }

            if (Updater.Instance.State.Contains(10))
            {
                ImGui.TextWrapped("Done Updating.");
            }

        }

        private void DrawGeneral() {
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Indent(65);
            
            if (this.PluginReference.Logo != null)
                ImGui.Image(this.PluginReference.Logo.ImGuiHandle, new Vector2(200, 200));
            else
                ImGui.Dummy(new Vector2(200, 200));

            // Working Directory
            ImGui.TextWrapped("Working Directory is " + this.configuration.WorkingDirectory);
            ImGui.Dummy(new Vector2(0, 10));

            // Data
            ImGui.Indent(10);
            ImGui.TextWrapped("NPCs:");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green color
            ImGui.TextWrapped(XivEngine.Instance.Database.Data["npcs"]);
            ImGui.PopStyleColor();
            ImGui.SameLine();

            ImGui.TextWrapped(" Voices:");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green color
            ImGui.TextWrapped(XivEngine.Instance.Database.Data["voices"]);
            ImGui.PopStyleColor();

            // Update Button
            ImGui.Unindent(65);
            ImGui.Dummy(new Vector2(0, 10));
            if (ImGui.Button("Click here to download the latest Voice Files", new Vector2(330, 60)))
            {
                Updater.Instance.Check();
            }

            // Xiv Voices Enabled
            ImGui.Dummy(new Vector2(0, 15));
            var activeValue = this.Configuration.Active;
            if (ImGui.Checkbox("##xivVoicesActive", ref activeValue)){
                this.configuration.Active = activeValue;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Xiv Voices Enabled");

            // Auto Update Enabled
            ImGui.Dummy(new Vector2(0, 8));
            var autoUpdate = this.Configuration.AutoUpdate;
            if (ImGui.Checkbox("##autoUpdate", ref autoUpdate))
            {
                this.configuration.AutoUpdate = autoUpdate;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Auto Update Enabled");

            // Xiv Voices Enabled
            ImGui.Dummy(new Vector2(0, 8));
            var reports = this.Configuration.Reports;
            if (ImGui.Checkbox("##reports", ref reports))
            {
                this.configuration.Reports = reports;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Report Missing Dialogues Automatically");
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.25f, 0.25f, 1.0f));
            ImGui.Text("( English dialogues only, do not enable for other languages )");
            ImGui.PopStyleColor();
            

            /*
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.LabelText("##Label", "Websocket Settings");
            ImGui.InputText("##port", ref _port, 5);
            ImGui.SameLine();
            if (ImGui.Button("Restart"))
            {
                this.configuration.Port = _port;
                needSave = true;
                lastChangeTime = DateTime.Now;
                PluginReference.webSocketServer.Stop();
                PluginReference.webSocketServer.Connect();
            }
            ImGui.TextWrapped(this.configuration.WebsocketStatus);
            ImGui.Dummy(new Vector2(0, 10));
            */

            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false; // Reset save flag after saving
            }
        }

        private void DrawSettings() {

            // Chat Settings ----------------------------------------------
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Chat Settings");
            ImGui.Dummy(new Vector2(0, 10));


            // SayEnabled
            var sayEnabled = this.Configuration.SayEnabled;
            if (ImGui.Checkbox("##sayEnabled", ref sayEnabled))
            {
                this.configuration.SayEnabled = sayEnabled;
                needSave = true;

            };
            ImGui.SameLine();
            ImGui.Text("Say Enabled");

            // TellEnabled
            var tellEnabled = this.Configuration.TellEnabled;
            if (ImGui.Checkbox("##tellEnabled", ref tellEnabled))
            {
                this.configuration.TellEnabled = tellEnabled;
                needSave = true;

            };
            ImGui.SameLine();
            ImGui.Text("Tell Enabled");

            // ShoutEnabled
            var shoutEnabled = this.Configuration.ShoutEnabled;
            if (ImGui.Checkbox("##shoutEnabled", ref shoutEnabled))
            {
                this.configuration.ShoutEnabled = shoutEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Shout/Yell Enabled");

            // PartyEnabled
            var partyEnabled = this.Configuration.PartyEnabled;
            if (ImGui.Checkbox("##partyEnabled", ref partyEnabled))
            {
                this.configuration.PartyEnabled = partyEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Party Enabled");

            // FreeCompanyEnabled
            var freeCompanyEnabled = this.Configuration.FreeCompanyEnabled;
            if (ImGui.Checkbox("##freeCompanyEnabled", ref freeCompanyEnabled))
            {
                this.configuration.FreeCompanyEnabled = freeCompanyEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Free Company Enabled");

            // BattleDialoguesEnabled
            var battleDialoguesEnabled = this.Configuration.BattleDialoguesEnabled;
            if (ImGui.Checkbox("##battleDialoguesEnabled", ref battleDialoguesEnabled))
            {
                this.configuration.BattleDialoguesEnabled = battleDialoguesEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Battle Dialogues Enabled");

            // RetainersEnabled
            var retainersEnabled = this.Configuration.RetainersEnabled;
            if (ImGui.Checkbox("##retainersEnabled", ref retainersEnabled))
            {
                this.configuration.RetainersEnabled = retainersEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Retainers Enabled");

            // Bubble Settings ----------------------------------------------
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Bubble Settings");
            ImGui.Dummy(new Vector2(0, 10));

            // BubblesEnabled
            var bubblesEnabled = this.Configuration.BubblesEnabled;
            if (ImGui.Checkbox("##bubblesEnabled", ref bubblesEnabled))
            {
                this.configuration.BubblesEnabled = bubblesEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Chat Bubbles Enabled");

            ImGui.Indent(28);
                var nullcheck = false;
                // BubblesEverywhere
                var bubblesEverywhere = this.Configuration.BubblesEverywhere;
                if (this.Configuration.BubblesEnabled)
                {
                    if (ImGui.Checkbox("##bubblesEverywhere", ref bubblesEverywhere))
                    {
                        if (bubblesEverywhere)
                        {
                            this.configuration.BubblesEverywhere = bubblesEverywhere;
                            this.configuration.BubblesInSafeZones = !bubblesEverywhere;
                            this.configuration.BubblesInBattleZones = !bubblesEverywhere;
                            needSave = true;
                        }
                    };
                }
                else
                    ImGui.Checkbox("##null", ref nullcheck);
                ImGui.SameLine();
                ImGui.Text("Enable Bubbles Everywhere");

                // BubblesInSafeZones
                var bubblesOutOfBattlesOnly = this.Configuration.BubblesInSafeZones;
                if (this.Configuration.BubblesEnabled)
                {
                    if (ImGui.Checkbox("##bubblesOutOfBattlesOnly", ref bubblesOutOfBattlesOnly))
                    {
                        if(bubblesOutOfBattlesOnly)
                        {
                            this.configuration.BubblesEverywhere = !bubblesOutOfBattlesOnly;
                            this.configuration.BubblesInSafeZones = bubblesOutOfBattlesOnly;
                            this.configuration.BubblesInBattleZones = !bubblesOutOfBattlesOnly;
                            needSave = true;
                        }
                    };
                }
                else
                    ImGui.Checkbox("##null", ref nullcheck);
                ImGui.SameLine();
                ImGui.Text("Only Enable Chat Bubbles In Safe Zones");

                // BubblesInBattleZones
                var bubblesInBattlesOnly = this.Configuration.BubblesInBattleZones;
                if (this.Configuration.BubblesEnabled)
                {
                    if (ImGui.Checkbox("##bubblesInBattlesOnly", ref bubblesInBattlesOnly))
                    {
                        if (bubblesInBattlesOnly)
                        {
                            this.configuration.BubblesEverywhere = !bubblesInBattlesOnly;
                            this.configuration.BubblesInSafeZones = !bubblesInBattlesOnly;
                            this.configuration.BubblesInBattleZones = bubblesInBattlesOnly;
                            needSave = true;
                        }
                    };
                }
                else
                    ImGui.Checkbox("##null", ref nullcheck);
                ImGui.SameLine();
                ImGui.Text("Only Enable Chat Bubbles in Battle Zones");


            ImGui.Unindent(28);

            // Other Settings ----------------------------------------------

            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Other Settings");
            ImGui.Dummy(new Vector2(0, 10));

            // ReplaceVoicedARRCutscenes
            var replaceVoicedARRCutscenes = this.Configuration.ReplaceVoicedARRCutscenes;
            if (ImGui.Checkbox("##replaceVoicedARRCutscenes", ref replaceVoicedARRCutscenes))
            {
                this.configuration.ReplaceVoicedARRCutscenes = replaceVoicedARRCutscenes;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Replace ARR Cutscenes");

            // SkipEnabled
            var skipEnabled = this.Configuration.SkipEnabled;
            if (ImGui.Checkbox("##interruptEnabled", ref skipEnabled))
            {
                this.configuration.SkipEnabled = skipEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Dialogue Skip Enabled");

            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false; // Reset save flag after saving
            }

        }

        private void AudioSettings()
        {
            // Mute Button -----------------------------------------------

            ImGui.Dummy(new Vector2(0, 20));
            var mute = this.Configuration.Mute;
            if (ImGui.Checkbox("##mute", ref mute))
            {
                this.configuration.Mute = mute;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Mute Enabled");

            // Lipsync Enabled -----------------------------------------------
            ImGui.Dummy(new Vector2(0, 10));
            var lipsyncEnabled = this.Configuration.LipsyncEnabled;
            if (ImGui.Checkbox("##lipsyncEnabled", ref lipsyncEnabled))
            {
                this.configuration.LipsyncEnabled = lipsyncEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Lipsync Enabled");

            // Volume Slider ---------------------------------------------

            ImGui.Dummy(new Vector2(0, 20));
            ImGui.TextWrapped("Volume Control");
            int volume = this.Configuration.Volume;
            if (ImGui.SliderInt("##volumeSlider", ref volume, 0, 100, volume.ToString()))
            {
                this.Configuration.Volume = volume;
                needSave = true;
            }
            ImGui.SameLine();
            ImGui.Text("Volume");

            // Speed Slider ---------------------------------------------

            ImGui.Dummy(new Vector2(0, 20));
            ImGui.TextWrapped("Speed Control");
            int speed = this.Configuration.Speed;
            if (ImGui.SliderInt("##speedSlider", ref speed, 75, 150, speed.ToString()))
            {
                this.Configuration.Speed = speed;
                needSave = true;
            }
            ImGui.SameLine();
            ImGui.Text("Speed");

            // Playback Engine  ---------------------------------------------
            ImGui.Dummy(new Vector2(0, 20));
            ImGui.TextWrapped("Playback Engine");
            string[] audioEngines = new string[] { "DirectSound", "Wasapi" }; 
            int currentEngine = this.Configuration.AudioEngine - 1;

            if (ImGui.Combo("##audioEngine", ref currentEngine, audioEngines, audioEngines.Length))
            {
                this.Configuration.AudioEngine = currentEngine + 1;
                needSave = true;
            }
            ImGui.SameLine();
            ImGui.Text("Engine");


            // Speed Slider ---------------------------------------------

            ImGui.Dummy(new Vector2(0, 30));
            ImGui.Separator();

            // Local AI Settings Settings ----------------------------------------------

            ImGui.Dummy(new Vector2(0, 20));
            var localTTSEnabled = this.Configuration.LocalTTSEnabled;
            if (ImGui.Checkbox("##localTTSEnabled", ref localTTSEnabled))
            {
                this.configuration.LocalTTSEnabled = localTTSEnabled;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Local TTS Enabled");

            ImGui.Indent(20);
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Text("Local TTS Ungendered Voice:");
            ImGui.SameLine();
            var localTTSUngendered = this.Configuration.LocalTTSUngendered;
            string[] genders = { "Male", "Female" };
            ImGui.SetNextItemWidth(120);
            if (ImGui.Combo("##localTTSUngendered", ref localTTSUngendered, genders, genders.Length))
            {
                this.Configuration.LocalTTSUngendered = localTTSUngendered;
                needSave = true;
            }
            ImGui.Unindent(20);

            // Polly Settings ----------------------------------------------

            //ImGui.Dummy(new Vector2(0, 20));
            //ImGui.TextWrapped("Poly Settings (soon)");
            //ImGui.Dummy(new Vector2(0, 10));


            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false; // Reset save flag after saving
            }

        }

        private void LogsSettings()
        {
            if (!configuration.Active)
            {
                ImGui.Dummy(new Vector2(0, 20));
                ImGui.TextWrapped("Xiv Voices is Disabled");
                ImGui.Dummy(new Vector2(0, 10));
            }
            else
            {
                // Begin a scrollable region
                if (ImGui.BeginChild("ScrollingRegion", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {
                    
                    foreach (var item in PluginReference.audio.AudioInfoState)
                    {
                        // Show Dialogue Details (Name: Sentence)
                        ImGui.TextWrapped($"{item.data.Speaker}: {item.data.Sentence}");

                        // Show Player Progress Bar
                        int progressSize = 208;
                        if (item.type == "xivv")
                            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.0f, 0.7f, 0.0f, 1.0f)); // RGBA: Full green
                        else if (item.type == "empty")
                        {
                            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // RGBA: Full green
                            progressSize = 265;
                        }
                        ImGui.ProgressBar(item.percentage, new Vector2(progressSize, 24), $"{item.state}");
                        if (item.type == "xivv" || item.type == "empty")
                            ImGui.PopStyleColor();

                        // Show Report Button
                        ImGui.SameLine();
                        string reportTitle = "Report";
                        if(item.type != "xivv")
                            reportTitle = "Mute";
                        if (ImGui.Button($"{reportTitle}##report{item.id}", new Vector2(50, 24)))
                        {
                            reportInput = new string('\0', 250);
                            ImGui.OpenPopup($"ReportDialogue##{item.id}");
                        }

                        // Report Popup
                        bool open = true;
                        ImGui.SetNextWindowSizeConstraints(new Vector2(350, 350), new Vector2(350, float.MaxValue));
                        if (ImGui.BeginPopupModal($"ReportDialogue##{item.id}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
                        {
                            ImGui.Dummy(new Vector2(0, 5));
                            ImGui.Text($"Speaker: {item.data.Speaker}");
                            ImGui.Dummy(new Vector2(0, 5));
                            ImGui.TextWrapped($"Sentence: {item.data.Sentence}");
                            ImGui.Dummy(new Vector2(0, 20));

                            if (item.type == "xivv")
                            {
                                ImGui.TextWrapped("Tell me why this dialogue needs to be redone or muted");
                                ImGui.Dummy(new Vector2(0, 5));
                                ImGui.InputTextMultiline($"##input_{item.id}", ref reportInput, 250, new Vector2(335, 100));
                                ImGui.Dummy(new Vector2(0, 5));
                                if (ImGui.Button("Ask to Redo", new Vector2(335, 25)))
                                {
                                    //PluginReference.webSocketServer.SendMessage("input:" + reportInput);
                                    PluginReference.xivEngine.ReportRedoToArc(item.data, reportInput);
                                    ImGui.CloseCurrentPopup();
                                }
                                ImGui.Dummy(new Vector2(0, 2));
                            }
                        
                            if (ImGui.Button("Ask to Mute", new Vector2(335, 25)))
                            {
                                PluginReference.xivEngine.ReportMuteToArc(item.data, reportInput);
                                PluginReference.xivEngine.IgnoredDialogues.Add(item.data.Speaker + item.data.Sentence);
                                ImGui.CloseCurrentPopup();
                            }
                            ImGui.Dummy(new Vector2(0, 2));
                            if (ImGui.Button("Close", new Vector2(335, 25)))
                                ImGui.CloseCurrentPopup();
                            ImGui.Dummy(new Vector2(0, 2));
                            ImGui.EndPopup();
                        }

                        if (item.type != "empty")
                        {
                            // Show Play and Stop Buttons
                            ImGui.SameLine();
                            if (item.state == "playing")
                            {
                                if (ImGui.Button("Stop", new Vector2(50, 24)))
                                    PluginReference.audio.StopAudio();
                            }
                            else
                            {
                                if (ImGui.Button($"Play##{item.id}", new Vector2(50, 24)))
                                {
                                    PluginReference.audio.StopAudio();
                                    PluginReference.xivEngine.AddToQueue(item.data);
                                }
                            }
                        }
                        ImGui.Dummy(new Vector2(0, 10));

                    }
                }
            }
        }

        private void Framework_General()
        {
            ImGui.Dummy(new Vector2(0, 10));
            var frameworkOnline = this.Configuration.FrameworkOnline;
            if (ImGui.Checkbox("##frameworkOnline", ref frameworkOnline))
            {
                this.configuration.FrameworkOnline = frameworkOnline;
                needSave = true;
            };
            ImGui.SameLine();
            ImGui.Text("Framework Enabled");

            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false;
            }
        }

        private void Framework_Unknown()
        {
            ImGui.Dummy(new Vector2(0, 10));
            if (ImGui.Button($"Load Unknown List##loadUnknownList", new Vector2(385, 25)))
            {
                XivEngine.Instance.UnknownList_Load();
            }

            ImGui.Dummy(new Vector2(0, 10));

            foreach (string item in XivEngine.Instance.Audio.unknownQueue)
            {
                if (ImGui.BeginChild("unknownList"+item, new Vector2(275, 50), true))
                {
                    ImGui.Dummy(new Vector2(0, 5));

                    ImGui.Text(item);
                    ImGui.EndChild();
                }
                ImGui.SameLine();
                ImGui.SameLine();
                if (ImGui.Button("Run##unknowButton" + item, new Vector2(45, 50)))
                {
                    XivEngine.Instance.Database.Framework.Run(item);
                }
                ImGui.SameLine();
                if (ImGui.Button("Once##unknowButton" + item, new Vector2(45, 50)))
                {
                    XivEngine.Instance.Database.Framework.Run(item, true);
                }
            }

            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false;
            }
        }

        private void Framework_Audio()
        {
            ImGui.Dummy(new Vector2(0, 10));
            if (ImGui.BeginChild("frameworkAudio", new Vector2(385, 90), true))
            {
                // Player Name
                ImGui.Dummy(new Vector2(130, 0));
                ImGui.SameLine();
                string playerName = XivEngine.Instance.Database.PlayerName;
                ImGui.SetNextItemWidth(112);
                if (ImGui.InputText("##playerName", ref playerName, 100))
                {
                    XivEngine.Instance.Database.PlayerName = playerName;
                    needSave = true;
                }
                ImGui.SameLine();
                var forcePlayerName = XivEngine.Instance.Database.ForcePlayerName;
                if (ImGui.Checkbox("##forcePlayerName", ref forcePlayerName))
                {
                    XivEngine.Instance.Database.ForcePlayerName = forcePlayerName;
                    needSave = true;
                };
                ImGui.SameLine();
                ImGui.Text("Force Name");

                // Full Sentence
                ImGui.Dummy(new Vector2(0, 3));
                ImGui.Dummy(new Vector2(3, 0));
                ImGui.SameLine();
                string wholeSentence = XivEngine.Instance.Database.WholeSentence;
                ImGui.SetNextItemWidth(240);
                if (ImGui.InputText("##wholeSentence", ref wholeSentence, 200))
                {
                    XivEngine.Instance.Database.WholeSentence = wholeSentence;
                    needSave = true;
                }
                ImGui.SameLine();
                var forceWholeSentence = XivEngine.Instance.Database.ForceWholeSentence;
                if (ImGui.Checkbox("##forceWholeSentence", ref forceWholeSentence))
                {
                    XivEngine.Instance.Database.ForceWholeSentence = forceWholeSentence;
                    needSave = true;
                };
                ImGui.SameLine();
                ImGui.Text("Sentence");

                ImGui.EndChild();
            }

            foreach (var item in PluginReference.audio.AudioInfoState.Take(6))
            {
                // Show Dialogue Details (Name: Sentence)
                if (ImGui.BeginChild(item.id, new Vector2(385, 43), false))
                {
                    float textHeight = ImGui.CalcTextSize($"{item.data.Speaker}: {item.data.Sentence}", 340.0f).Y;
                    float paddingHeight = Math.Max(35 - textHeight, 0);
                    ImGui.Dummy(new Vector2(1, 3));
                    if (paddingHeight > 3)
                        ImGui.Dummy(new Vector2(1, paddingHeight - 3));

                    ImGui.TextWrapped($"{item.data.Speaker}: {item.data.Sentence}");
                    ImGui.EndChild();
                }

                // Show Player Progress Bar
                int progressSize = 265;
                if (item.type == "xivv")
                    ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.0f, 0.7f, 0.0f, 1.0f)); // RGBA: Full green
                else if (item.type == "empty")
                {
                    ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // RGBA: Full green
                    progressSize = 380;
                }
                ImGui.ProgressBar(item.percentage, new Vector2(progressSize, 24), $"{item.state}");
                if (item.type == "xivv" || item.type == "empty")
                    ImGui.PopStyleColor();


                if (item.type != "empty")
                {
                    ImGui.SameLine();

                    // Show Report Button
                    if (ImGui.Button($"Redo##redo{item.id}", new Vector2(50, 24)))
                    {
                        XivEngine.Instance.Database.Framework.Process(item.data);
                    }

                    // Show Play and Stop Buttons
                    ImGui.SameLine();
                    if (item.state == "playing")
                    {
                        if (ImGui.Button("Stop", new Vector2(50, 24)))
                            PluginReference.audio.StopAudio();
                    }
                    else
                    {
                        if (ImGui.Button($"Play##{item.id}", new Vector2(50, 24)))
                        {
                            PluginReference.audio.StopAudio();
                            PluginReference.xivEngine.AddToQueue(item.data);
                        }
                    }
                }

            }

            ImGui.Dummy(new Vector2(1, 1));
            ImGui.TextWrapped($"Files: {XivEngine.Instance.Database.Framework.Queue.Count}");

            // Saving Process
            if (needSave && (DateTime.Now - lastSaveTime).TotalMilliseconds > debounceIntervalMs)
            {
                RequestSave();
                needSave = false;
            }
        }


        public class MessageEventArgs : EventArgs {
            string message;

            public string Message { get => message; set => message = value; }
        }
    }
}
