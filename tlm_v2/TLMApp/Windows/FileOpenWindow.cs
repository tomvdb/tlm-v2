using IconFonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace tlm_v2.TLMApp.Windows
{

    public delegate void NewDataPlaybackFile(string FileName);

    public class FileEntry
    {
        public int type;    // 0 is file, 1 is folder
        public string FileName;
        public long FileSize;
        public DateTime FileModified;

        public FileEntry(int type, string FileName, long FileSize, DateTime FileModified)
        {
            this.type = type;
            this.FileName = FileName;
            this.FileSize = FileSize;
            this.FileModified = FileModified;
        }
    }

    public class FileOpenWindow
    {
        public string CurrentPath 
        { 
            get => _currentPath; 
            set
            {
                _currentPath = value;
                _update = true;
            } 
        }

        private bool _update = false;
        private string _id;
        private string _currentPath = "";
        private string _selectedFile = "";

        private bool _updateSort = false;

        private List<FileEntry> _fileEntries = new List<FileEntry>();

        public event NewDataPlaybackFile? OnNewDataPlayFile;

        public FileOpenWindow(string id, string currentPath) 
        {
            _id = id;            
            _currentPath = currentPath;

            _updateAll();
        }


        private void _updateAll()
        {
            string[] _files = new string[0];
            string[] _directories = new string[0];


            _fileEntries.Clear();

            _directories = Directory.GetDirectories(_currentPath);

            for (int x = 0; x < _directories.Length; x++)
            {
                string fileName = _directories[x];
                FileInfo fileInfo = new FileInfo(fileName);

                _fileEntries.Add(new FileEntry(1, fileName, 0, fileInfo.LastWriteTime));
            }


            _files = Directory.GetFiles(_currentPath);

            for (int x = 0; x < _files.Length; x++)
            {
                string fileName = _files[x];
                FileInfo fileInfo = new FileInfo(fileName);

                _fileEntries.Add(new FileEntry(0, fileName, fileInfo.Length, fileInfo.LastWriteTime));
            }

            _update = false;
            _updateSort = true;
        }

        private void _SelectKissFile(string FilePath)
        {
            OnNewDataPlayFile?.Invoke(FilePath);
        }

        public void Render()
        {
            var io = ImGui.GetIO();

            bool popen = true;

            ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(ImGui.GetFontSize() * 40f, ImGui.GetFontSize() * 20f));

            if (ImGui.BeginPopupModal(FontAwesome6.FolderOpen + " " + _id, ref popen, ImGuiWindowFlags.NoResize))
            {

                ImGui.Text(CurrentPath);
                ImGui.Separator();

                if (ImGui.BeginChild("##ScrollingRegion", new System.Numerics.Vector2(0, -2.3f * (ImGui.CalcTextSize("FF").Y + ImGui.GetStyle().FramePadding.Y * 2.0f)), false, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    if (ImGui.BeginTable("##Table", 3, ImGuiTableFlags.Sortable))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);

                        // does our data require sorting ?
                        ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

                        if (sortSpecs.SpecsDirty || _updateSort)
                        {
                            Console.WriteLine("Sorting Required");

                            switch (sortSpecs.Specs.ColumnIndex)
                            {
                                case 0:
                                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                                        _fileEntries.Sort((x, y) => x.FileName.CompareTo(y.FileName));
                                    else
                                        _fileEntries.Sort((y, x) => x.FileName.CompareTo(y.FileName));

                                    break;

                                case 1:  if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                                            _fileEntries.Sort((x, y) => x.FileSize.CompareTo(y.FileSize));
                                         else
                                            _fileEntries.Sort((y, x) => x.FileSize.CompareTo(y.FileSize));

                                    break;

                                case 2:
                                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                                        _fileEntries.Sort((x, y) => x.FileModified.CompareTo(y.FileModified));
                                    else
                                        _fileEntries.Sort((y, x) => x.FileModified.CompareTo(y.FileModified));

                                    break;
                            }

                            sortSpecs.SpecsDirty = false;
                            _updateSort = false;
                        }

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TableHeader("Filename");
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TableHeader("Size (b)");
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TableHeader("Modified");

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);

                        if (ImGui.Selectable("..", false, ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.SpanAllColumns))
                        {
                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                Directory.SetCurrentDirectory(@"..");
                                CurrentPath = Directory.GetCurrentDirectory();
                            }
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text("");
                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text("");

                        // show directories
                        for (int i = 0; i < _fileEntries.Count(); i++)
                        {

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);

                            if (_fileEntries[i].type == 1)
                                ImGui.TextColored(new Vector4(0.8f, 0f, 0f, 1f), FontAwesome6.Folder);
                            else
                            {
                                ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 2f, ImGui.GetCursorPosY()));
                                ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), FontAwesome6.File);
                            }

                            ImGui.SameLine(20f);
                            if (ImGui.Selectable(" " + Path.GetFileName(_fileEntries[i].FileName), false, ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.SpanAllColumns))
                            {
                                if (_fileEntries[i].type == 1)
                                {
                                    if (ImGui.IsMouseDoubleClicked(0))
                                    {
                                        Directory.SetCurrentDirectory(_fileEntries[i].FileName);
                                        CurrentPath = Directory.GetCurrentDirectory();
                                    }

                                }

                                if (_fileEntries[i].type == 0)
                                {
                                    if (ImGui.IsMouseDoubleClicked(0))
                                    {
                                        ImGui.CloseCurrentPopup();
                                        _SelectKissFile(_fileEntries[i].FileName);
                                    }
                                    else
                                    {
                                        _selectedFile = _fileEntries[i].FileName;
                                    }
                                }

                            }

                            ImGui.TableSetColumnIndex(1);
                            if (_fileEntries[i].type == 0)
                                ImGui.Text(_fileEntries[i].FileSize.ToString());
                            else
                                ImGui.Text("");
                            ImGui.TableSetColumnIndex(2);
                            ImGui.Text(_fileEntries[i].FileModified.ToString());

                        }

                        ImGui.EndTable();
                    }
                    ImGui.EndChild();
                }

                ImGui.Separator();
                ImGui.Text(_selectedFile);

                if (ImGui.Button("Select File"))
                {
                    if (_selectedFile.Length > 0)
                    {
                        ImGui.CloseCurrentPopup();
                        _SelectKissFile(_selectedFile);
                        //OpenKissFile(selectedFile);
                    }

                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            if (_update)
            {
                _updateAll();
            }

        }

    }
}
